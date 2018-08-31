package net.azurewebsites.expenses_by_klaudia.expensesapp;


import android.accounts.Account;
import android.accounts.AccountManager;
import android.app.Fragment;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.google.gson.Gson;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.AppAuthenticator;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.DateHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.model.GetMembersResponseDto;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;

import java.net.HttpURLConnection;


/**
 * A simple {@link Fragment} subclass.
 */
public class HouseholdFragment extends Fragment {

    private static final String ARG_SECTION_NUMBER = "section_number";
    static final String ARG_LOGIN = "login";
    static final String ARG_KEY = "key";

    View mFormView;
    View mProgressView;
    View mFragment;
    TextView mTxtNoMembersInfo;
    Button mBtnGoToInvitations;
    RecyclerView mListOfMembersView;
    GetMembersTask mGetMembersTask;

    public static HouseholdFragment newInstance(int sectionNumber, String login, String key) {
        HouseholdFragment fragment = new HouseholdFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_SECTION_NUMBER, sectionNumber);
        args.putString(ARG_LOGIN, login);
        args.putString(ARG_KEY, key);
        fragment.setArguments(args);
        return fragment;
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        mFragment = inflater.inflate(R.layout.fragment_household, container, false);
        mFormView = mFragment.findViewById(R.id.household_form);
        mProgressView = mFragment.findViewById(R.id.household_progress);

        mTxtNoMembersInfo = mFragment.findViewById(R.id.household_info_no_members);
        mBtnGoToInvitations = mFragment.findViewById(R.id.household_btn_go_to_invitations);
        mBtnGoToInvitations.setOnClickListener(x -> goToInvitations());
        mListOfMembersView = mFragment.findViewById(R.id.household_list_of_members);

        boolean shouldStartRefreshingMembers = doesUserBelongToHousehold();
        if (shouldStartRefreshingMembers) {
            attemptFillMembersList();

            mTxtNoMembersInfo.setVisibility(View.GONE);
            mBtnGoToInvitations.setVisibility(View.GONE);
        } else {
            mTxtNoMembersInfo.setVisibility(View.VISIBLE);
            mBtnGoToInvitations.setVisibility(View.VISIBLE);
        }
        
        return mFragment;
    }

    private void attemptFillMembersList() {
        mGetMembersTask = new GetMembersTask();
        mGetMembersTask.execute();
    }

    private boolean doesUserBelongToHousehold() {
        AccountManager accountManager = AccountManager.get(getContext());
        String accountName = getArguments().getString(ARG_LOGIN);
        String accountType = getString(R.string.account_type);
        Account account = new Account(accountName, accountType);
        return accountManager.getUserData(account, AppAuthenticator.ACCOUNT_BELONGS_TO_HOUSEHOLD).equals(AppAuthenticator.ACCOUNT_VALUE_TRUE);
    }

    private void goToInvitations() {
        HomepageActivity activity = (HomepageActivity)getActivity();
        activity.selectFragment(3);
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mFormView, mProgressView);
    }

    public class GetMembersTask extends AsyncTask<Void, Void, HttpResponse<GetMembersResponseDto>> {

        private final Repository mRepository;

        public final static String PREF_MEMBERS_DATA = "PREF_MEMBERS_DATA";
        public final static String PREF_MEMBERS_DATA_LAST_TIME = "PREF_MEMBERS_DATA";

        public GetMembersTask() {
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected void onPreExecute() {
            showProgress(true);
        }

        @Override
        @SuppressWarnings("unchecked")
        protected HttpResponse<GetMembersResponseDto> doInBackground(Void... voids) {
            SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getContext());

            String lastTimeStr = sp.getString(PREF_MEMBERS_DATA_LAST_TIME, "");
            boolean shouldCheck = TextUtils.isEmpty(lastTimeStr) || DateHelper.shouldRefresh(lastTimeStr);

            if (shouldCheck) {

                AccountManager accountManager = AccountManager.get(getContext());
                String accountName = getArguments().getString(ARG_LOGIN);
                String accountType = getString(R.string.account_type);
                Account account = new Account(accountName, accountType);
                String householdId = accountManager.getUserData(account, AppAuthenticator.ACCOUNT_HOUSEHOLD_ID);
                String key = getArguments().getString(ARG_KEY);
                HttpResponse<GetMembersResponseDto> response = HttpResponder.askForDtoFromJson(() -> {
                    try {
                        return mRepository.getHouseholdsRepository().getMemebers(householdId, key);
                    } catch (Exception ex) {
                        return null;
                    }
                }, GetMembersResponseDto.class);


                if (response.getCode() == HttpURLConnection.HTTP_OK
                        && response.getObject() != null) {

                    Gson gson = new Gson();
                    String serialized = gson.toJson(response.getObject());
                    String date = DateHelper.getCurrentDate();
                    sp.edit().putString(PREF_MEMBERS_DATA_LAST_TIME, date)
                            .putString(PREF_MEMBERS_DATA, serialized)
                            .apply();
                }


                return response;
            }

            return new HttpResponse<>(HttpURLConnection.HTTP_OK, null);
        }

        @Override
        protected void onPostExecute(final HttpResponse<GetMembersResponseDto> success) {
            if (success != null && success.getCode() == HttpURLConnection.HTTP_OK) {
                if (success.getObject() != null) {

                    RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(getContext());
                    mListOfMembersView.setLayoutManager(layoutManager);
                    MemberListItemAdapter adapter = new MemberListItemAdapter(success.getObject().Members);
                    mListOfMembersView.setAdapter(adapter);
                }
            } else {
                Toast.makeText(getContext(), getString(R.string.error_cant_download_members), Toast.LENGTH_SHORT).show();
            }
            showProgress(false);
        }

        @Override
        protected void onCancelled() {
            mGetMembersTask = null;
            showProgress(false);
        }
    }
}

package net.azurewebsites.expenses_by_klaudia.expensesapp;


import android.accounts.Account;
import android.accounts.AccountManager;
import android.app.Fragment;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.AppAuthenticator;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.DateHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.model.GetNewMessagesResponseDto;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;
import net.azurewebsites.expenses_by_klaudia.repository.RepositoryException;

import java.lang.reflect.Type;
import java.net.HttpURLConnection;
import java.text.ParseException;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.Date;
import java.util.List;
import java.util.function.Consumer;
import java.util.function.Supplier;
import java.util.regex.Matcher;
import java.util.regex.Pattern;


public class InvitationsFragment extends Fragment {

    private static final String ARG_SECTION_NUMBER = "section_number";
    static final String ARG_LOGIN = "login";
    static final String ARG_KEY = "key";

    View mFormView;
    View mProgressView;
    View mFragment;
    TextView mTxtInfoAboutInvitation;
    Button mBtnAcceptInvitation;
    EditText mTxtPersonToInviteLogin;
    Button mBtnInvite;
    GetInvitationsTask mGetInvitationsTask;
    List<GetNewMessagesResponseDto> mListOfGetNewMessagesResponseDto;
    AcceptInvitationTask mAcceptInvitationTask;
    InviteToHouseholdTask mInviteToHouseholdTask;
    Button mBtnRejectInvitation;

    public Consumer<String> notifyHouseholdChanged;

    public static InvitationsFragment newInstance(int sectionNumber, String login, String key) {
        InvitationsFragment fragment = new InvitationsFragment();
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
        mFragment = inflater.inflate(R.layout.fragment_invitations, container, false);
        mFormView = mFragment.findViewById(R.id.invitations_form);
        mProgressView = mFragment.findViewById(R.id.invitations_progress);

        mTxtInfoAboutInvitation = mFragment.findViewById(R.id.invitations_pending_invitation);
        mBtnAcceptInvitation = mFragment.findViewById(R.id.invitations_accept);
        mTxtPersonToInviteLogin = mFragment.findViewById(R.id.invitations_login);
        mBtnInvite = mFragment.findViewById(R.id.invitations_invite);
        mBtnInvite.setOnClickListener(x -> attemptInvite());
        mBtnRejectInvitation = mFragment.findViewById(R.id.invitations_reject);

        ValidationUseCases useCases = new ValidationUseCases(this::getString);
        BindRulesToActivityHelper b = new BindRulesToActivityHelper(mBtnInvite, getContext());
        b.add(mTxtPersonToInviteLogin, useCases::isLoginValid);
        b.validateForm();

        mGetInvitationsTask = new GetInvitationsTask();
        mGetInvitationsTask.execute();

        return mFragment;
    }

    private void attemptInvite() {
        if (mInviteToHouseholdTask != null)
            return;
        mInviteToHouseholdTask = new InviteToHouseholdTask();
        mInviteToHouseholdTask.execute();
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mFormView, mProgressView);
    }

    public class GetInvitationsTask extends AsyncTask<Void, Void, HttpResponse<List<GetNewMessagesResponseDto>>> {

        private final Repository mRepository;

        public final static String PREF_INVITATIONS_DATA = "PREF_INVITATIONS_DATA";
        public final static String PREF_INVITATIONS_DATA_LAST_TIME = "PREF_INVITATIONS_DATA_LAST_TIME";

        public GetInvitationsTask() {
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected void onPreExecute() {
            showProgress(true);
        }


        @Override
        @SuppressWarnings("unchecked")
        protected HttpResponse<List<GetNewMessagesResponseDto>> doInBackground(Void... voids) {

            SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getContext());

            String lastTimeStr = sp.getString(PREF_INVITATIONS_DATA_LAST_TIME, "");
            boolean shouldRefresh = false;
            Date dateFromToRefresh = new Date();

            if (TextUtils.isEmpty(lastTimeStr)) {
                shouldRefresh = true;
                dateFromToRefresh = DateHelper.getMinDate();
            } else if (DateHelper.shouldRefreshMessages(lastTimeStr)) {
                try {
                    dateFromToRefresh = DateHelper.stringToDate(lastTimeStr);
                } catch (ParseException ex) {
                    dateFromToRefresh = DateHelper.getMinDate();
                }
            }

            if (shouldRefresh) {
                final Date date = dateFromToRefresh;
                Type listType = new TypeToken<List<GetNewMessagesResponseDto>>(){}.getType();

                return HttpResponder.askForDtoFromJsonArray(() -> {
                    try {
                        final String login = getArguments().getString(ARG_LOGIN);
                        final String key = getArguments().getString(ARG_KEY);
                        return mRepository.getMessagesRepository().GetNew(login, date, key);
                    } catch (RepositoryException ex) {
                        return null;
                    }
                }, listType, new ArrayList<GetNewMessagesResponseDto>());


            }
            return new HttpResponse<>(HttpURLConnection.HTTP_OK, null);
        }

        @Override
        protected void onPostExecute(final HttpResponse<List<GetNewMessagesResponseDto>> success) {
            SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getContext());

            String dataSerialized = sp.getString(PREF_INVITATIONS_DATA, "");
            List<GetNewMessagesResponseDto> workingList = new ArrayList<>();

            if (!TextUtils.isEmpty(dataSerialized)) {
                Gson gson = new Gson();
                Type listType = new TypeToken<List<GetNewMessagesResponseDto>>(){}.getType();
                workingList.addAll(gson.fromJson(dataSerialized, listType));
            }

            if (success != null && success.getCode() == HttpURLConnection.HTTP_OK) {
                if (success.getObject() != null) {
                    workingList.addAll(success.getObject());
                }
            } else {
                Toast.makeText(getContext(), getString(R.string.error_cant_download_invitations), Toast.LENGTH_SHORT).show();
            }

            workingList.sort(Comparator.comparing(lhs -> lhs.RowKey));

            mListOfGetNewMessagesResponseDto = workingList;

            saveListToCache();


            updateView();


            showProgress(false);
        }

        private void saveListToCache() {
            SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getContext());

            Gson gson = new Gson();
            String serialized = gson.toJson(mListOfGetNewMessagesResponseDto);
            String date = DateHelper.getCurrentDate();
            sp.edit().putString(PREF_INVITATIONS_DATA_LAST_TIME, date)
                    .putString(PREF_INVITATIONS_DATA, serialized)
                    .apply();
        }

        private void updateView() {

            if (mListOfGetNewMessagesResponseDto.size() > 0) {
                GetNewMessagesResponseDto top = mListOfGetNewMessagesResponseDto.get(0);
                mTxtInfoAboutInvitation.setText(top.Topic);
                mTxtInfoAboutInvitation.setVisibility(View.VISIBLE);
                mBtnAcceptInvitation.setVisibility(View.VISIBLE);
                mBtnAcceptInvitation.setOnClickListener(x -> attemptAccept(top.Content));
                mBtnRejectInvitation.setVisibility(View.VISIBLE);
                mBtnRejectInvitation.setOnClickListener(x -> rejectInvitation(top));
            } else {
                mTxtInfoAboutInvitation.setVisibility(View.GONE);
                mBtnAcceptInvitation.setVisibility(View.GONE);
                mBtnRejectInvitation.setVisibility(View.GONE);
            }
        }

        public void rejectInvitation(GetNewMessagesResponseDto top) {
            if (mListOfGetNewMessagesResponseDto != null && top != null) {
                mListOfGetNewMessagesResponseDto.remove(top);
                saveListToCache();
                updateView();
            }

        }

        private void attemptAccept(String content) {
            if (mAcceptInvitationTask != null)
                return;

            Pattern pattern = Pattern.compile("/[a-z]+/households/accept/([a-z_]+)/([a-z]+)/([0-9._:]+)");
            Matcher matcher = pattern.matcher(content);
            if (matcher.find()){
                String from = matcher.group(1);
                String to = matcher.group(2);
                String rowkey = matcher.group(3);

                mAcceptInvitationTask = new AcceptInvitationTask(from, to, rowkey);
                mAcceptInvitationTask.execute();
            } else {
                Toast.makeText(getContext(), getString(R.string.error_other), Toast.LENGTH_SHORT).show();
            }
        }

        @Override
        protected void onCancelled() {
            mGetInvitationsTask = null;
            showProgress(false);
        }

    }

    public class AcceptInvitationTask extends AsyncTask<Void, Void, HttpResponse<String>> {

        private String mFrom;
        private String mTo;
        private String mRowKey;

        private final Repository mRepository;

        public AcceptInvitationTask(String from, String to, String rowKey) {
            mFrom = from;
            mTo = to;
            mRowKey = rowKey;
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected void onPreExecute() {
            showProgress(true);
        }

        @Override
        protected HttpResponse<String> doInBackground(Void... voids) {
            return HttpResponder.askForString(() -> {
               try {
                   final String key = getArguments().getString(ARG_KEY);
                   return mRepository.getHouseholdsRepository().acceptInvitation(mFrom, mTo, mRowKey, key);
               } catch (Exception ex) {
                   return null;
               }
            });
        }

        @Override
        protected void onPostExecute(final HttpResponse<String> success) {
            if (success != null && success.getCode() == HttpURLConnection.HTTP_OK) {
                AccountManager accountManager = AccountManager.get(getContext());
                String accountName = getArguments().getString(ARG_LOGIN);
                String accountType = getString(R.string.account_type);
                Account account = new Account(accountName, accountType);
                accountManager.setUserData(account, AppAuthenticator.ACCOUNT_BELONGS_TO_HOUSEHOLD, AppAuthenticator.ACCOUNT_VALUE_TRUE);
                String newHouseholdId = success.getObject();
                accountManager.setUserData(account, AppAuthenticator.ACCOUNT_HOUSEHOLD_ID, newHouseholdId);
                if (notifyHouseholdChanged != null)
                    notifyHouseholdChanged.accept(newHouseholdId);
                deleteFromList();

                Toast.makeText(getContext(), getString(R.string.info_accepted), Toast.LENGTH_SHORT).show();
            }
            else {
                Toast.makeText(getContext(), getString(R.string.error_other), Toast.LENGTH_SHORT).show();
            }
            showProgress(false);
        }

        private void deleteFromList() {
            GetNewMessagesResponseDto itemToDelete = null;
            if (mListOfGetNewMessagesResponseDto != null)
                for (GetNewMessagesResponseDto item : mListOfGetNewMessagesResponseDto)
                    if (item.RowKey == mRowKey)
                        itemToDelete = item;
            if (itemToDelete != null) {
                mGetInvitationsTask.rejectInvitation(itemToDelete);
            }
        }

        @Override
        protected void onCancelled() {
            mAcceptInvitationTask = null;
            showProgress(false);
        }

    }

    public class InviteToHouseholdTask extends AsyncTask<Void, Void, Integer> {

        private final Repository mRepository;

        public InviteToHouseholdTask() {

            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }


        @Override
        protected void onPreExecute() {
            showProgress(true);
        }


        @Override
        protected Integer doInBackground(Void... voids) {
            return HttpResponder.ask(() -> {
                final String from = getArguments().getString(ARG_LOGIN);
                final String to = mTxtPersonToInviteLogin.getText().toString();
                final String key = getArguments().getString(ARG_KEY);
                try {
                    return mRepository.getHouseholdsRepository().invite(from, to, key);
                } catch (Exception ex) {
                    return null;
                }
            });
        }

        @Override
        protected void onPostExecute(final Integer success) {

            if (success != null && success == HttpURLConnection.HTTP_OK) {

                Toast.makeText(getContext(), getString(R.string.into_invitation_sent), Toast.LENGTH_SHORT).show();
            }
            else {
                Toast.makeText(getContext(), getString(R.string.error_other), Toast.LENGTH_SHORT).show();
            }
            showProgress(false);

        }


        @Override
        protected void onCancelled() {
            mInviteToHouseholdTask = null;
            showProgress(false);
        }
    }
}

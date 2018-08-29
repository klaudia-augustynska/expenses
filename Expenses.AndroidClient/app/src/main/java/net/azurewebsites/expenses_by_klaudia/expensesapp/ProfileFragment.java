package net.azurewebsites.expenses_by_klaudia.expensesapp;


import android.app.Activity;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.Bundle;
import android.app.Fragment;
import android.preference.PreferenceManager;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.Toast;

import com.google.gson.Gson;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.AppAuthenticator;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.DateHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.model.GetCashSummaryResponseDto;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;

import java.net.HttpURLConnection;


/**
 * A simple {@link Fragment} subclass.
 */
public class ProfileFragment extends Fragment {

    private static final String ARG_SECTION_NUMBER = "section_number";
    static final String ARG_LOGIN = "login";
    static final String ARG_KEY = "key";

    View mFormView;
    View mProgressView;
    View mFragment;
    DeleteAccountTask mDeleteAccountTask;

    public static ProfileFragment newInstance(int sectionNumber, String login, String key) {
        ProfileFragment fragment = new ProfileFragment();
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
        mFragment = inflater.inflate(R.layout.fragment_profile, container, false);

        mFormView = mFragment.findViewById(R.id.profile_form);
        mProgressView = mFragment.findViewById(R.id.profile_progress);

        Button btn = mFragment.findViewById(R.id.profile_btn_delete_account);
        btn.setOnClickListener((x) -> attemptDeleteAccount());

        return mFragment;
    }

    private void attemptDeleteAccount() {
        if (mDeleteAccountTask != null)
            return;
        mDeleteAccountTask = new DeleteAccountTask();
        mDeleteAccountTask.execute();
    }


    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mFormView, mProgressView);
    }

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        ((HomepageActivity) activity).onSectionAttached(
                getArguments().getInt(ARG_SECTION_NUMBER));
    }

    public class DeleteAccountTask extends AsyncTask<Void, Void, Integer> {

        private final Repository mRepository;

        String mLogin;
        String mKey;

        public DeleteAccountTask(){
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
            mLogin = getArguments().getString(ARG_LOGIN);
            mKey = getArguments().getString(ARG_KEY);
        }

        @Override
        protected Integer doInBackground(Void... voids) {
            return HttpResponder.ask(() -> {
                try {
                    return mRepository.GetUsersRepository().Delete(mLogin, mKey);
                } catch (Exception ex) {
                    return null;
                }
            });
        }

        @Override
        protected void onPostExecute(final Integer success) {
            if (success != null && success == HttpURLConnection.HTTP_OK) {
                AppAuthenticator.LogOut(getContext(), mLogin);
                getActivity().finish();
                Toast.makeText(getContext(), getString(R.string.profile_info_account_deleted), Toast.LENGTH_SHORT).show();
            }
            else {
                Toast.makeText(getContext(), getString(R.string.error_other), Toast.LENGTH_SHORT).show();
            }
            showProgress(false);
        }

        @Override
        protected void onCancelled() {
            mDeleteAccountTask = null;
            showProgress(false);
        }

        @Override
        protected void onPreExecute (){
            showProgress(true);
        }
    }
}

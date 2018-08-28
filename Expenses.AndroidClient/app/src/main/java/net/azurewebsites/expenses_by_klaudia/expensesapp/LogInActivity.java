package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.accounts.Account;
import android.accounts.AccountAuthenticatorActivity;
import android.accounts.AccountManager;
import android.content.Intent;
import android.os.AsyncTask;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.AppAuthenticator;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.model.LogInResponseDto;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;
import net.azurewebsites.expenses_by_klaudia.repository.RepositoryException;
import net.azurewebsites.expenses_by_klaudia.utils.HashUtil;

import java.net.HttpURLConnection;

public class LogInActivity extends AccountAuthenticatorActivity {

    private LogInTask mLogInTask = null;

    private EditText mLoginView;
    private EditText mPasswordView;
    private View mProgressView;
    private View mFormView;
    private Button mLogInButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_log_in);

        Intent intent = getIntent();
        String login = intent.getStringExtra(SplashActivity.EXTRA_LOGIN);
        
        mLoginView = findViewById(R.id.log_in_login);
        if (login != null) {
            mLoginView.setText(login);
        }
        mPasswordView = findViewById(R.id.log_in_password);
        mProgressView = findViewById(R.id.log_in_progress);
        mFormView = findViewById(R.id.log_in_form);
        mLogInButton = findViewById(R.id.log_in_button);
        mLogInButton.setOnClickListener(view -> attemptLogIn());
        Button goToSignUpButton = findViewById(R.id.go_to_sign_up_button);
        goToSignUpButton.setOnClickListener(x -> goToSignUp());

        ValidationUseCases useCases = new ValidationUseCases(x ->
                getString(x == null ? R.string.error_other : x));
        BindRulesToActivityHelper b = new BindRulesToActivityHelper(mLogInButton, getApplicationContext());
        b.add(mLoginView, useCases::isLoginValid);
        b.add(mPasswordView, useCases::isPasswordValid);
        b.validateForm();
    }

    private void goToSignUp() {
        Intent intent = new Intent(this, SignUpActivity.class);
        startActivity(intent);
    }

    private void attemptLogIn() {
        if (mLogInTask != null) {
            return;
        }

        String login = mLoginView.getText().toString();
        String password = mPasswordView.getText().toString();

        mLogInTask = new LogInActivity.LogInTask(login, password);
        mLogInTask.execute((Void) null);
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mFormView, mProgressView);
    }

    public class LogInTask extends AsyncTask<Void, Void, HttpResponse<LogInResponseDto>> {

        private final String mLogin;
        private final String mPassword;
        private final Repository mRepository;

        public LogInTask(String login, String password) {
            mLogin = login;
            mPassword = password;

            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected void onPreExecute (){
            showProgress(true);
        }

        @Override
        @SuppressWarnings("unchecked")
        protected HttpResponse<LogInResponseDto> doInBackground(Void... voids) {
            HttpResponse<String> saltResponse = HttpResponder.askForString(() -> {
                try {
                    return mRepository.GetUsersRepository().GetSalt(mLogin);
                } catch (RepositoryException ex) {
                    return null;
                }
            });
            if (saltResponse.getCode() == null
                    || saltResponse.getCode() != HttpURLConnection.HTTP_OK)
            {
                return null;
            }
            String salt = saltResponse.getObject();
            String hashedPassword = HashUtil.Hash(mPassword, salt);

            return HttpResponder.askForDtoFromJson(() -> {
                try {
                    return mRepository.GetUsersRepository().LogIn(mLogin, hashedPassword);
                } catch (RepositoryException ex) {
                    return null;
                }
            }, LogInResponseDto.class);
        }

        @Override
        protected void onPostExecute(final HttpResponse<LogInResponseDto> response) {
            mLogInTask = null;

            if (response != null
                    && response.getCode() == HttpURLConnection.HTTP_OK
                    && response.getObject() != null) {
                LogInResponseDto dto = response.getObject();
                saveCredentialsToAccountManager(dto);
                if (dto.Configured)
                    goToHomepage(dto.Key, dto.HouseholdId);
                else
                    goToConfiguration(dto.Key, dto.HouseholdId);
            }
            else if (response != null
                    && response.getCode() == HttpURLConnection.HTTP_BAD_REQUEST) {
                mLoginView.setError(getString(R.string.error_incorrect_credentials));
                mLoginView.requestFocus();
            }
            else {
                mLogInButton.setError(getString(R.string.error_other));
            }
            showProgress(false);
        }

        private void goToConfiguration(String key, String householdId) {
            Intent intent = new Intent(LogInActivity.this, InitialConfigurationActivity.class);
            intent.putExtra(SplashActivity.EXTRA_KEY, key);
            intent.putExtra(SplashActivity.EXTRA_LOGIN, mLogin);
            intent.putExtra(SplashActivity.EXTRA_HOUSEHOLD_ID, householdId);
            startActivity(intent);
        }

        private void goToHomepage(String key, String householdId) {
            Intent intent = new Intent(LogInActivity.this, HomepageActivity.class);
            intent.putExtra(SplashActivity.EXTRA_KEY, key);
            intent.putExtra(SplashActivity.EXTRA_LOGIN, mLogin);
            intent.putExtra(SplashActivity.EXTRA_HOUSEHOLD_ID, householdId);
            startActivity(intent);
        }

        private void saveCredentialsToAccountManager(LogInResponseDto dto) {
            String accountName = mLogin;
            String accountType = getString(R.string.account_type);
            String tokenType = getString(R.string.main_token_type);
            String key = dto.Key;

            AccountManager accountManager = AccountManager.get(LogInActivity.this);
            boolean accountExists = checkAccountExists(accountManager, accountName, accountType);
            final Account account = new Account(accountName, accountType);
            if (!accountExists) {
                accountManager.addAccountExplicitly(account, "", null);
            }
            accountManager.setUserData(account, AppAuthenticator.ACCOUNT_CONFIGURED, dto.Configured ? AppAuthenticator.ACCOUNT_VALUE_TRUE : AppAuthenticator.ACCOUNT_VALUE_FALSE);
            accountManager.setUserData(account, AppAuthenticator.ACCOUNT_BELONGS_TO_HOUSEHOLD, dto.BelongsToHousehold ? AppAuthenticator.ACCOUNT_VALUE_TRUE : AppAuthenticator.ACCOUNT_VALUE_FALSE);
            accountManager.setUserData(account, AppAuthenticator.ACCOUNT_HOUSEHOLD_ID, dto.HouseholdId);
            accountManager.setAuthToken(account, tokenType, key);
        }

        private boolean checkAccountExists(AccountManager accountManager, String accountName, String accountType) {
            final Account[] accounts = accountManager.getAccountsByType(accountType);
            for (Account account : accounts) {
                if (account.name.equals(accountName))
                    return true;
            }
            return false;
        }

        @Override
        protected void onCancelled() {
            mLogInTask = null;
            showProgress(false);
        }
    }

}

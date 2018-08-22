package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.app.Activity;
import android.content.Intent;
import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.model.LogInResponseDto;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;
import net.azurewebsites.expenses_by_klaudia.repository.RepositoryException;
import net.azurewebsites.expenses_by_klaudia.utils.HashUtil;

import java.io.IOException;
import java.net.HttpURLConnection;

public class LogInActivity extends AppCompatActivity {

    private LogInTask mAuthTask = null;

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
        String login = intent.getStringExtra(SignUpActivity.EXTRA_LOGIN);
        
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
        BindRulesToActivityHelper b = new BindRulesToActivityHelper(mLogInButton);
        b.add(mLoginView, useCases::isLoginValid);
        b.add(mPasswordView, useCases::isPasswordValid);
        b.validateForm();
    }

    private void goToSignUp() {
    }

    private void attemptLogIn() {
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

        }

        @Override
        protected void onCancelled() {
            mAuthTask = null;
            showProgress(false);
        }
    }

}

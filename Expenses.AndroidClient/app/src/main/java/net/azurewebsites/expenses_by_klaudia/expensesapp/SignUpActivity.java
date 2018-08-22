package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.app.Activity;
import android.content.Intent;
import android.support.v7.app.AppCompatActivity;

import android.os.AsyncTask;

import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;

import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationResult;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;
import net.azurewebsites.expenses_by_klaudia.repository.RepositoryException;
import net.azurewebsites.expenses_by_klaudia.utils.HashUtil;

import java.io.IOException;
import java.net.HttpURLConnection;

/**
 * A login screen that offers login via email/password.
 */
public class SignUpActivity extends AppCompatActivity {

    public static final String EXTRA_LOGIN = "net.azurewebsites.expenses_by_klaudia.expensesapp.LOGIN";

    private AddUserTask mAuthTask = null;

    // UI references.
    private EditText mLoginView;
    private EditText mPasswordView;
    private View mProgressView;
    private View mLoginFormView;
    Button mSignUpButton;
    Button mGoToLogInButton;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_sign_up);

        mSignUpButton = findViewById(R.id.sign_up_button);
        mSignUpButton.setOnClickListener(view -> attemptSignUp());

        mLoginView = findViewById(R.id.sign_up_login);
        mPasswordView = findViewById(R.id.sign_up_password);

        ValidationUseCases useCases = new ValidationUseCases(x ->
                getString(x == null ? R.string.error_other : x.intValue()));
        BindRulesToActivityHelper b = new BindRulesToActivityHelper(mSignUpButton);
        b.add(mLoginView, x -> useCases.isLoginValid(x));
        b.add(mPasswordView, x -> useCases.isPasswordValid(x));
        b.validateForm();

        mGoToLogInButton = findViewById(R.id.go_to_log_in_button);
        mGoToLogInButton.setOnClickListener(view -> goToLogIn());

        mLoginFormView = findViewById(R.id.sign_up_form_scrollview);
        mProgressView = findViewById(R.id.sign_up_progress);
    }

    private void goToLogIn() {
        Intent intent = new Intent(this, LogInActivity.class);
        startActivity(intent);
    }

    /**
     * Attempts to register the account specified by the login form.
     * If there are form errors (invalid email, missing fields, etc.), the
     * errors are presented and no actual login attempt is made.
     */
    private void attemptSignUp() {
        if (mAuthTask != null) {
            return;
        }

        String login = mLoginView.getText().toString();
        String password = mPasswordView.getText().toString();

        showProgress(true);
        mAuthTask = new AddUserTask(login, password, this);
        mAuthTask.execute((Void) null);
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mLoginFormView, mProgressView);
    }

    /**
     * Represents an asynchronous login/registration task used to authenticate
     * the user.
     */
    public class AddUserTask extends AsyncTask<Void, Void, Integer> {

        private final String mLogin;
        private final String mPassword;
        private final Activity mActivity;
        private final Repository mRepository;

        AddUserTask(String login, String password, Activity activity) {
            mLogin = login;
            mPassword = password;
            mActivity = activity;

            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected Integer doInBackground(Void... params) {
            String salt = HashUtil.GenerateSalt();
            String hashedPassword = HashUtil.Hash(mPassword, salt);

            HttpURLConnection connection = null;
            Integer response;
            try {
                connection = mRepository.GetUsersRepository().Add(mLogin, hashedPassword, salt);
                response = connection.getResponseCode();
            } catch (RepositoryException|IOException ex) {
                response = null;
            }
            finally {
                if (connection != null)
                    connection.disconnect();
            }
            return response;
        }

        @Override
        protected void onPostExecute(final Integer success) {
            mAuthTask = null;

            if (success != null
                    && success == HttpURLConnection.HTTP_CREATED) {
                Intent intent = new Intent(mActivity, LogInActivity.class);
                intent.putExtra(EXTRA_LOGIN, mLogin);
                startActivity(intent);
                finish();
            }
            else if (success != null
                    && success == HttpURLConnection.HTTP_BAD_REQUEST) {
                mLoginView.setError(getString(R.string.error_user_already_exists));
                mLoginView.requestFocus();
            }
            else {
                mSignUpButton.setError(getString(R.string.error_other));
            }
            showProgress(false);
        }

        @Override
        protected void onCancelled() {
            mAuthTask = null;
            showProgress(false);
        }
    }
}


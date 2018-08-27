package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.accounts.Account;
import android.accounts.AccountManager;
import android.accounts.AccountManagerFuture;
import android.app.AlertDialog;
import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.text.TextUtils;
import android.widget.ArrayAdapter;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.AppAuthenticator;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.model.LogInResponseDto;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;

import java.util.concurrent.TimeUnit;

public class SplashActivity extends AppCompatActivity {

    private String mAccountType;
    private String mAccountTokenType;
    private AlertDialog mAlertDialog;
    private AccountManager mAccountManager;

    private static final String STATE_DIALOG = "state_dialog";
    public static final String EXTRA_KEY = "net.azurewebsites.expenses_by_klaudia.expensesapp.TOKEN";
    public static final String EXTRA_LOGIN = "net.azurewebsites.expenses_by_klaudia.expensesapp.LOGIN";
    public static final String EXTRA_HOUSEHOLD_ID = "net.azurewebsites.expenses_by_klaudia.expensesapp.HOUSEHOLD_ID";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        setTheme(R.style.AppTheme);
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_splash);

        mAccountType = getString(R.string.account_type);
        mAccountTokenType = getString(R.string.main_token_type);
        mAccountManager = AccountManager.get(this);

        if (savedInstanceState != null) {
            boolean showDialog = savedInstanceState.getBoolean(STATE_DIALOG);
            if (showDialog) {
                final AccountManager accountManager = AccountManager.get(this);
                final Account[] accounts = accountManager.getAccountsByType(mAccountType);
                showAccountPicker(accounts);
            }
        }

        routeToAppropriatePage();
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        super.onSaveInstanceState(outState);
        if (mAlertDialog != null && mAlertDialog.isShowing()) {
            outState.putBoolean(STATE_DIALOG, true);
        }
    }

    /**
     * Checks whether the account manager contains a token for the user.
     * If there is no account, then the app will route to sign up page.
     * If there is an account but no key set, the app will route to log in page.
     * If there is an account and a key, the app will route to homepage
     */
    private void routeToAppropriatePage() {
        final Account[] accounts = mAccountManager.getAccountsByType(mAccountType);
        if (accounts.length == 0) {
            routeToSignUp();
            return;
        }
        if (accounts.length == 1) {
            tryToGetKeyFromAccount(accounts[0]);
            return;
        }
        // this will try to get key as soon as the user picks the account
        showAccountPicker(accounts);
    }

    private void tryToGetKeyFromAccount(Account account) {
        final AccountManagerFuture<Bundle> future = mAccountManager.getAuthToken(account, mAccountTokenType, null, this, null, null);

        new Thread(() -> {
            try {
                Bundle bundle = future.getResult(5, TimeUnit.SECONDS);
                String key = bundle.getString(AccountManager.KEY_AUTHTOKEN);
                if (!TextUtils.isEmpty(key)) {
                    // will route to homepage or user initial configuration page
                    checkIfUserWasConfigured(account, key);
                    return;
                }
                runOnUiThread(() -> {
                    Intent intent = bundle.getParcelable(AccountManager.KEY_INTENT);
                    startActivity(intent);
                });
            } catch (Exception ex) {
                routeToSignUp();
            }
        }).start();
    }

    @SuppressWarnings("unchecked")
    private void checkIfUserWasConfigured(Account account, String key) {
        String accountConfiguredInfo = mAccountManager.getUserData(account, AppAuthenticator.ACCOUNT_CONFIGURED);
        if (accountConfiguredInfo != null
                && accountConfiguredInfo.equals(AppAuthenticator.ACCOUNT_VALUE_TRUE)) {
            String householdId = mAccountManager.getUserData(account, AppAuthenticator.ACCOUNT_HOUSEHOLD_ID);
            routeToHomepage(account.name, key, householdId);
            return;
        }

        new Thread(() -> {
            Repository repository = new Repository(getString(R.string.repository_host));
            HttpResponse<LogInResponseDto> response = HttpResponder.askForDtoFromJson(() -> {
                try {
                    return repository.GetUsersRepository().LogInWithKey(account.name, key);
                } catch (Exception ex) {
                    return null;
                }
            },LogInResponseDto.class);


            if (response == null || response.getObject() == null) {
                routeToSignUp();
                return;
            }

            LogInResponseDto dto = response.getObject();

            mAccountManager.setUserData(account, AppAuthenticator.ACCOUNT_CONFIGURED, dto.Configured ? AppAuthenticator.ACCOUNT_VALUE_TRUE : AppAuthenticator.ACCOUNT_VALUE_FALSE);
            mAccountManager.setUserData(account, AppAuthenticator.ACCOUNT_BELONGS_TO_HOUSEHOLD, dto.BelongsToHousehold ? AppAuthenticator.ACCOUNT_VALUE_TRUE : AppAuthenticator.ACCOUNT_VALUE_FALSE);
            mAccountManager.setUserData(account, AppAuthenticator.ACCOUNT_HOUSEHOLD_ID, dto.HouseholdId);

            if (dto.Configured) {
                routeToHomepage(account.name, key, dto.HouseholdId);
            }
            else {
                routeToConfiguration(account.name, key, dto.HouseholdId);
            }
        }).start();
    }

    private void routeToConfiguration(String login, String key, String householdId) {
        runOnUiThread(() -> {
            Intent intent = new Intent(this, InitialConfigurationActivity.class);
            intent.putExtra(EXTRA_KEY, key);
            intent.putExtra(EXTRA_LOGIN, login);
            intent.putExtra(EXTRA_HOUSEHOLD_ID, householdId);
            startActivity(intent);
            finish();
        });
    }

    private void routeToHomepage(String login, String key, String householdId) {
        runOnUiThread(() -> {
            Intent intent = new Intent(this, HomepageActivity.class);
            intent.putExtra(EXTRA_KEY, key);
            intent.putExtra(EXTRA_LOGIN, login);
            intent.putExtra(EXTRA_HOUSEHOLD_ID, householdId);
            startActivity(intent);
            finish();
        });
    }

    private void routeToSignUp() {
        runOnUiThread(() -> {
            Intent intent = new Intent(this, SignUpActivity.class);
            startActivity(intent);
            finish();
        });
    }

    private void showAccountPicker(Account[] accounts) {

        String name[] = new String[accounts.length];
        for (int i = 0; i < accounts.length; i++) {
            name[i] = accounts[i].name;
        }

        // Account picker
        mAlertDialog = new AlertDialog.Builder(this).setTitle("Pick Account").setAdapter(new ArrayAdapter<String>(getBaseContext(), android.R.layout.simple_list_item_1, name), (dialog, which) ->
                tryToGetKeyFromAccount(accounts[which])).create();
        mAlertDialog.show();
    }
}

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

import java.util.concurrent.TimeUnit;

public class SplashActivity extends AppCompatActivity {

    private String mAccountType;
    private String mAccountTokenType;
    private AlertDialog mAlertDialog;
    private AccountManager mAccountManager;

    private static final String STATE_DIALOG = "state_dialog";
    public static final String EXTRA_KEY = "net.azurewebsites.expenses_by_klaudia.expensesapp.TOKEN";

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
        finish();
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
        try {
            Bundle bundle = future.getResult(5, TimeUnit.SECONDS);
            String key = bundle.getString(AccountManager.KEY_AUTHTOKEN);
            if (!TextUtils.isEmpty(key)) {
                routeToHomepage(key);
                return;
            }
            Intent intent = bundle.getParcelable(AccountManager.KEY_INTENT);
            startActivity(intent);
        } catch (Exception ex) {
            routeToSignUp();
        }
    }

    private void routeToHomepage(String key) {
        Intent intent = new Intent(this, HomepageActivity.class);
        intent.putExtra(EXTRA_KEY, key);
        startActivity(intent);
    }

    private void routeToSignUp() {
        Intent intent = new Intent(this, SignUpActivity.class);
        startActivity(intent);
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

package net.azurewebsites.expenses_by_klaudia.expensesapp.helpers;

import android.accounts.AbstractAccountAuthenticator;
import android.accounts.Account;
import android.accounts.AccountAuthenticatorResponse;
import android.accounts.AccountManager;
import android.accounts.NetworkErrorException;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.text.TextUtils;

import net.azurewebsites.expenses_by_klaudia.expensesapp.LogInActivity;

public class AppAuthenticator extends AbstractAccountAuthenticator {

    private Context mContext;

    public final static String ARG_ACCOUNT_TYPE = "ACCOUNT_TYPE";
    public final static String ARG_TOKEN_TYPE = "AUTH_TYPE";
    public final static String ACCOUNT_CONFIGURED = "ACCOUNT_CONFIGURED";
    public final static String ACCOUNT_VALUE_TRUE = "true";
    public final static String ACCOUNT_VALUE_FALSE = "false";
    public final static String ACCOUNT_HOUSEHOLD_ID = "ACCOUNT_HOUSEHOLD_ID";
    public final static String ACCOUNT_BELONGS_TO_HOUSEHOLD = "ACCOUNT_BELONGS_TO_HOUSEHOLD";

    public AppAuthenticator(Context context) {
        super(context);
        mContext = context;
    }

    @Override
    public Bundle editProperties(AccountAuthenticatorResponse response, String s) {
        return null;
    }

    @Override
    public Bundle addAccount(AccountAuthenticatorResponse response,
                                       String accountType, String authTokenType,
                                       String[] requiredFeatures, Bundle options) {
        final Intent intent = new Intent(mContext, LogInActivity.class);
        intent.putExtra(ARG_ACCOUNT_TYPE, accountType);
        intent.putExtra(ARG_TOKEN_TYPE, authTokenType);
        intent.putExtra(AccountManager.KEY_ACCOUNT_AUTHENTICATOR_RESPONSE, response);
        final Bundle bundle = new Bundle();
        bundle.putParcelable(AccountManager.KEY_INTENT, intent);
        return bundle;
    }

    @Override
    public Bundle confirmCredentials(AccountAuthenticatorResponse response, Account account, Bundle options) {
        return null;
    }

    @Override
    public Bundle getAuthToken(AccountAuthenticatorResponse response, Account account, String authTokenType, Bundle options) {
        // retrieve token from AccountManager
        final AccountManager am = AccountManager.get(mContext);
        String authToken = am.peekAuthToken(account, authTokenType);

        final Bundle result = new Bundle();

        // if the user doesn't have token, redirect to LogInActivity
        if (TextUtils.isEmpty(authToken)) {
            final Intent intent = new Intent(mContext, LogInActivity.class);
            intent.putExtra(AccountManager.KEY_ACCOUNT_AUTHENTICATOR_RESPONSE, response);
            intent.putExtra(ARG_ACCOUNT_TYPE, account.type);
            intent.putExtra(ARG_TOKEN_TYPE, authTokenType);
            result.putParcelable(AccountManager.KEY_INTENT, intent);
        }
        // if the user does have token, return it
        else {
            result.putString(AccountManager.KEY_ACCOUNT_NAME, account.name);
            result.putString(AccountManager.KEY_ACCOUNT_TYPE, account.type);
            result.putString(AccountManager.KEY_AUTHTOKEN, authToken);
        }
        return result;
    }

    @Override
    public String getAuthTokenLabel(String s) {
        return null;
    }

    @Override
    public Bundle updateCredentials(AccountAuthenticatorResponse response, Account account, String s, Bundle options) {
        return null;
    }

    @Override
    public Bundle hasFeatures(AccountAuthenticatorResponse response, Account account, String[] strings) {
        return null;
    }
}

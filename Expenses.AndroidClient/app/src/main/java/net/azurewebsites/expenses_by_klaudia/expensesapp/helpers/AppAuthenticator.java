package net.azurewebsites.expenses_by_klaudia.expensesapp.helpers;

import android.accounts.AbstractAccountAuthenticator;
import android.accounts.Account;
import android.accounts.AccountAuthenticatorResponse;
import android.accounts.AccountManager;
import android.accounts.NetworkErrorException;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.text.TextUtils;

import net.azurewebsites.expenses_by_klaudia.expensesapp.AddExpensesActivity;
import net.azurewebsites.expenses_by_klaudia.expensesapp.CategoriesFragment;
import net.azurewebsites.expenses_by_klaudia.expensesapp.HomepageFragment;
import net.azurewebsites.expenses_by_klaudia.expensesapp.HouseholdFragment;
import net.azurewebsites.expenses_by_klaudia.expensesapp.InvitationsFragment;
import net.azurewebsites.expenses_by_klaudia.expensesapp.LogInActivity;
import net.azurewebsites.expenses_by_klaudia.expensesapp.R;
import net.azurewebsites.expenses_by_klaudia.expensesapp.SplashActivity;

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

    public static void LogOut(Context context, String login) {
        clearKeyFromAccountManager(context, login);
        clearCacheFromPreferences(context);
        goToLogInPage(context, login);
    }

    private static void goToLogInPage(Context context, String login) {
        Intent intent = new Intent(context, LogInActivity.class);
        intent.putExtra(SplashActivity.EXTRA_LOGIN, login);
        context.startActivity(intent);
    }

    private static void clearCacheFromPreferences(Context context) {
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(context);
        sp.edit()
                .remove(AddExpensesActivity.GetDataTask.PREF_ADD_EXPENSES_DATA)
                .remove(AddExpensesActivity.GetDataTask.PREF_ADD_EXPENSES_DATA_LAST_TIME)
                .remove(HomepageFragment.PREF_SUMMARY_LAST_TIME)
                .remove(HomepageFragment.PREF_SUMMARY_DATA)
                .remove(CategoriesFragment.GetCategoriesTask.PREF_CATEGORIES_DATA)
                .remove(CategoriesFragment.GetCategoriesTask.PREF_CATEGORIES_DATA_LAST_TIME)
                .remove(InvitationsFragment.GetInvitationsTask.PREF_INVITATIONS_DATA)
                .remove(InvitationsFragment.GetInvitationsTask.PREF_INVITATIONS_DATA_LAST_TIME)
                .remove(HouseholdFragment.GetMembersTask.PREF_MEMBERS_DATA_LAST_TIME)
                .remove(HouseholdFragment.GetMembersTask.PREF_MEMBERS_DATA)
                .apply();
    }

    private static void clearKeyFromAccountManager(Context context, String login) {
        AccountManager accountManager = AccountManager.get(context);
        Account account = new Account(login, context.getString(R.string.account_type));
        accountManager.removeAccount(account, null, null);
    }
}

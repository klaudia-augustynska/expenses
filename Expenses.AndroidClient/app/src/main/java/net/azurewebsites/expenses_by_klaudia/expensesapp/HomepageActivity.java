package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.app.Activity;

import android.app.Fragment;
import android.app.FragmentManager;
import android.app.FragmentTransaction;
import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.ActionBar;
import android.support.v7.app.AppCompatActivity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.support.v4.widget.DrawerLayout;

import net.azurewebsites.expenses_by_klaudia.model.CURRENCY_CODE;

public class HomepageActivity extends AppCompatActivity
        implements NavigationDrawerFragment.NavigationDrawerCallbacks {

    /**
     * Fragment managing the behaviors, interactions and presentation of the navigation drawer.
     */
    private NavigationDrawerFragment mNavigationDrawerFragment;

    /**
     * Used to store the last screen title. For use in {@link #restoreActionBar()}.
     */
    private CharSequence mTitle;

    String mLogin;
    String mKey;
    String mHouseholdId;

    public final static String EXTRA_BILL_ADDED = "net.azurewebsites.expenses_by_klaudia.expensesapp.BILL_ADDED";
    public final static String EXTRA_BILL_CURRENCY = "net.azurewebsites.expenses_by_klaudia.expensesapp.BILL_CURRENCY";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_homepage);

        Intent intent = getIntent();
        mKey = intent.getStringExtra(SplashActivity.EXTRA_KEY);
        mLogin = intent.getStringExtra(SplashActivity.EXTRA_LOGIN);
        mHouseholdId = intent.getStringExtra(SplashActivity.EXTRA_HOUSEHOLD_ID);

        mNavigationDrawerFragment = (NavigationDrawerFragment)
                getFragmentManager().findFragmentById(R.id.navigation_drawer);
        mTitle = getTitle();

        // Set up the drawer.
        mNavigationDrawerFragment.setUp(
                R.id.navigation_drawer,
                (DrawerLayout) findViewById(R.id.drawer_layout),
                mLogin);
    }

    @Override
    public void onNavigationDrawerItemSelected(int position) {
        // update the main content by replacing fragments

        Intent intent = getIntent();
        mKey = intent.getStringExtra(SplashActivity.EXTRA_KEY);
        mLogin = intent.getStringExtra(SplashActivity.EXTRA_LOGIN);
        mHouseholdId = intent.getStringExtra(SplashActivity.EXTRA_HOUSEHOLD_ID);

        FragmentManager fragmentManager = getFragmentManager();
        FragmentTransaction transaction = fragmentManager.beginTransaction();
        Fragment fragment;
        switch (position) {
            case 3:
                fragment = CategoriesFragment.newInstance(position, mLogin, mKey);
                break;
            case 1:
                fragment = ProfileFragment.newInstance(position, mLogin, mKey);
                break;
            case 0:
            default:
                double billAdded = intent.getDoubleExtra(EXTRA_BILL_ADDED, 0);
                CURRENCY_CODE billAddedCurrency = (CURRENCY_CODE) intent.getSerializableExtra(EXTRA_BILL_CURRENCY);
                fragment = HomepageFragment.newInstance(position, mLogin, mKey, mHouseholdId, billAdded, billAddedCurrency);
                break;
        }
        transaction.replace(R.id.container, fragment);
        transaction.commit();
    }

    public void onSectionAttached(int number) {
        switch (number) {
            case 0:
                mTitle = getString(R.string.title_section_summary);
                break;
            case 1:
                mTitle = getString(R.string.title_section_profile);
                break;
            case 2:
                mTitle = getString(R.string.title_section_household);
                break;
            case 3:
                mTitle = getString(R.string.title_section_categories);
                break;
        }
        restoreActionBar();
    }

    public void restoreActionBar() {
        ActionBar actionBar = getSupportActionBar();
        actionBar.setNavigationMode(ActionBar.NAVIGATION_MODE_STANDARD);
        actionBar.setDisplayShowTitleEnabled(true);
        actionBar.setTitle(mTitle);
    }
}

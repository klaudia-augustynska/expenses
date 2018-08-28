package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.accounts.Account;
import android.accounts.AccountManager;
import android.content.Intent;
import android.icu.util.Calendar;
import android.icu.util.GregorianCalendar;
import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.SeekBar;
import android.widget.Spinner;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.AppAuthenticator;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationResult;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.model.CURRENCY_CODE;
import net.azurewebsites.expenses_by_klaudia.model.ConfigureUserDto;
import net.azurewebsites.expenses_by_klaudia.model.Money;
import net.azurewebsites.expenses_by_klaudia.model.SEX_CODE;
import net.azurewebsites.expenses_by_klaudia.model.Wallet;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;
import net.azurewebsites.expenses_by_klaudia.repository.RepositoryException;

import java.net.HttpURLConnection;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;

public class InitialConfigurationActivity extends AppCompatActivity {
    String mKey;
    String mLogin;
    String mHouseholdId;

    EditText mTxtDateView;
    EditText mTxtWeight;
    EditText mTxtHeight;
    EditText mTxtName;
    EditText mTxtWalletName;
    EditText mTxtWalletMoney;
    SeekBar mPal;
    Spinner mGenderSpinner;
    Spinner mWalletCurrency;
    List<Wallet> mWallets;
    WalletListItemAdapter mWalletsAdapter;
    Button mBtnAddWallet;
    Button mBtnConfigure;
    View mConfigurationFormView;
    View mProgressView;
    ConfigureUserTask mConfigureUserTask;
    BindRulesToActivityHelper mConfigureValidator;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_initial_configuration);

        Intent intent = getIntent();
        mKey = intent.getStringExtra(SplashActivity.EXTRA_KEY);
        mLogin = intent.getStringExtra(SplashActivity.EXTRA_LOGIN);
        mHouseholdId = intent.getStringExtra(SplashActivity.EXTRA_HOUSEHOLD_ID);

        mGenderSpinner = findViewById(R.id.configuration_gender);
        List<String> genders = new ArrayList<>();
        genders.add(getString(R.string.spinner_female));
        genders.add(getString(R.string.spinner_male));
        ArrayAdapter<String> genderDataAdapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, genders);
        mGenderSpinner.setAdapter(genderDataAdapter);

        mTxtDateView = findViewById(R.id.configuration_date);
        Button pickDateButton = findViewById(R.id.configuration_pick_date);
        pickDateButton.setOnClickListener((view) -> {
            DatePickerFragment newFragment = new DatePickerFragment();
            newFragment.setEditText(mTxtDateView);
            newFragment.show(getSupportFragmentManager(), "datePicker");
        });

        mWalletCurrency = findViewById(R.id.configuration_money_currency);
        List<String> currencies = new ArrayList<>();
        CURRENCY_CODE[] currencyValues = CURRENCY_CODE.values();
        // note: ommiting the first value left for errors
        for (int i = 1; i < currencyValues.length; ++i)
            currencies.add(currencyValues[i].toString());
        ArrayAdapter<String> currencyDataAdapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, currencies);
        mWalletCurrency.setAdapter(currencyDataAdapter);

        RecyclerView walletsRecycler = findViewById(R.id.configuration_list_of_wallets);
        walletsRecycler.setHasFixedSize(true);
        RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(this);
        walletsRecycler.setLayoutManager(layoutManager);
        mWallets = new ArrayList<>();
        mWalletsAdapter = new WalletListItemAdapter(mWallets);
        walletsRecycler.setAdapter(mWalletsAdapter);

        mTxtWalletName = findViewById(R.id.configuration_wallet_name);
        mTxtWalletMoney = findViewById(R.id.configuration_wallet_money);
        mWalletCurrency = findViewById(R.id.configuration_money_currency);
        mBtnAddWallet = findViewById(R.id.configuration_addwallet);
        mBtnAddWallet.setOnClickListener((x) -> {
            addWallet();
        });

        mTxtWeight = findViewById(R.id.configuration_weight);
        mTxtHeight = findViewById(R.id.configuration_height);
        mTxtName = findViewById(R.id.configuration_name);
        mPal = findViewById(R.id.configuration_pal);

        ValidationUseCases useCases = new ValidationUseCases(x ->
                getString(x == null ? R.string.error_other : x));

        BindRulesToActivityHelper bWallet = new BindRulesToActivityHelper(mBtnAddWallet, getApplicationContext());
        bWallet.add(mTxtWalletName, useCases::isWalletNameValid);
        bWallet.add(mTxtWalletMoney, useCases::isMoneyValid);
        bWallet.validateForm();

        mBtnConfigure = findViewById(R.id.configuration_configure);
        mBtnConfigure.setOnClickListener((x) -> attemptConfigure());
        BindRulesToActivityHelper bForm = new BindRulesToActivityHelper(mBtnConfigure, getApplicationContext());
        bForm.add(mTxtWeight, useCases::isWeightValid);
        bForm.add(mTxtHeight, useCases::isHeightValid);
        bForm.add(mTxtName, useCases::isNameValid);
        bForm.add(mTxtDateView, useCases::isDateValid);
        bForm.addRule((x) -> {
            if (mWallets != null && mWallets.size() > 0)
                return new ValidationResult(true, null);
            return new ValidationResult(false, getString(R.string.error_no_wallets));
        });
        bForm.validateForm();
        mConfigureValidator = bForm;

        mConfigurationFormView = findViewById(R.id.configuration_form);
        mProgressView = findViewById(R.id.configuration_progress);
    }

    private void attemptConfigure() {
        if (mConfigureUserTask != null) {
            return;
        }

        ConfigureUserDto dto = new ConfigureUserDto();
        dto.DateOfBirth = parseStringToDate(mTxtDateView.getText().toString());
        dto.Height = Integer.parseInt(mTxtHeight.getText().toString());
        dto.Name = mTxtName.getText().toString();
        dto.Pal = ((double) mPal.getProgress()) / 10d + 1.4;
        dto.Sex = SEX_CODE.values()[(int)mGenderSpinner.getSelectedItemId()];
        dto.Weight = Double.parseDouble(mTxtWeight.getText().toString());
        dto.Wallets = mWallets;

        mConfigureUserTask = new ConfigureUserTask(dto);
        mConfigureUserTask.execute((Void) null);
    }

    private Date parseStringToDate(String s) {
        String[] split = s.split("-");
        int[] integers = new int[split.length];
        for (int i = 0; i < split.length; ++i)
            integers[i] = Integer.parseInt(split[i]);

        Calendar cal = new GregorianCalendar(integers[0], integers[1]-1, integers[2]);
        return cal.getTime();
    }

    private void addWallet() {
        Wallet wallet = new Wallet();
        wallet.Name = mTxtWalletName.getText().toString();
        wallet.Money = new Money();
        wallet.Money.Amount = Double.parseDouble(mTxtWalletMoney.getText().toString());
        wallet.Money.Currency = getEnumValue((String)mWalletCurrency.getSelectedItem());
        mWallets.add(wallet);
        mWalletsAdapter.notifyDataSetChanged();
        mTxtWalletName.setText("");
        mTxtWalletMoney.setText("");
        mConfigureValidator.validateForm();
    }

    private CURRENCY_CODE getEnumValue(String selectedItem) {
        CURRENCY_CODE[] values = CURRENCY_CODE.values();
        for (CURRENCY_CODE item : values) {
            if (item.toString().equals(selectedItem))
                return item;
        }
        return CURRENCY_CODE.Default;
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mConfigurationFormView, mProgressView);
    }

    public class ConfigureUserTask extends AsyncTask<Void, Void, Integer> {
        private final Repository mRepository;
        private final ConfigureUserDto mDto;

        public ConfigureUserTask(ConfigureUserDto dto) {
            mDto = dto;
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected void onPreExecute (){
            showProgress(true);
        }

        @Override
        protected Integer doInBackground(Void... voids) {
            return HttpResponder.ask(() -> {
                try {
                    return mRepository.GetUsersRepository().ConfigureUser(mLogin, mKey, mDto);
                } catch (RepositoryException ex) {
                    return null;
                }
            });
        }

        @Override
        protected void onPostExecute(final Integer success) {
            mConfigureUserTask = null;

            if (success != null
                    && success == HttpURLConnection.HTTP_OK) {
                accountManagerSetThatUserIsConfigured();

                Intent intent = new Intent(InitialConfigurationActivity.this, HomepageActivity.class);
                intent.putExtra(SplashActivity.EXTRA_LOGIN, mLogin);
                intent.putExtra(SplashActivity.EXTRA_KEY, mKey);
                intent.putExtra(SplashActivity.EXTRA_HOUSEHOLD_ID, mHouseholdId);
                startActivity(intent);
                finish();
            }
            else {
                mBtnConfigure.setError(getString(R.string.error_other));
            }
            showProgress(false);
        }

        private void accountManagerSetThatUserIsConfigured() {
            AccountManager accountManager = AccountManager.get(InitialConfigurationActivity.this);
            String accountName = mLogin;
            String accountType = getString(R.string.account_type);
            final Account account = new Account(accountName, accountType);
            accountManager.setUserData(account, AppAuthenticator.ACCOUNT_CONFIGURED, AppAuthenticator.ACCOUNT_VALUE_TRUE);
        }

        @Override
        protected void onCancelled() {
            mConfigureUserTask = null;
            showProgress(false);
        }
    }
}

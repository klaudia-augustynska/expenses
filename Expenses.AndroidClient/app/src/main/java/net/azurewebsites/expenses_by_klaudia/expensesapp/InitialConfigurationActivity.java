package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.SeekBar;
import android.widget.Spinner;

import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.model.CURRENCY_CODE;
import net.azurewebsites.expenses_by_klaudia.model.Money;
import net.azurewebsites.expenses_by_klaudia.model.Wallet;

import java.util.ArrayList;
import java.util.List;

public class InitialConfigurationActivity extends AppCompatActivity {

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

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_initial_configuration);

        Intent intent = getIntent();
        String key = intent.getStringExtra(SplashActivity.EXTRA_KEY);

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

        BindRulesToActivityHelper bWallet = new BindRulesToActivityHelper(mBtnAddWallet);
        bWallet.add(mTxtWalletName, useCases::isWalletNameValid);
        bWallet.add(mTxtWalletMoney, useCases::isMoneyValid);
        bWallet.validateForm(false);

        mBtnConfigure = findViewById(R.id.configuration_configure);
        mBtnConfigure.setOnClickListener((x) -> attemptConfigure());
        BindRulesToActivityHelper bForm = new BindRulesToActivityHelper(mBtnConfigure);
        bForm.add(mTxtWeight, useCases::isWeightValid);
        bForm.add(mTxtHeight, useCases::isHeightValid);
        bForm.add(mTxtName, useCases::isNameValid);
        bForm.add(mTxtDateView, useCases::isDateValid);
    }

    private void attemptConfigure() {
    }

    private void addWallet() {
        Wallet wallet = new Wallet();
        wallet.Name = mTxtWalletName.getText().toString();
        wallet.Money = new Money();
        wallet.Money.Amount = Double.parseDouble(mTxtWalletMoney.getText().toString());
        wallet.Money.Currency = getEnumValue((String)mWalletCurrency.getSelectedItem());
        mWallets.add(wallet);
        mWalletsAdapter.notifyDataSetChanged();
    }

    private CURRENCY_CODE getEnumValue(String selectedItem) {
        CURRENCY_CODE[] values = CURRENCY_CODE.values();
        for (CURRENCY_CODE item : values) {
            if (item.toString() == selectedItem)
                return item;
        }
        return CURRENCY_CODE.Default;
    }
}

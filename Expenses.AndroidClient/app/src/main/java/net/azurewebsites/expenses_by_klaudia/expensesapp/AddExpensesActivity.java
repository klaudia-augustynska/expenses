package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.preference.PreferenceManager;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.text.TextUtils;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.Toast;

import com.google.gson.Gson;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.DateHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationResult;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.model.AddCashFlowDto;
import net.azurewebsites.expenses_by_klaudia.model.CashFlowDetail;
import net.azurewebsites.expenses_by_klaudia.model.Category;
import net.azurewebsites.expenses_by_klaudia.model.GetCashSummaryResponseDto;
import net.azurewebsites.expenses_by_klaudia.model.GetCategoriesResponseDto;
import net.azurewebsites.expenses_by_klaudia.model.GetDataForAddCashFlowResponseDto;
import net.azurewebsites.expenses_by_klaudia.model.Money;
import net.azurewebsites.expenses_by_klaudia.model.Wallet;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;
import net.azurewebsites.expenses_by_klaudia.repository.RepositoryException;

import java.net.HttpURLConnection;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;
import java.util.UUID;

import static net.azurewebsites.expenses_by_klaudia.expensesapp.HomepageActivity.EXTRA_BILL_ADDED;
import static net.azurewebsites.expenses_by_klaudia.expensesapp.HomepageActivity.EXTRA_BILL_CURRENCY;
import static net.azurewebsites.expenses_by_klaudia.expensesapp.SplashActivity.EXTRA_HOUSEHOLD_ID;
import static net.azurewebsites.expenses_by_klaudia.expensesapp.SplashActivity.EXTRA_KEY;
import static net.azurewebsites.expenses_by_klaudia.expensesapp.SplashActivity.EXTRA_LOGIN;

public class AddExpensesActivity extends AppCompatActivity {
    String mKey;
    String mLogin;
    String mHouseholdId;

    View mFormView;
    View mProgressView;
    EditText mTxtDateView;
    Spinner mGlobalCategorySpinner;
    EditText mTxtMoneyView;
    Spinner mWalletsSpinner;
    Spinner mCategoryDetailSpinner;
    EditText mTxtMoneyDetailView;
    EditText mTxtCommentView;
    GetDataTask mGetDataTask;
    List<Category> mCategories;
    List<String> mCategoriesStrings;
    boolean mFatalError;
    List<Wallet> mWallets;
    List<String> mWalletsStrings;
    Button mBtnAddDetail;
    Button mBtnAddCashflow;
    List<CashFlowDetail> mCashFlowDetails;
    CashflowListItemAdapter mDetailAdapter;
    BindRulesToActivityHelper mbDetail;
    AddCashFlowTask mAddCashFlowTask;
    BindRulesToActivityHelper bCf;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_add_expenses);

        Intent intent = getIntent();
        mKey = intent.getStringExtra(SplashActivity.EXTRA_KEY);
        mLogin = intent.getStringExtra(EXTRA_LOGIN);
        mHouseholdId = intent.getStringExtra(SplashActivity.EXTRA_HOUSEHOLD_ID);

        mFormView = findViewById(R.id.add_expenses_form);
        mProgressView = findViewById(R.id.add_expenses_progress);

        mTxtDateView = findViewById(R.id.add_expenses_date);
        Button pickDateBtn = findViewById(R.id.add_expenses_pick_date);
        pickDateBtn.setOnClickListener((view) -> {
            DatePickerFragment newFragment = new DatePickerFragment();
            newFragment.setEditText(mTxtDateView);
            newFragment.show(getSupportFragmentManager(), "datePicker");
        });
        Calendar cal = Calendar.getInstance();
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.getDefault());
        mTxtDateView.setText(sdf.format(cal.getTime()));

        mGlobalCategorySpinner = findViewById(R.id.add_expenses_categories);
        mCategoryDetailSpinner = findViewById(R.id.add_expenses_categories_detail);
        mWalletsSpinner = findViewById(R.id.add_expenses_wallets);
        mWalletsSpinner.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parentView, View selectedItemView, int position, long id) {
                mDetailAdapter.setCurrency(mWallets.get(position).Money.Currency);
                mDetailAdapter.notifyDataSetChanged();
            }

            @Override
            public void onNothingSelected(AdapterView<?> parentView) {

            }

        });

        mGetDataTask = new GetDataTask();
        mGetDataTask.execute((Void) null);

        mTxtMoneyView = findViewById(R.id.add_expenses_money);
        mTxtMoneyDetailView = findViewById(R.id.add_expenses_money_detail);
        mTxtCommentView = findViewById(R.id.add_expenses_comment);


        ValidationUseCases useCases = new ValidationUseCases(x ->
                getString(x == null ? R.string.error_other : x));

        mBtnAddDetail = findViewById(R.id.add_expenses_btn_add_detail);
        mBtnAddDetail.setOnClickListener((x) -> addDetail());
        mbDetail = new BindRulesToActivityHelper(mBtnAddDetail, getApplicationContext());
        mbDetail.add(mTxtMoneyDetailView, useCases::isMoneyValid);
        mbDetail.addRule((x) -> (mFatalError)
                ? new ValidationResult(false, getString(R.string.error_add_expenses_cannot_download_data))
                : new ValidationResult(true, null));
        mbDetail.validateForm();

        mBtnAddCashflow = findViewById(R.id.add_expenses_btn_add_bill);
        mBtnAddCashflow.setOnClickListener((x) -> attemptAddCashflow());
        bCf = new BindRulesToActivityHelper(mBtnAddCashflow, getApplicationContext());
        bCf.add(mTxtDateView, useCases::isDateValid);
        bCf.add(mTxtMoneyView, useCases::isMoneyValid);
        bCf.addRule((x) -> {
            if (mFatalError)
                return new ValidationResult(false, getString(R.string.error_add_expenses_cannot_download_data));
            if (!sumOfDetailsIsValid())
                return new ValidationResult(false, getString(R.string.error_add_expenses_wrong_details));
            return new ValidationResult(true, null);
        });
        bCf.validateForm();
    }

    private boolean sumOfDetailsIsValid() {
        ValidationResult isMoneyValid = bCf.validateField(mTxtMoneyView, false);
        if (!isMoneyValid.getSuccess())
            return false;
        double globalSum = Double.parseDouble(mTxtMoneyView.getText().toString());
        double sumOfDetails = 0;
        if (mCashFlowDetails != null)
            for (CashFlowDetail detail : mCashFlowDetails)
                sumOfDetails += detail.Amount;
        return globalSum >= sumOfDetails;
    }

    private void addDetail() {
        CashFlowDetail detail = new CashFlowDetail();
        detail.Amount = Double.parseDouble(mTxtMoneyDetailView.getText().toString());
        detail.CategoryGuid = findCategoryGuid(mCategoryDetailSpinner);
        String comment = mTxtCommentView.getText().toString();
        detail.Comment = TextUtils.isEmpty(comment) ? null : comment;
        mCashFlowDetails.add(detail);
        mTxtMoneyDetailView.setText("");
        mTxtCommentView.setText("");
        mDetailAdapter.notifyDataSetChanged();
        mbDetail.validateForm();
        bCf.validateForm(true);
    }

    private UUID findCategoryGuid(Spinner spinner) {
        int pos = spinner.getSelectedItemPosition();
        Category cat = mCategories.get(pos);
        return cat.Guid;
    }

    private void attemptAddCashflow() {
        if (mAddCashFlowTask != null)
            return;

        AddCashFlowDto dto = new AddCashFlowDto();
        dto.Amount = new Money();
        dto.Amount.Amount = Double.parseDouble(mTxtMoneyView.getText().toString());
        Wallet wallet = findSelectedWallet();
        dto.Amount.Currency = wallet.Money.Currency;
        dto.CategoryGuid = findCategoryGuid(mGlobalCategorySpinner);
        dto.DateTime = parseStringToDate(mTxtDateView.getText().toString());
        dto.WalletGuid = wallet.Guid;
        dto.Details = mCashFlowDetails;

        mAddCashFlowTask = new AddCashFlowTask(dto);
        mAddCashFlowTask.execute((Void) null);
    }


    private Date parseStringToDate(String s) {
        String[] split = s.split("-");
        int[] integers = new int[split.length];
        for (int i = 0; i < split.length; ++i)
            integers[i] = Integer.parseInt(split[i]);

        android.icu.util.Calendar cal = new android.icu.util.GregorianCalendar(integers[0], integers[1]-1, integers[2]);
        return cal.getTime();
    }

    private Wallet findSelectedWallet() {
        int selected = mWalletsSpinner.getSelectedItemPosition();
        return mWallets.get(selected);
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mFormView, mProgressView);
    }

    public class GetDataTask extends AsyncTask<Void, Void, HttpResponse<GetDataForAddCashFlowResponseDto>> {
        private final Repository mRepository;

        public final static String PREF_ADD_EXPENSES_DATA = "PREF_ADD_EXPENSES_DATA";
        public final static String PREF_ADD_EXPENSES_DATA_LAST_TIME = "PREF_ADD_EXPENSES_DATA_LAST_TIME";

        public GetDataTask() {
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected void onPreExecute (){
            showProgress(true);
        }

        @Override
        @SuppressWarnings("unchecked")
        protected HttpResponse<GetDataForAddCashFlowResponseDto> doInBackground(Void... voids) {

            SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(AddExpensesActivity.this);

            String lastTimeStr = sp.getString(PREF_ADD_EXPENSES_DATA_LAST_TIME, "");
            boolean shouldDownload = false;
            if (lastTimeStr.equals("")) {
                shouldDownload = true;
            } else {
                shouldDownload = DateHelper.shouldRefresh(lastTimeStr);
            }

            if (!shouldDownload) {

                String dataSerialized = sp.getString(PREF_ADD_EXPENSES_DATA, "");

                if (dataSerialized != null) {
                    Gson gson = new Gson();
                    GetDataForAddCashFlowResponseDto dto = gson.fromJson(dataSerialized, GetDataForAddCashFlowResponseDto.class);
                    if (dto != null)
                        return new HttpResponse<>(HttpURLConnection.HTTP_OK, dto);
                }
            }


            HttpResponse<GetDataForAddCashFlowResponseDto> response = HttpResponder.askForDtoFromJson(() -> {
                try {
                    return mRepository.getCashFlowsRepository().GetDataForAdd(mHouseholdId, mLogin, mKey);
                } catch (RepositoryException ex) {
                    return null;
                }
            }, GetDataForAddCashFlowResponseDto.class);

            if (response.getCode() == HttpURLConnection.HTTP_OK
                    && response.getObject() != null) {
                Gson gson = new Gson();
                String serialized = gson.toJson(response.getObject());
                String date = DateHelper.getCurrentDate();
                GetCategoriesResponseDto categoriesDto = new GetCategoriesResponseDto();
                categoriesDto.Categories = response.getObject().Categories;
                String serializedCategories = gson.toJson(categoriesDto);
                sp.edit().putString(PREF_ADD_EXPENSES_DATA_LAST_TIME, date)
                        .putString(PREF_ADD_EXPENSES_DATA, serialized)
                        .putString(CategoriesFragment.GetCategoriesTask.PREF_CATEGORIES_DATA_LAST_TIME, date)
                        .putString(CategoriesFragment.GetCategoriesTask.PREF_CATEGORIES_DATA, serializedCategories)
                        .apply();
            }

            return response;
        }

        @Override
        protected void onPostExecute(final HttpResponse<GetDataForAddCashFlowResponseDto> success) {
            if (success != null
                    && success.getCode() == HttpURLConnection.HTTP_OK
                    && success.getObject() != null) {
                mCategories = success.getObject().Categories;
                mCategoriesStrings = new ArrayList<>();
                for (Category category : mCategories) {
                    mCategoriesStrings.add(category.Name);
                }

                ArrayAdapter<String> categoryDataAdapter = new ArrayAdapter<>(AddExpensesActivity.this, android.R.layout.simple_spinner_item, mCategoriesStrings);
                mGlobalCategorySpinner.setAdapter(categoryDataAdapter);
                mCategoryDetailSpinner.setAdapter(categoryDataAdapter);


                RecyclerView detailsRecycler = findViewById(R.id.add_expenses_list_of_details);
                detailsRecycler.setHasFixedSize(true);
                RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(AddExpensesActivity.this);
                detailsRecycler.setLayoutManager(layoutManager);
                mCashFlowDetails = new ArrayList<>();
                mDetailAdapter = new CashflowListItemAdapter(mCashFlowDetails, mCategories);
                detailsRecycler.setAdapter(mDetailAdapter);

                mWallets = success.getObject().Wallets;
                mWalletsStrings = new ArrayList<>();
                for (Wallet wallet : mWallets) {
                    mWalletsStrings.add(wallet.Name);
                }
                mDetailAdapter.setCurrency(mWallets.get(0).Money.Currency);

                ArrayAdapter<String> walletDataAdapter = new ArrayAdapter<>(AddExpensesActivity.this, android.R.layout.simple_spinner_item, mWalletsStrings);
                mWalletsSpinner.setAdapter(walletDataAdapter);
            } else {
                Toast.makeText(AddExpensesActivity.this, getString(R.string.error_add_expenses_cannot_download_data), Toast.LENGTH_SHORT).show();
                mFatalError = true;
                mbDetail.validateForm(true);
                bCf.validateForm(true);
            }
            showProgress(false);
        }

        @Override
        protected void onCancelled() {
            mGetDataTask = null;
            showProgress(false);
        }
    }

    public class AddCashFlowTask extends AsyncTask<Void, Void, Integer> {

        private final Repository mRepository;
        AddCashFlowDto mDto;

        public AddCashFlowTask(AddCashFlowDto dto){
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
            return HttpResponder.ask(() -> { try {
                return mRepository.getCashFlowsRepository().Add(mLogin, mKey, mDto);
            } catch (Exception ex) {
                return null;
            }});
        }

        @Override
        protected void onPostExecute(final Integer success) {
            if (success != null && success == HttpURLConnection.HTTP_OK) {
                Intent intent = new Intent(AddExpensesActivity.this, HomepageActivity.class);
                intent.putExtra(EXTRA_LOGIN, mLogin);
                intent.putExtra(EXTRA_KEY, mKey);
                intent.putExtra(EXTRA_HOUSEHOLD_ID, mHouseholdId);
                intent.putExtra(EXTRA_BILL_ADDED, mDto.Amount.Amount);
                intent.putExtra(EXTRA_BILL_CURRENCY, mDto.Amount.Currency);
                startActivity(intent);
                finish();
            }
            else {
                mBtnAddCashflow.setError(getString(R.string.error_other));
            }
        }

        @Override
        protected void onCancelled() {
            mAddCashFlowTask = null;
            showProgress(false);
        }
    }
}
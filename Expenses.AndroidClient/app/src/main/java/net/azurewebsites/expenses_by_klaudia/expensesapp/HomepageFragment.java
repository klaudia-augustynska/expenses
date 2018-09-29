package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.accounts.Account;
import android.accounts.AccountManager;
import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.Bundle;
import android.app.Fragment;
import android.preference.PreferenceManager;
import android.support.annotation.Nullable;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.google.gson.Gson;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.AppAuthenticator;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.DateHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.model.CURRENCY_CODE;
import net.azurewebsites.expenses_by_klaudia.model.GetCashSummaryResponseDto;
import net.azurewebsites.expenses_by_klaudia.model.Money;
import net.azurewebsites.expenses_by_klaudia.model.Wallet;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;

import java.net.HttpURLConnection;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class HomepageFragment extends Fragment {
    private static final String ARG_SECTION_NUMBER = "section_number";
    static final String ARG_LOGIN = "login";
    static final String ARG_KEY = "key";
    static final String ARG_HOUSEHOLD_ID = "householdId";
    static final String ARG_BILL_ADDED = "billAdded";
    static final String ARG_BILL_CURRENCY = "billAddedCurrency";
    public static final String PREF_SUMMARY_LAST_TIME = "PREF_SUMMARY_LAST_TIME";
    public static final String PREF_SUMMARY_DATA = "PREF_SUMMARY_DATA";

    TextView mTxtHelloLogin;
    TextView mTxtHouseholdExpenses;
    TextView mTxtHouseholdMoney;
    TextView mTxtUserMoney;
    TextView mTxtUserExpenses;
    TextView mTxtUserCharges;
    GetCashSummaryTask mGetCashSummaryTask;
    View mFormView;
    View mProgressView;
    View mFragment;
    View mHouseholdExpensesGroup;
    View mHouseholdMoneyGroup;
    View mUserChargesGroup;
    Button mBtnSync;

    public static HomepageFragment newInstance(int sectionNumber, String login, String key, String householdId, double billAdded, CURRENCY_CODE billCurrency) {
        HomepageFragment fragment = new HomepageFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_SECTION_NUMBER, sectionNumber);
        args.putString(ARG_LOGIN, login);
        args.putString(ARG_KEY, key);
        args.putString(ARG_HOUSEHOLD_ID, householdId);
        args.putDouble(ARG_BILL_ADDED, billAdded);
        args.putSerializable(ARG_BILL_CURRENCY, billCurrency);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        mFragment = inflater.inflate(R.layout.fragment_homepage, container, false);

        String login = getArguments().getString(ARG_LOGIN);
        mFormView = mFragment.findViewById(R.id.summary_form);
        mProgressView = mFragment.findViewById(R.id.summary_progress);
        mTxtHelloLogin = mFragment.findViewById(R.id.summary_household_hello);
        mTxtHelloLogin.setText(String.format(getString(R.string.summary_hello_login), login));
        mTxtHouseholdExpenses = mFragment.findViewById(R.id.summary_household_expenses_content);
        mTxtHouseholdMoney = mFragment.findViewById(R.id.summary_household_money_content);
        mTxtUserMoney = mFragment.findViewById(R.id.summary_user_money_content);
        mTxtUserExpenses = mFragment.findViewById(R.id.summary_user_expenses_content);
        mTxtUserCharges = mFragment.findViewById(R.id.summary_user_charges_content);
        mHouseholdExpensesGroup = mFragment.findViewById(R.id.summary_household_expenses_group);
        mHouseholdMoneyGroup = mFragment.findViewById(R.id.summary_household_money_group);
        mUserChargesGroup = mFragment.findViewById(R.id.summary_user_charges_group);
        mBtnSync = mFragment.findViewById(R.id.summary_button_refresh);
        mBtnSync.setOnClickListener((x) -> attemptSync());


        double billAdded = getArguments().getDouble(ARG_BILL_ADDED, 0);
        CURRENCY_CODE billAddedCurrency = (CURRENCY_CODE) getArguments().getSerializable(ARG_BILL_CURRENCY);

        AccountManager accountManager = AccountManager.get(getActivity());
        String accountType = getString(R.string.account_type);
        final Account account = new Account(login, accountType);

        String userBelongsToHouseholdInfo = accountManager.getUserData(account, AppAuthenticator.ACCOUNT_BELONGS_TO_HOUSEHOLD);
        boolean userBelongsToHousehold = userBelongsToHouseholdInfo != null && userBelongsToHouseholdInfo.equals(AppAuthenticator.ACCOUNT_VALUE_TRUE);

        if (userBelongsToHousehold) {
            mHouseholdExpensesGroup.setVisibility(View.VISIBLE);
            mHouseholdMoneyGroup.setVisibility(View.VISIBLE);
            mUserChargesGroup.setVisibility(View.VISIBLE);

        } else {
            mHouseholdExpensesGroup.setVisibility(View.GONE);
            mHouseholdMoneyGroup.setVisibility(View.GONE);
            mUserChargesGroup.setVisibility(View.GONE);
        }

        boolean shouldRefresh = !tryGetFromSharedPreferences(billAdded > 0, billAdded, billAddedCurrency);

        if (shouldRefresh) {
            mGetCashSummaryTask = new GetCashSummaryTask();
            mGetCashSummaryTask.execute();
        }

        return mFragment;
    }

    private void attemptSync() {
        if (mGetCashSummaryTask != null)
            return;
        mGetCashSummaryTask = new GetCashSummaryTask();
        mGetCashSummaryTask.execute();
    }

    /**
     * @return false if couldn't read from preferences
     */
    private boolean tryGetFromSharedPreferences(boolean ignoreCheckTime, double billAdded, CURRENCY_CODE billCurrency) {
        SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getActivity());

        if (!ignoreCheckTime) {
            String lastTimeStr = sp.getString(PREF_SUMMARY_LAST_TIME, "DEFAULT");
            if (TextUtils.isEmpty(lastTimeStr))
                return false;
            if (DateHelper.shouldRefresh(lastTimeStr))
                return false;
        }

        String dataSerialized = sp.getString(PREF_SUMMARY_DATA, "");

        if (dataSerialized != null) {
            Gson gson = new Gson();
            GetCashSummaryResponseDto dto = gson.fromJson(dataSerialized, GetCashSummaryResponseDto.class);

            boolean billWasAdded = billAdded > 0 && billCurrency != null;
            if (billWasAdded) {
                if (dto.HouseholdExpenses != null)
                    for (Money money : dto.HouseholdExpenses)
                        if (money.Currency.equals(billCurrency))
                            money.Amount += billAdded;
                else {
                    dto.HouseholdExpenses = new ArrayList<>();
                    Money m = new Money();
                    m.Amount = billAdded;
                    m.Currency = billCurrency;
                    dto.HouseholdExpenses.add(m);
                        }
                if (dto.HouseholdMoney != null)
                    for (Money money : dto.HouseholdMoney)
                        if (money.Currency.equals(billCurrency))
                            money.Amount -= billAdded;
                if (dto.UserExpenses != null)
                    for (Money money : dto.UserExpenses)
                        if (money.Currency.equals(billCurrency))
                            money.Amount += billAdded;
                else {
                    dto.UserExpenses = new ArrayList<>();
                    Money m = new Money();
                    m.Amount = billAdded;
                    m.Currency = billCurrency;
                    dto.UserExpenses.add(m);
                        }
                if (dto.UserWallets != null)
                    for (Wallet wallet : dto.UserWallets)
                        if (wallet.Money.Currency.equals(billCurrency))
                            wallet.Money.Amount -= billAdded;
            }

            setDto(dto, billWasAdded);
            return true;
        }
        return false;
    }

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        ((HomepageActivity) activity).onSectionAttached(
                getArguments().getInt(ARG_SECTION_NUMBER));
    }

    @Override
    public void onViewCreated(View view, @Nullable Bundle savedInstanceState) {
        String login = getArguments().getString(ARG_LOGIN);
        String key = getArguments().getString(ARG_KEY);
        String householdId = getArguments().getString(ARG_HOUSEHOLD_ID);
        ImageView imageView = getView().findViewById(R.id.add_expenses_img);
        imageView.setOnClickListener((x) -> {
            Intent intent = new Intent(getActivity(), AddExpensesActivity.class);
            intent.putExtra(SplashActivity.EXTRA_LOGIN, login);
            intent.putExtra(SplashActivity.EXTRA_KEY, key);
            intent.putExtra(SplashActivity.EXTRA_HOUSEHOLD_ID, householdId);
            startActivity(intent);
        });
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mFormView, mProgressView);
    }

    private void setDto(GetCashSummaryResponseDto dto, boolean wasBillAdded) {
        mTxtHouseholdExpenses.setText(formatMoneyList(dto.HouseholdExpenses));
        mTxtHouseholdMoney.setText(formatMoneyList(dto.HouseholdMoney));
        mTxtUserMoney.setText(formatWalletList(dto.UserWallets));
        mTxtUserExpenses.setText(formatMoneyList(dto.UserExpenses));
        mTxtUserCharges.setText(formatDictOfListOfMoney(dto.UserCharges));
        if (wasBillAdded)
            mTxtUserCharges.setText(getString(R.string.summary_info_charges));
    }

    private String formatMoneyList(List<Money> list) {
        StringBuilder sb = new StringBuilder();
        if (list == null || list.size() == 0){
            sb.append(getString(R.string.summary_info_no_expenses));
        }
        else {
            int i = 0;
            for (Money item : list) {
                if (i++ > 0)
                    sb.append(", ");
                sb.append(item.Amount);
                sb.append(' ');
                sb.append(item.Currency.toString());
            }
        }
        return sb.toString();
    }

    private String formatWalletList(List<Wallet> list) {
        StringBuilder sb = new StringBuilder();
        if (list != null && list.size() > 0) {
            int i = 0;
            for (Wallet item : list) {
                if (i++ > 0)
                    sb.append(", ");
                sb.append(item.Name);
                sb.append(": ");
                sb.append(item.Money.Amount);
                sb.append(' ');
                sb.append(item.Money.Currency.toString());
            }
        } else {
            sb.append(getString(R.string.summary_info_sync));
        }
        return sb.toString();
    }

    private String formatDictOfListOfMoney(HashMap<String, List<Money>> dict) {
        StringBuilder sb = new StringBuilder();
        boolean wasSth = false;
        if (dict != null && dict.size() > 0) {
            int i = 0;
            for (Map.Entry<String, List<Money>> keyvalue : dict.entrySet()) {
                if (i++ > 0)
                    sb.append("\n");
                if (keyvalue.getValue().size() > 0
                        && firstOrDefaultWhereAmountOtherThanZero(keyvalue.getValue()) != null) {
                    sb.append(keyvalue.getKey());
                    sb.append(": ");
                    wasSth = true;
                } else {
                    continue;
                }
                int j = 0;
                for (Money money : keyvalue.getValue()) {
                    if (money.Amount == 0)
                        continue;
                    if (j++ > 0)
                        sb.append(", ");
                    if (money.Amount < 0)
                    {
                        sb.append(getString(R.string.summary_info_debt));
                        sb.append(' ');
                        sb.append(money.Amount * -1);
                        sb.append(' ');
                        sb.append(money.Currency.toString());
                    }
                    else
                    {
                        sb.append(getString(R.string.summary_info_lending));
                        sb.append(' ');
                        sb.append(money.Amount);
                        sb.append(' ');
                        sb.append(money.Currency.toString());
                    }
                }
            }
        }
        if (!wasSth)
            sb.append(getString(R.string.summary_info_no_debts));
        return sb.toString();
    }

    private Money firstOrDefaultWhereAmountOtherThanZero(List<Money> list) {
        for (Money item : list) {
            if (item.Amount != 0)
                return item;
        }
        return null;
    }

    public class GetCashSummaryTask extends AsyncTask<Void, Void, HttpResponse<GetCashSummaryResponseDto>> {

        private final Repository mRepository;


        String mLogin;
        String mKey;
        String mHouseholdId;

        public GetCashSummaryTask() {
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
            mLogin = getArguments().getString(ARG_LOGIN);
            mKey = getArguments().getString(ARG_KEY);
            mHouseholdId = getArguments().getString(ARG_HOUSEHOLD_ID);
        }

        @Override
        protected void onPreExecute (){
            showProgress(true);
        }

        @Override
        @SuppressWarnings("unchecked")
        protected HttpResponse<GetCashSummaryResponseDto> doInBackground(Void... voids) {
            return HttpResponder.askForDtoFromJson(() -> {
                try {
                    return mRepository.getCashFlowsRepository().GetSummary(mHouseholdId, mLogin, DateHelper.getDateThatBeginsTheMonth(), DateHelper.getDateThatEndsTheMonth(), mKey);
                } catch (Exception ex) {
                    return null;
                }
            }, GetCashSummaryResponseDto.class);
        }

        @Override
        protected void onPostExecute(final HttpResponse<GetCashSummaryResponseDto> success) {
            if (success != null
                    && success.getCode() == HttpURLConnection.HTTP_OK) {
                setDto(success.getObject(), false);

                SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getActivity());
                Gson gson = new Gson();
                String json = gson.toJson(success.getObject());
                sp.edit().putString(PREF_SUMMARY_LAST_TIME, DateHelper.getCurrentDate())
                        .putString(PREF_SUMMARY_DATA, json)
                        .apply();
            }
            else {
                Toast.makeText(getContext(), getString(R.string.error_other), Toast.LENGTH_SHORT).show();
            }
            showProgress(false);
            mGetCashSummaryTask = null;
        }

        @Override
        protected void onCancelled() {
            mGetCashSummaryTask = null;
            showProgress(false);
        }
    }
}

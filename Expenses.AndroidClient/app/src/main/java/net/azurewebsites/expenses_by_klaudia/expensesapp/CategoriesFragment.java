package net.azurewebsites.expenses_by_klaudia.expensesapp;


import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.os.Bundle;
import android.app.Fragment;
import android.preference.PreferenceManager;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Toast;

import com.google.gson.Gson;

import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.DateHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponder;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.HttpResponse;
import net.azurewebsites.expenses_by_klaudia.expensesapp.helpers.ProgressHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.BindRulesToActivityHelper;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationResult;
import net.azurewebsites.expenses_by_klaudia.expensesapp.validation.ValidationUseCases;
import net.azurewebsites.expenses_by_klaudia.model.AddCategoryDto;
import net.azurewebsites.expenses_by_klaudia.model.Category;
import net.azurewebsites.expenses_by_klaudia.model.GetCategoriesResponseDto;
import net.azurewebsites.expenses_by_klaudia.model.GetDataForAddCashFlowResponseDto;
import net.azurewebsites.expenses_by_klaudia.repository.Categories;
import net.azurewebsites.expenses_by_klaudia.repository.Repository;
import net.azurewebsites.expenses_by_klaudia.repository.RepositoryException;

import java.net.HttpURLConnection;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.function.Function;


/**
 * A simple {@link Fragment} subclass.
 */
public class CategoriesFragment extends Fragment {

    private static final String ARG_SECTION_NUMBER = "section_number";
    static final String ARG_LOGIN = "login";
    static final String ARG_KEY = "key";

    View mFormView;
    View mProgressView;
    View mFragment;
    RecyclerView mCategoriesView;
    List<Category> mCategoriesValues;
    CategoriesListItemAdapter mCategoriesAdapter;
    boolean mFatalError;
    GetCategoriesTask mGetCategoriesTask;
    EditText mTxtCatName;
    Button mBtnAddCategory;
    BindRulesToActivityHelper mbAddCat;
    AddCategoryTask mAddCategoryTask;

    public static CategoriesFragment newInstance(int sectionNumber, String login, String key) {
        CategoriesFragment fragment = new CategoriesFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_SECTION_NUMBER, sectionNumber);
        args.putString(ARG_LOGIN, login);
        args.putString(ARG_KEY, key);
        fragment.setArguments(args);
        return fragment;
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        mFragment = inflater.inflate(R.layout.fragment_categories, container, false);
        mFormView = mFragment.findViewById(R.id.categories_form);
        mProgressView = mFragment.findViewById(R.id.categories_progress);

        mCategoriesView = mFragment.findViewById(R.id.categories_list_of_categories);
        mGetCategoriesTask = new GetCategoriesTask();
        mGetCategoriesTask.execute();

        mTxtCatName = mFragment.findViewById(R.id.categories_name);
        mBtnAddCategory = mFragment.findViewById(R.id.categories_add_category);
        mBtnAddCategory.setOnClickListener((x) -> attemptAddCategory());

        ValidationUseCases useCases = new ValidationUseCases(this::getString);
        mbAddCat = new BindRulesToActivityHelper(mBtnAddCategory, getContext());
        mbAddCat.add(mTxtCatName, useCases::isCategoryValid);
        mbAddCat.addRule((x) -> mFatalError
        ? new ValidationResult(false, getString(R.string.error_other))
        : new ValidationResult(true, null));
        mbAddCat.validateForm();

        return mFragment;
    }

    private void attemptAddCategory() {
        if (mAddCategoryTask != null)
            return;
        mAddCategoryTask = new AddCategoryTask();
        mAddCategoryTask.execute();
    }

    private void showProgress(final boolean show) {
        ProgressHelper.showProgress(show, mFormView, mProgressView);
    }

    public class GetCategoriesTask extends AsyncTask<Void, Void, HttpResponse<GetCategoriesResponseDto>> {

        private final Repository mRepository;

        public final static String PREF_CATEGORIES_DATA = "PREF_CATEGORIES_DATA";
        public final static String PREF_CATEGORIES_DATA_LAST_TIME = "PREF_CATEGORIES_DATA_LAST_TIME";

        public GetCategoriesTask() {
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }

        @Override
        protected void onPreExecute() {
            showProgress(true);
        }

        @Override
        @SuppressWarnings("unchecked")
        protected HttpResponse<GetCategoriesResponseDto> doInBackground(Void... voids) {
            SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getContext());

            String lastTimeStr = sp.getString(PREF_CATEGORIES_DATA_LAST_TIME, "");
            boolean shouldDownload = false;
            if (lastTimeStr.equals("")) {
                shouldDownload = true;
            } else {
                shouldDownload = DateHelper.shouldRefresh(lastTimeStr);
            }

            if (!shouldDownload) {

                String dataSerialized = sp.getString(PREF_CATEGORIES_DATA, "");

                if (dataSerialized != null) {
                    Gson gson = new Gson();
                    GetCategoriesResponseDto dto = gson.fromJson(dataSerialized, GetCategoriesResponseDto.class);
                    if (dto != null)
                        return new HttpResponse<>(HttpURLConnection.HTTP_OK, dto);
                }
            }


            HttpResponse<GetCategoriesResponseDto> response = HttpResponder.askForDtoFromJson(() -> {
                try {
                    String login = getArguments().getString(ARG_LOGIN);
                    String key = getArguments().getString(ARG_KEY);
                    return mRepository.GetCategoriesRepository().GetAll(login, key);
                } catch (RepositoryException ex) {
                    return null;
                }
            }, GetCategoriesResponseDto.class);

            if (response.getCode() == HttpURLConnection.HTTP_OK
                    && response.getObject() != null) {
                Gson gson = new Gson();
                String serialized = gson.toJson(response.getObject());
                String date = DateHelper.getCurrentDate();
                sp.edit().putString(PREF_CATEGORIES_DATA_LAST_TIME, date)
                        .putString(PREF_CATEGORIES_DATA, serialized)
                        .apply();
            }

            return response;
        }


        @Override
        protected void onPostExecute(final HttpResponse<GetCategoriesResponseDto> success) {

            if (success != null
                    && success.getCode() == HttpURLConnection.HTTP_OK
                    && success.getObject() != null) {
                mCategoriesView.setHasFixedSize(true);
                RecyclerView.LayoutManager layoutManager = new LinearLayoutManager(getContext());
                mCategoriesView.setLayoutManager(layoutManager);
                mCategoriesValues = success.getObject().Categories;
                mCategoriesAdapter = new CategoriesListItemAdapter(mCategoriesValues);
                mCategoriesView.setAdapter(mCategoriesAdapter);
            } else {
                Toast.makeText(getContext(), getString(R.string.error_categories_cant_download), Toast.LENGTH_SHORT).show();
                mFatalError = true;
                mbAddCat.validateForm(true);
            }
            showProgress(false);
        }

        @Override
        protected void onCancelled() {
            mGetCategoriesTask = null;
            showProgress(false);
        }
    }

    public class AddCategoryTask extends AsyncTask<Void, Void, Integer> {

        private final Repository mRepository;

        public AddCategoryTask() {
            String repositoryHost = getString(R.string.repository_host);
            mRepository = new Repository(repositoryHost);
        }


        @Override
        protected void onPreExecute() {
            showProgress(true);
        }


        @Override
        protected Integer doInBackground(Void... voids) {
            return HttpResponder.ask(() -> {
                try {
                    String login = getArguments().getString(ARG_LOGIN);
                    String key = getArguments().getString(ARG_KEY);
                    return mRepository.GetCategoriesRepository().Add(login, key, mTxtCatName.getText().toString(), null);
                } catch (Exception ex) {
                    return null;
                }
            });
        }

        @Override
        protected void onPostExecute(final Integer success) {

            if (success != null && success == HttpURLConnection.HTTP_OK) {
                Category newCat = new Category();
                newCat.Name = mTxtCatName.getText().toString();
                mCategoriesValues.add(newCat);
                mCategoriesAdapter.notifyDataSetChanged();

                SharedPreferences sp = PreferenceManager.getDefaultSharedPreferences(getActivity());
                Gson gson = new Gson();
                GetCategoriesResponseDto dto = new GetCategoriesResponseDto();
                dto.Categories = mCategoriesValues;
                String json = gson.toJson(dto);
                sp.edit().putString(GetCategoriesTask.PREF_CATEGORIES_DATA_LAST_TIME, DateHelper.getMinDate())
                        .putString(GetCategoriesTask.PREF_CATEGORIES_DATA, json)
                        .putString(AddExpensesActivity.GetDataTask.PREF_ADD_EXPENSES_DATA_LAST_TIME, DateHelper.getMinDate())
                        .apply();
            } else {
                Toast.makeText(getContext(), getString(R.string.error_other), Toast.LENGTH_SHORT).show();
            }

            showProgress(false);
        }

        @Override
        protected void onCancelled() {
            mAddCategoryTask = null;
            showProgress(false);
        }

    }
}


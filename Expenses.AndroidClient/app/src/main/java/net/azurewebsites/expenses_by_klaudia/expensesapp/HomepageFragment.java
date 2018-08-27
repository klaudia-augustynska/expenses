package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.app.Fragment;
import android.support.annotation.Nullable;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;

public class HomepageFragment extends Fragment {
    private static final String ARG_SECTION_NUMBER = "section_number";
    static final String ARG_LOGIN = "login";
    static final String ARG_KEY = "key";
    static final String ARG_HOUSEHOLD_ID = "householdId";

    public static HomepageFragment newInstance(int sectionNumber, String login, String key, String householdId) {
        HomepageFragment fragment = new HomepageFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_SECTION_NUMBER, sectionNumber);
        args.putString(ARG_LOGIN, login);
        args.putString(ARG_KEY, key);
        args.putString(ARG_HOUSEHOLD_ID, householdId);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_homepage, container, false);
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

}

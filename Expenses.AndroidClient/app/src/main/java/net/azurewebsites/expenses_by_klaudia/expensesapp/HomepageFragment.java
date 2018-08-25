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

    public static HomepageFragment newInstance(int sectionNumber) {
        HomepageFragment fragment = new HomepageFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_SECTION_NUMBER, sectionNumber);
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
        ImageView imageView = getView().findViewById(R.id.add_expenses_img);
        imageView.setOnClickListener((x) -> {
            Intent intent = new Intent(getActivity(), AddExpensesActivity.class);
            startActivity(intent);
        });
    }

}

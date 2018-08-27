package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.content.Context;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.Button;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import net.azurewebsites.expenses_by_klaudia.expensesapp.AddExpensesActivity;
import net.azurewebsites.expenses_by_klaudia.expensesapp.R;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.function.Function;
import java.util.function.Supplier;

public class BindRulesToActivityHelper {

    private HashMap<TextView, Function<String,ValidationResult>> mBindings;
    private List<Function<Object,ValidationResult>> mCheckAfter;
    private Button mButton;
    private Context mContext;

    public BindRulesToActivityHelper(Button button, Context context){
        mBindings = new HashMap<>();
        mCheckAfter = new ArrayList<>();
        mButton = button;
        mContext = context;
    }

    public void add(TextView textView, Function<String,ValidationResult> rules) {
        textView.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence charSequence, int i, int i1, int i2) {

            }

            @Override
            public void onTextChanged(CharSequence charSequence, int i, int i1, int i2) {

            }

            @Override
            public void afterTextChanged(Editable editable) {
                validateForm(true);
            }
        });
//        textView.setOnFocusChangeListener(new View.OnFocusChangeListener() {
//            @Override
//            public void onFocusChange(View v, boolean hasFocus) {
//                if (!hasFocus)
//                    validateForm(true);
//            }
//        });
        mBindings.put(textView, rules);
    }

    public ValidationResult validateField(TextView textView, boolean showErrors) {
        Function<String,ValidationResult> function = mBindings.get(textView);
        ValidationResult validationResult = function.apply(textView.getText().toString());
        if (showErrors){
            textView.setError(null);
            if (!validationResult.getSuccess()) {
                textView.setError(validationResult.getErrorMsg());
            }
        }
        return validationResult;
    }

    public void validateForm() {
        validateForm(false);
    }

    public void validateForm(boolean showErrors) {
        boolean success = true;
        for (TextView key : mBindings.keySet()) {
            ValidationResult result = validateField(key, showErrors);
            if (!result.getSuccess()) {
                success = false;
                if (!showErrors)
                    break;
            }
        }

        if (success) {
            mButton.setError(null);
            if (mCheckAfter != null) {
                for (Function<Object,ValidationResult> func : mCheckAfter) {
                    ValidationResult result = func.apply(null);
                    if (!result.getSuccess() && showErrors) {
                        success = false;
                        if (showErrors) {
                            mButton.setError(result.getErrorMsg());
                            Toast.makeText(mContext, result.getErrorMsg(), Toast.LENGTH_SHORT).show();
                        }
                        break;
                    }
                }
            }
        }
        mButton.setEnabled(success);
    }

    public void addRule(Function<Object,ValidationResult> func) {
        mCheckAfter.add(func);
    }
}
package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import java.util.HashMap;
import java.util.function.Function;

public class BindRulesToActivityHelper {

    private HashMap<TextView, Function<String,ValidationResult>> mBindings;
    private Button mButton;

    public BindRulesToActivityHelper(Button button){
        mBindings = new HashMap<>();
        mButton = button;
    }

    public void add(TextView textView, Function<String,ValidationResult> rules) {
        textView.setOnFocusChangeListener(new View.OnFocusChangeListener() {
            @Override
            public void onFocusChange(View v, boolean hasFocus) {
                if (!hasFocus)
                    validateForm(true);
            }
        });
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
        mButton.setEnabled(success);
    }

}
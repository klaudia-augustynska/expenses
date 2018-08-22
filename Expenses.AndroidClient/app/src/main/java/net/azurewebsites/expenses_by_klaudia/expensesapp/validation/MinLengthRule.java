package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.text.TextUtils;

import net.azurewebsites.expenses_by_klaudia.expensesapp.R;

import java.util.function.Function;

public class MinLengthRule extends ValidationRule {

    private int mMinLength;

    public MinLengthRule(Function<Integer, String> getStringDelegate, int minLength) {
        super(getStringDelegate);
        mMinLength = minLength;
    }

    @Override
    public boolean validate(String value) {
        return TextUtils.isEmpty(value)
            || value.length() >= mMinLength;
    }

    @Override
    public String getErrorMsg() {
        return String.format(mGetStringDelegate.apply(R.string.error_field_too_short), mMinLength);
    }
}

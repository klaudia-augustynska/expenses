package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.text.TextUtils;

import net.azurewebsites.expenses_by_klaudia.expensesapp.R;

import java.util.function.Function;

public final class RequiredRule extends ValidationRule {

    public RequiredRule(Function<Integer, String> getStringDelegate) {
        super(getStringDelegate);
    }

    @Override
    public boolean validate(String value) {
        return !TextUtils.isEmpty(value);
    }

    @Override
    public String getErrorMsg() {
        return mGetStringDelegate.apply(R.string.error_field_required);
    }
}

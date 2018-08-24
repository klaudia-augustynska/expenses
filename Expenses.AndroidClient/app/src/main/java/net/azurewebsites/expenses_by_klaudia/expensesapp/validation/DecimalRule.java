package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.text.TextUtils;

import net.azurewebsites.expenses_by_klaudia.expensesapp.R;

import java.util.function.Function;

public class DecimalRule extends ValidationRule {
    public DecimalRule(Function<Integer, String> getStringDelegate) {
        super(getStringDelegate);
    }

    @Override
    public boolean validate(String value) {
        if (TextUtils.isEmpty(value))
            return true;
        String[] split = value.split(".");
        if (split.length > 2)
            return false;
        for (String part : split) {
            for (int i = 0; i < part.length(); ++i) {
                char c = part.charAt(i);
                if (c <= '0' || c >= '9')
                    return false;
            }
        }
        try {
            double decimal = Double.parseDouble(value);
        }
        catch (Exception ex) {
            return false;
        }
        return true;
    }

    @Override
    public String getErrorMsg() {
        return mGetStringDelegate.apply(R.string.error_field_not_decimal);
    }
}

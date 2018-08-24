package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.text.TextUtils;

import net.azurewebsites.expenses_by_klaudia.expensesapp.R;

import java.util.Locale;
import java.util.function.Function;

public class RangeRuleDecimal extends DecimalRule {

    Double mMinValue;
    Double mMaxValue;
    boolean mDecimalNotInRange;

    public RangeRuleDecimal(Function<Integer, String> getStringDelegate, Double minValue, Double maxValue) {
        super(getStringDelegate);
        mMinValue = minValue;
        mMaxValue = maxValue;
        mDecimalNotInRange = true;
    }

    @Override
    public boolean validate(String value) {
        if (TextUtils.isEmpty(value))
            return true;
        boolean isDecimal = super.validate(value);
        if (!isDecimal) {
            mDecimalNotInRange = false;
            return false;
        }
        double decimal = Double.parseDouble(value);
        boolean ok = true;
        if (mMinValue != null) {
            ok = decimal >= mMinValue;
        }
        if (mMaxValue != null) {
            ok = ok && decimal <= mMaxValue;
        }
        return ok;
    }

    @Override
    public String getErrorMsg() {
        if (mDecimalNotInRange) {
            if (mMinValue != null && mMaxValue != null) {
                return String.format(Locale.getDefault(), mGetStringDelegate.apply(R.string.error_field_not_in_range), mMinValue, mMaxValue);
            }
            if (mMinValue != null) {
                return String.format(Locale.getDefault(), mGetStringDelegate.apply(R.string.error_field_min_value), mMinValue);
            }
            if (mMaxValue != null) {
                return String.format(Locale.getDefault(), mGetStringDelegate.apply(R.string.error_field_max_value), mMaxValue);
            }
        }
        return super.getErrorMsg();
    }
}

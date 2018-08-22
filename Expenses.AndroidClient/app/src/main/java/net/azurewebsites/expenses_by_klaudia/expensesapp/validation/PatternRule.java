package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.text.TextUtils;

import java.util.function.Function;

public class PatternRule extends ValidationRule {

    private String mPattern;
    private int mErrorMsgId;

    public PatternRule(Function<Integer, String> getStringDelegate, String pattern, int errorMsgId) {
        super(getStringDelegate);
        mPattern = pattern;
        mErrorMsgId = errorMsgId;
    }

    @Override
    public boolean validate(String value) {
        return TextUtils.isEmpty(value)
                || value.matches(mPattern);
    }

    @Override
    public String getErrorMsg() {
        return mGetStringDelegate.apply(mErrorMsgId);
    }
}

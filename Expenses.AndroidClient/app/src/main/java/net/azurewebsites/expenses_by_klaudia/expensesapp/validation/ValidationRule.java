package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import java.util.function.Function;

public abstract class ValidationRule {

    protected Function<Integer, String> mGetStringDelegate;

    public ValidationRule(Function<Integer, String> getStringDelegate) {
        mGetStringDelegate = getStringDelegate;
    }

    public abstract boolean validate(String value);
    public abstract String getErrorMsg();
}

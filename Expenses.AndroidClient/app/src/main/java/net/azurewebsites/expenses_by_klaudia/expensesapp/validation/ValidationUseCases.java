package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import net.azurewebsites.expenses_by_klaudia.expensesapp.R;

import java.util.ArrayList;
import java.util.List;
import java.util.function.Function;

public class ValidationUseCases {

    private Function<Integer, String> mGetStringDelegate;

    public ValidationUseCases(Function<Integer, String> getStringDelegate) {
        mGetStringDelegate = getStringDelegate;
    }

    private List<ValidationRule> mLoginRules;
    public ValidationResult isLoginValid(String value) {
        if (mLoginRules == null) {
            mLoginRules = new ArrayList<>();
            mLoginRules.add(new RequiredRule(mGetStringDelegate));
            mLoginRules.add(new PatternRule(mGetStringDelegate, "^[a-zA-Z0-9-\\._~]{3,20}$", R.string.error_login_bad_format));
        }
        return performValidation(mLoginRules, value);
    }

    private List<ValidationRule> mPasswordRules;
    public ValidationResult isPasswordValid(String value) {
        if (mPasswordRules == null) {
            mPasswordRules = new ArrayList<>();
            mPasswordRules.add(new RequiredRule(mGetStringDelegate));
            mPasswordRules.add(new MinLengthRule(mGetStringDelegate, 5));
        }
        return performValidation(mPasswordRules, value);
    }

    private ValidationResult performValidation(List<ValidationRule> rules, String value) {
        for (ValidationRule rule : rules) {
            if (!rule.validate(value))
            {
                return new ValidationResult(false, rule.getErrorMsg());
            }
        }
        return new ValidationResult(true);
    }
}

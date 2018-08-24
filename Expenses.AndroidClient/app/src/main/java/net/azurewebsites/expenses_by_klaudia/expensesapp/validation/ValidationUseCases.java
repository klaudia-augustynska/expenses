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
        List<ValidationRule> rules = mLoginRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new PatternRule(mGetStringDelegate, "^[a-zA-Z0-9-\\._~]{3,20}$", R.string.error_login_bad_format));
        }
        return performValidation(rules, value);
    }

    private List<ValidationRule> mPasswordRules;
    public ValidationResult isPasswordValid(String value) {
        List<ValidationRule> rules = mPasswordRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new MinLengthRule(mGetStringDelegate, 5));
        }
        return performValidation(rules, value);
    }

    private List<ValidationRule> mWalletNameRules;
    public ValidationResult isWalletNameValid(String value) {
        List<ValidationRule> rules = mWalletNameRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new MinLengthRule(mGetStringDelegate, 3));
        }
        return performValidation(rules, value);
    }

    private List<ValidationRule> mMoneyRules;
    public ValidationResult isMoneyValid(String value) {
        List<ValidationRule> rules = mMoneyRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new DecimalRule(mGetStringDelegate));
        }
        return performValidation(rules, value);
    }

    private List<ValidationRule> mWeightRules;
    public ValidationResult isWeightValid(String value) {
        List<ValidationRule> rules = mWeightRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new RangeRuleDecimal(mGetStringDelegate, 20d, 600d));
        }
        return performValidation(rules, value);
    }

    private List<ValidationRule> mHeightRules;
    public ValidationResult isHeightValid(String value) {
        List<ValidationRule> rules = mHeightRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new RangeRuleDecimal(mGetStringDelegate, 54d, 275d));
        }
        return performValidation(rules, value);
    }

    private List<ValidationRule> mNameRules;
    public ValidationResult isNameValid(String value) {
        List<ValidationRule> rules = mNameRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new MinLengthRule(mGetStringDelegate, 3));
            rules.add(new PatternRule(mGetStringDelegate, "^[A-Za-ząłóńźżęś]{3,}[A-Za -ząłóńźżęś]*$", R.string.error_invalid_name));
        }
        return performValidation(rules, value);
    }

    private List<ValidationRule> mDateRules;
    public ValidationResult isDateValid(String value) {
        List<ValidationRule> rules = mDateRules;
        if (rules == null) {
            rules = new ArrayList<>();
            rules.add(new RequiredRule(mGetStringDelegate));
            rules.add(new PatternRule(mGetStringDelegate, "^\\d{4}-\\d{1,2}-\\d{1,2}$", R.string.error_invalid_date));
        }
        return performValidation(rules, value);
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

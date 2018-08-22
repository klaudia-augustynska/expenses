package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

public class ValidationResult {

    private boolean mSuccess;
    private String mErrorMsg;

    public ValidationResult(boolean success) {
        mSuccess = success;
        mErrorMsg = null;
    }

    public ValidationResult(boolean success, String errorMsg) {
        mSuccess = success;
        mErrorMsg = errorMsg;
    }

    public boolean getSuccess() {
        return mSuccess;
    }

    public String getErrorMsg() {
        return mErrorMsg;
    }
}

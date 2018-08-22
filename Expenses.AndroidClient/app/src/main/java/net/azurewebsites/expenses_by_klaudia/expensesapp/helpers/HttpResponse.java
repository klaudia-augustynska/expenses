package net.azurewebsites.expenses_by_klaudia.expensesapp.helpers;

public class HttpResponse<T> {
    private Integer mCode;
    public Integer getCode() {
        return mCode;
    }

    private T mObject;
    public T getObject() {
        return mObject;
    }

    public HttpResponse(Integer code, T object){
        mCode = code;
        mObject = object;
    }
}

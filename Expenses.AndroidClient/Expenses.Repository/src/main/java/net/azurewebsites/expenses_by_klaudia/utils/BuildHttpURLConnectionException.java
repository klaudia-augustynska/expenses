package net.azurewebsites.expenses_by_klaudia.utils;

public class BuildHttpURLConnectionException extends Exception {
    public BuildHttpURLConnectionException(Throwable innerException) {
        super(innerException);
    }
    public BuildHttpURLConnectionException(String customMessage, Throwable innerException) {
        super(customMessage, innerException);
    }
}

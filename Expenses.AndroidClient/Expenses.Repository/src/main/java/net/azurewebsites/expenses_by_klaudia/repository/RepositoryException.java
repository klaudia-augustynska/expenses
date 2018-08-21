package net.azurewebsites.expenses_by_klaudia.repository;

public class RepositoryException extends Exception {
    public RepositoryException(Throwable innerException) {
        super(innerException);
    }
    public RepositoryException(String customMessage, Throwable innerException) {
        super(customMessage, innerException);
    }
}

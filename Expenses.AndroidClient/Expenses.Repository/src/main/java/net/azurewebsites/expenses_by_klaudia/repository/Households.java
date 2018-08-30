package net.azurewebsites.expenses_by_klaudia.repository;

import java.net.HttpURLConnection;

public class Households extends RepositoryBase {
    public Households(String host, String path) {
        super(host, path, "households");
    }

    public HttpURLConnection invite(String invitersLogin, String invitedLogin, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("invite")
                .appendPath(invitersLogin)
                .appendPath(invitedLogin)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendGet(uri);
    }

    public HttpURLConnection acceptInvitation(String from, String to, String rowkey, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("accept")
                .appendPath(from)
                .appendPath(to)
                .appendPath(rowkey)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendGet(uri);
    }

    public HttpURLConnection getMemebers(String householdId, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("members")
                .appendPath(householdId)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendGet(uri);
    }
}

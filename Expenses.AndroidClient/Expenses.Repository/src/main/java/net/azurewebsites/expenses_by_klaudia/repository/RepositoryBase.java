package net.azurewebsites.expenses_by_klaudia.repository;

import android.net.Uri;

import net.azurewebsites.expenses_by_klaudia.utils.BuildHttpURLConnectionException;
import net.azurewebsites.expenses_by_klaudia.utils.HttpURLConnectionBuilder;

import java.net.HttpURLConnection;

public abstract class RepositoryBase {

    protected String mHost, mPath, mApiRepositoryName;

    public RepositoryBase(String host, String path, String apiRepositoryName)
    {
        mHost = host;
        mPath = path;
        mApiRepositoryName = apiRepositoryName;
    }

    protected Uri.Builder GetBaseUriBuilder() {
        String[] parts = mHost.split("://");
        return new Uri.Builder()
                .scheme(parts[0])
                .encodedAuthority(parts[1])
                .appendPath(mPath)
                .appendPath(mApiRepositoryName);
    }

    protected HttpURLConnection SendJsonPost(String uri, Object dto) throws RepositoryException {
        try {
            HttpURLConnectionBuilder builder = new HttpURLConnectionBuilder(uri, true);
            builder.setJsonInput(dto);
            return builder.build();
        } catch (BuildHttpURLConnectionException ex) {
            throw new RepositoryException(ex);
        }
    }

    protected HttpURLConnection SendJsonPostWithAuthorisation(String uri, Object dto, String key) throws RepositoryException {
        try {
            HttpURLConnectionBuilder builder = new HttpURLConnectionBuilder(uri, true);
            builder.setJsonInput(dto);
            builder.setAuthorization(key);
            return builder.build();
        } catch (BuildHttpURLConnectionException ex) {
            throw new RepositoryException(ex);
        }
    }

    protected HttpURLConnection SendGet(String uri) throws RepositoryException {
        try {
            HttpURLConnectionBuilder builder = new HttpURLConnectionBuilder(uri, false);
            return builder.build();
        } catch (BuildHttpURLConnectionException ex) {
            throw new RepositoryException(ex);
        }
    }

    protected HttpURLConnection SendGetWithAuthorisation(String uri, String key) throws RepositoryException {
        try {
            HttpURLConnectionBuilder builder = new HttpURLConnectionBuilder(uri, false);
            builder.setAuthorization(key);
            return builder.build();
        } catch (BuildHttpURLConnectionException ex) {
            throw new RepositoryException(ex);
        }
    }
}

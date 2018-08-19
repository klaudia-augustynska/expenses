package net.azurewebsites.expenses_by_klaudia.repository;


import com.google.gson.Gson;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.entity.StringEntity;
import org.apache.http.impl.client.HttpClientBuilder;
import org.springframework.web.util.UriComponentsBuilder;

import java.io.IOException;
import java.io.UnsupportedEncodingException;

public abstract class RepositoryBase {

    protected String _host, _path, _apiRepositoryName;

    public RepositoryBase(String host, String path, String apiRepositoryName)
    {
        _host = host;
        _path = path;
        _apiRepositoryName = apiRepositoryName;
    }

    private UriComponentsBuilder _baseUriBuilder;
    protected UriComponentsBuilder GetBaseUriBuilder() {

        if (_baseUriBuilder == null)
        {
            _baseUriBuilder = UriComponentsBuilder
                    .fromUriString(_host)
                    .pathSegment(_path, _apiRepositoryName);
        }
        return _baseUriBuilder;
    }

    HttpClient httpClient = HttpClientBuilder.create().build();



    protected StringEntity GetJsonStringEntity(Object dto) throws RepositoryException {
        try {
            Gson gson = new Gson();
            String json = gson.toJson(dto);
            return new StringEntity(json);
        } catch (UnsupportedEncodingException ex) {
            throw new RepositoryException(ex);
        }
    }

    protected HttpResponse SendJsonPost(String uri, Object dto) throws RepositoryException {

        StringEntity params = GetJsonStringEntity(dto);

        try {
            HttpPost request = new HttpPost(uri);
            request.addHeader("content-type", "text/plain; charset=utf-8");
            request.setEntity(params);
            return httpClient.execute(request);
        } catch (IOException ex) {
            throw new RepositoryException(ex);
        }
    }

    protected HttpResponse SendJsonPostWithAuthorisation(String uri, Object dto, String key) throws RepositoryException {

        StringEntity params = GetJsonStringEntity(dto);

        try {
            HttpPost request = new HttpPost(uri);
            request.addHeader("content-type", "text/plain; charset=utf-8");
            request.setEntity(params);
            request.addHeader("x-functions-key", key);
            return httpClient.execute(request);
        } catch (IOException ex) {
            throw new RepositoryException(ex);
        }
    }

    protected HttpResponse SendGet(String uri) throws RepositoryException {

        try {
            HttpGet request = new HttpGet(uri);
            return httpClient.execute(request);
        } catch (IOException ex) {
            throw new RepositoryException(ex);
        }
    }

    protected HttpResponse SendGetWithAuthorisation(String uri, String key) throws RepositoryException {
        try {
            HttpGet request = new HttpGet(uri);
            request.addHeader("x-functions-key", key);
            return httpClient.execute(request);
        } catch (IOException ex) {
            throw new RepositoryException(ex);
        }
    }
}

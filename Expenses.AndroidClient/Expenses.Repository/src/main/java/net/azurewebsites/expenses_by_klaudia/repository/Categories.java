package net.azurewebsites.expenses_by_klaudia.repository;

import net.azurewebsites.expenses_by_klaudia.model.AddCategoryDto;

import java.net.HttpURLConnection;
import java.util.Dictionary;
import java.util.HashMap;

public class Categories extends RepositoryBase {
    public Categories(String host, String path) {
        super(host, path, "categories");
    }

    public HttpURLConnection GetAll(String login, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("getall")
                .appendPath(login)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendGet(uri);
    }

    public HttpURLConnection Add(String login, String key, String categoryName, HashMap<String, Double> factors) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("add")
                .appendPath(login)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        AddCategoryDto dto = new AddCategoryDto();
        dto.Name = categoryName;
        dto.Factor = factors;
        return SendJsonPost(uri, dto);
    }
}

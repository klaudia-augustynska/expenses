package net.azurewebsites.expenses_by_klaudia.repository;

import net.azurewebsites.expenses_by_klaudia.model.AddCashFlowDto;

import java.net.HttpURLConnection;
import java.text.SimpleDateFormat;
import java.util.Date;

public class CashFlows extends RepositoryBase {
    public CashFlows(String host, String path) {
        super(host, path, "cashflows");
    }

    public HttpURLConnection Add(String login, String key, AddCashFlowDto dto) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("add")
                .appendPath(login)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendJsonPost(uri, dto);
    }

    public HttpURLConnection GetSummary(String householdId, String login, Date dateFrom, Date dateTo, String key) throws RepositoryException {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy.MM.dd");
        String dateFromString = sdf.format(dateFrom);
        String dateToString = sdf.format(dateTo);
        String uri = GetBaseUriBuilder()
                .appendPath("summary")
                .appendPath(householdId)
                .appendPath(login)
                .appendPath(dateFromString)
                .appendPath(dateToString)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendGet(uri);
    }

    public HttpURLConnection GetDataForAdd(String householdId, String login, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("getdata")
                .appendPath(householdId)
                .appendPath(login)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendGet(uri);
    }
}

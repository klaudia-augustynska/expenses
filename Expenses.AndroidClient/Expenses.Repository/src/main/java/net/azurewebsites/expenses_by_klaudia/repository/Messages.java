package net.azurewebsites.expenses_by_klaudia.repository;

import net.azurewebsites.expenses_by_klaudia.model.GetNewMessagesDto;

import java.net.HttpURLConnection;
import java.text.SimpleDateFormat;
import java.util.Date;

public class Messages extends RepositoryBase {
    public Messages(String host, String path) {
        super(host, path, "messages");
    }

    public HttpURLConnection GetNew(String login, Date dateFrom, String key) throws RepositoryException {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy.MM.dd_HH:mm:ss");
        String dateFromString = sdf.format(dateFrom);
        String uri = GetBaseUriBuilder()
                .appendPath("getnew")
                .appendPath(login)
                .appendPath(dateFromString)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        return SendGet(uri);
    }
}

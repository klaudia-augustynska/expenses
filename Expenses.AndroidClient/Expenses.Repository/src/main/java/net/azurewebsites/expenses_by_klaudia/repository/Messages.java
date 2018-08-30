package net.azurewebsites.expenses_by_klaudia.repository;

import net.azurewebsites.expenses_by_klaudia.model.GetNewMessagesDto;

import java.net.HttpURLConnection;
import java.util.Date;

public class Messages extends RepositoryBase {
    public Messages(String host, String path) {
        super(host, path, "messages");
    }

    public HttpURLConnection GetNew(String login, Date dateFrom, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("getnew")
                .appendPath(login)
                .appendQueryParameter("code", key)
                .build()
                .toString();
        GetNewMessagesDto dto = new GetNewMessagesDto();
        dto.DateTime = dateFrom;
        return SendJsonPost(uri, dto);
    }
}

package net.azurewebsites.expenses_by_klaudia.repository;

import net.azurewebsites.expenses_by_klaudia.model.AddUserDto;
import net.azurewebsites.expenses_by_klaudia.model.ConfigureUserDto;
import net.azurewebsites.expenses_by_klaudia.model.LogInDto;

import java.net.HttpURLConnection;

public class Users extends RepositoryBase {

    public Users(String host, String path) {
        super(host, path, "users");
    }

    public HttpURLConnection Add(String login, String hashedPassword, String salt) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("add")
                .appendPath(login)
                .build()
                .toString();
        AddUserDto dto = new AddUserDto();
        dto.HashedPassword = hashedPassword;
        dto.Salt = salt;

        return SendJsonPost(uri, dto);
    }

    public HttpURLConnection LogIn(String login, String hashedPassword) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("login")
                .appendPath(login)
                .build()
                .toString();
        LogInDto dto = new LogInDto();
        dto.HashedPassword = hashedPassword;

        return SendJsonPost(uri, dto);
    }

    public HttpURLConnection GetSalt(String login) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("salt")
                .appendPath(login)
                .build()
                .toString();
        return SendGet(uri);
    }

    public HttpURLConnection Delete(String login, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("delete")
                .appendPath(login)
                .build()
                .toString();
        return SendGetWithAuthorisation(uri, key);
    }

    public HttpURLConnection ConfigureUser(String login, String key, ConfigureUserDto configureUserDto) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("configure")
                .appendPath(login)
                .build()
                .toString();
        return SendJsonPostWithAuthorisation(uri, configureUserDto, key);
    }

    public HttpURLConnection GetWallets(String login, String householdId, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder()
                .appendPath("wallets")
                .appendPath(householdId)
                .appendPath(login)
                .build()
                .toString();
        return SendGetWithAuthorisation(uri, key);
    }
}

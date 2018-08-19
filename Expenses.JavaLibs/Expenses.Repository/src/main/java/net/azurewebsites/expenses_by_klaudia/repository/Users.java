package net.azurewebsites.expenses_by_klaudia.repository;

import net.azurewebsites.expenses_by_klaudia.model.AddUserDto;
import net.azurewebsites.expenses_by_klaudia.model.ConfigureUserDto;
import net.azurewebsites.expenses_by_klaudia.model.LogInDto;
import org.apache.http.HttpResponse;

public class Users extends RepositoryBase {

    public Users(String host, String path) {
        super(host, path, "users");
    }

    public HttpResponse Add(String login, String hashedPassword, String salt) throws RepositoryException {

        String uri = GetBaseUriBuilder().pathSegment("add", login).toUriString();

        AddUserDto dto = new AddUserDto();
        dto.HashedPassword = hashedPassword;
        dto.Salt = salt;

        return SendJsonPost(uri, dto);
    }

    public HttpResponse LogIn(String login, String hashedPassword) throws RepositoryException {
        String uri = GetBaseUriBuilder().pathSegment("login", login).toUriString();

        LogInDto dto = new LogInDto();
        dto.HashedPassword = hashedPassword;

        return SendJsonPost(uri, dto);
    }

    public HttpResponse GetSalt(String login) throws RepositoryException {
        String uri = GetBaseUriBuilder().pathSegment("salt", login).toUriString();
        return SendGet(uri);
    }

    public HttpResponse Delete(String login, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder().pathSegment("delete", login).toUriString();
        return SendGetWithAuthorisation(uri, key);
    }

    public HttpResponse ConfigureUser(String login, String key, ConfigureUserDto configureUserDto) throws RepositoryException {
        String uri = GetBaseUriBuilder().pathSegment("configure", login).toUriString();
        return SendJsonPostWithAuthorisation(uri, configureUserDto, key);
    }

    public HttpResponse GetWallets(String login, String householdId, String key) throws RepositoryException {
        String uri = GetBaseUriBuilder().pathSegment("wallets", householdId, login).toUriString();
        return SendGetWithAuthorisation(uri, key);
    }
}

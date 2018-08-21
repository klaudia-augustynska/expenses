package net.azurewebsites.expenses_by_klaudia.repository;

public class Repository {
    private final String Path = "api";
    public Repository(String host) {
        _usersRepository = new Users(host, Path);
    }

    private Users _usersRepository;
    public Users GetUsersRepository() {
        return _usersRepository;
    }
}

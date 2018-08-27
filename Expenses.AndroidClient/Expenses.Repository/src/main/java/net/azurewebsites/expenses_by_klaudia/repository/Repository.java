package net.azurewebsites.expenses_by_klaudia.repository;

public class Repository {
    private final String Path = "api";
    public Repository(String host) {
        _usersRepository = new Users(host, Path);
        _categoriesRepository = new Categories(host, Path);
        _cashFlowsRepository = new CashFlows(host, Path);
    }

    private Users _usersRepository;
    public Users GetUsersRepository() {
        return _usersRepository;
    }

    private Categories _categoriesRepository;
    public Categories GetCategoriesRepository() {
        return _categoriesRepository;
    }

    private CashFlows _cashFlowsRepository;
    public CashFlows getCashFlowsRepository() {
        return _cashFlowsRepository;
    }
}

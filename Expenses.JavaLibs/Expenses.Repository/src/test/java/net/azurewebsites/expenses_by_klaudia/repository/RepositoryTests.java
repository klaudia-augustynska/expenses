package net.azurewebsites.expenses_by_klaudia.repository;


import org.junit.Assert;
import org.junit.Test;

public class RepositoryTests {
    @Test
    public void RepositorySendsRequest() {
        Repository r = new Repository("http://localhost:7071/");
        Boolean doesNotThrow;

        try {
            //r.GetUsersRepository().Add("klaudia", "aaaaa", "aaaa");
            r.GetUsersRepository().
            doesNotThrow = true;
        }
        catch (Exception ex) {
            doesNotThrow = false;
        }

        Assert.assertTrue(doesNotThrow);
    }
}

package net.azurewebsites.expenses_by_klaudia.expensesapp;


import org.junit.Test;

import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;

import static org.junit.Assert.*;

/**
 * Example local unit test, which will execute on the development machine (host).
 *
 * @see <a href="http://d.android.com/tools/testing">Testing documentation</a>
 */
public class ExampleUnitTest {
    @Test
    public void dateObject_isJavaThatStupidLanguage() {

        int[] date = new int[] {1992, 12, 10};
        Calendar cal = new GregorianCalendar(2012, 9, 5);
        Date aaa = cal.getTime();
    }
}
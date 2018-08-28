package net.azurewebsites.expenses_by_klaudia.expensesapp.helpers;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.Locale;

public class DateHelper {

    public static boolean shouldRefresh(String lastTimeStr) {
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("MM-dd-yyyy HH:mm:ss", Locale.getDefault());
            Calendar startDate = new GregorianCalendar();
            startDate.setTimeInMillis(sdf.parse(lastTimeStr).getTime());
            Calendar endDate = Calendar.getInstance();
            long totalMillis = endDate.getTimeInMillis() - startDate.getTimeInMillis();
            int hours = (int) (totalMillis / 1000) / 3600;
            return hours > 0;
        } catch (ParseException ex) {
            return true;
        }
    }

    public static String getCurrentDate() {
        SimpleDateFormat sdf = new SimpleDateFormat("MM-dd-yyyy HH:mm:ss", Locale.getDefault());
        Date d = new Date();
        return sdf.format(d);
    }

    public static Date getDateThatBeginsTheMonth() {
        Calendar currentTime = Calendar.getInstance();
        currentTime.set(Calendar.DAY_OF_MONTH, 1);
        return currentTime.getTime();
    }

    public static Date getDateThatEndsTheMonth() {
        Calendar currentTime = Calendar.getInstance();
        int lastDay = currentTime.getActualMaximum(Calendar.DAY_OF_MONTH);
        currentTime.set(Calendar.DAY_OF_MONTH, lastDay);
        return currentTime.getTime();
    }
}

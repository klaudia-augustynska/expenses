package net.azurewebsites.expenses_by_klaudia.expensesapp.validation;

import android.text.TextUtils;

import net.azurewebsites.expenses_by_klaudia.expensesapp.R;

import java.util.Calendar;
import java.util.Date;
import java.util.GregorianCalendar;
import java.util.function.Function;

public class DateRule extends ValidationRule {
    public DateRule(Function<Integer, String> getStringDelegate) {
        super(getStringDelegate);
    }

    @Override
    public boolean validate(String value) {
        if (TextUtils.isEmpty(value))
            return true;

        String[] split = value.split("-");
        if (split.length != 3)
            return false;

        int[] integers = new int[split.length];
        try {
            for (int i = 0; i < split.length; ++i)
                integers[i] = Integer.parseInt(split[i]);
        } catch (Exception ex) {
            return false;
        }

        try {
            Calendar cal = Calendar.getInstance();
            cal.set(Calendar.YEAR, integers[0]);
            cal.set(Calendar.MONTH, integers[1] - 1);
            cal.set(Calendar.DAY_OF_MONTH, 1);

            Calendar date = new GregorianCalendar(integers[0], integers[1]-1, integers[2]);
            if (date.get(Calendar.YEAR) != integers[0]
                    || date.get(Calendar.MONTH) != integers[1] - 1
                    || date.get(Calendar.DAY_OF_MONTH) != integers[2])
                return false;
            return true;
        } catch (Exception ex)
        {
            return false;
        }
    }

    @Override
    public String getErrorMsg() {
        return mGetStringDelegate.apply(R.string.error_incorrect_date);
    }
}

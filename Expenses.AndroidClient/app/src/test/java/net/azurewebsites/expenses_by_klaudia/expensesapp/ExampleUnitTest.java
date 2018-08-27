package net.azurewebsites.expenses_by_klaudia.expensesapp;


import com.google.gson.Gson;

import net.azurewebsites.expenses_by_klaudia.model.DEFAULT_CATEGORY_CODE;
import net.azurewebsites.expenses_by_klaudia.model.GetDataForAddCashFlowResponseDto;

import org.junit.Assert;
import org.junit.Test;

import java.util.Calendar;
import java.util.Date;
import java.util.Dictionary;
import java.util.Enumeration;
import java.util.GregorianCalendar;
import java.util.HashMap;
import java.util.UUID;

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

    @Test
    public void getDataForAddCashFlowResposneDto_jsonCompatible() {
        String json = "{\"Categories\":[{\"Guid\":\"f40f27d3-8630-4804-8ed9-8f4353389ee3\",\"DefaultCategory\":1,\"Name\":\"Jedzenie\",\"Factor\":{\"qwerty\":100.0}},{\"Guid\":\"232146cf-be29-4418-9065-f75ea928f6be\",\"DefaultCategory\":2,\"Name\":\"Niezdrowa żywność\",\"Factor\":{\"qwerty\":100.0}},{\"Guid\":\"addea61e-0c4f-48c8-baac-8785e655226f\",\"DefaultCategory\":3,\"Name\":\"Alkohol\",\"Factor\":{\"qwerty\":100.0}},{\"Guid\":\"0a04c0e6-db08-4526-bc06-1ab18443d19f\",\"DefaultCategory\":5,\"Name\":\"Higiena\",\"Factor\":{\"qwerty\":100.0}},{\"Guid\":\"a153a88e-e5d4-4e0b-8bb2-cb88f357751f\",\"DefaultCategory\":6,\"Name\":\"Opłaty\",\"Factor\":{\"qwerty\":100.0}},{\"Guid\":\"c57b3340-f55c-4923-ae9d-49e067e8b9e3\",\"DefaultCategory\":4,\"Name\":\"Transport\",\"Factor\":{\"qwerty\":100.0}},{\"Guid\":\"eca180ef-692d-490e-8b02-ed5f795d6c52\",\"DefaultCategory\":7,\"Name\":\"Różne\",\"Factor\":{\"qwerty\":100.0}}],\"Wallets\":[{\"Guid\":\"75ea0409-ee35-47b8-982e-4b0e64becd6c\",\"Name\":\"cash pln\",\"Money\":{\"Amount\":9999.0,\"Currency\":1,\"ExchangeRate\":null}}]}\n";
        Gson gson = new Gson();
        GetDataForAddCashFlowResponseDto dto = gson.fromJson(json, GetDataForAddCashFlowResponseDto.class);
        Assert.assertNotNull(dto);
        Assert.assertEquals(1, dto.Wallets.get(0).Money.Currency.getNumVal());
    }

    @Test
    public void jsonTest() {
        String categoryJson = "{\"Guid\":\"f40f27d3-8630-4804-8ed9-8f4353389ee3\",\"DefaultCategory\":1,\"Factor\":{\"qwerty\":100.0}}";
        Gson gson = new Gson();
        Cat cat = gson.fromJson(categoryJson, Cat.class);
        Assert.assertEquals(1, cat.DefaultCategory.getNumVal());
        for (String key : cat.Factor.keySet())
            Assert.assertEquals("qwerty", key);
    }

    public class Cat {
        public UUID Guid;
        public DEFAULT_CATEGORY_CODE DefaultCategory;
        public HashMap<String, Double> Factor;
    }
}
package net.azurewebsites.expenses_by_klaudia.expensesapp.helpers;

import com.google.gson.Gson;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.util.function.Supplier;

public class HttpResponder {
    public static Integer ask(Supplier<HttpURLConnection> connectionSupplier) {

        HttpURLConnection connection = null;
        Integer response;
        try {
            connection = connectionSupplier.get();
            if (connection == null)
                return null;
            response = connection.getResponseCode();
        } catch (Exception ex) {
            response = null;
        }
        finally {
            if (connection != null)
                connection.disconnect();
        }
        return response;
    }

    @SuppressWarnings("unchecked")
    public static HttpResponse<String> askForString(Supplier<HttpURLConnection> connectionSupplier) {
        HttpURLConnection connection = null;
        Integer response;
        String message = null;
        try {
            connection = connectionSupplier.get();
            if (connection == null)
                return new HttpResponse(null, null);
            response = connection.getResponseCode();
            if (response == HttpURLConnection.HTTP_OK)
            {
                message = getString(connection);
            }
        } catch (Exception ex) {
            response = null;
        }
        finally {
            if (connection != null)
                connection.disconnect();
        }
        return new HttpResponse(response, message);
    }

    @SuppressWarnings("unchecked")
    public static <T> HttpResponse askForDtoFromJson(Supplier<HttpURLConnection> connectionSupplier, Class<T> clazz) {
        HttpURLConnection connection = null;
        Integer response;
        T dto = null;
        try {
            connection = connectionSupplier.get();
            if (connection == null)
                return new HttpResponse(null, null);
            response = connection.getResponseCode();
            if (response == HttpURLConnection.HTTP_OK)
            {
                String json = getString(connection);
                Gson gson = new Gson();
                dto = gson.fromJson(json, clazz);
            }
        } catch (Exception ex) {
            response = null;
        }
        finally {
            if (connection != null)
                connection.disconnect();
        }
        return new HttpResponse(response, dto);
    }

    private static String getString(HttpURLConnection connection) throws IOException {
        BufferedReader br = new BufferedReader(new InputStreamReader(connection.getInputStream()));
        StringBuilder sb = new StringBuilder();
        String line;
        while ((line = br.readLine()) != null) {
            sb.append(line).append("\n");
        }
        br.close();
        return sb.toString();
    }
}

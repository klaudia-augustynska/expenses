package net.azurewebsites.expenses_by_klaudia.expensesapp.helpers;

import com.google.gson.Gson;
import com.google.gson.reflect.TypeToken;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.lang.reflect.Type;
import java.net.HttpURLConnection;
import java.util.List;
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
                message = message.trim();
                if (message.length() > 1 && message.charAt(0) == '"' && message.charAt(message.length()-1) == '"')
                    message = message.substring(1, message.length()-1);
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


    @SuppressWarnings("unchecked")
    public static <T> HttpResponse<List<T>> askForDtoFromJsonArray(Supplier<HttpURLConnection> connectionSupplier, Type listType, List<T> listObjectToProvideType) {
        HttpURLConnection connection = null;
        Integer response;
        List<T> dto = listObjectToProvideType;
        try {
            connection = connectionSupplier.get();
            if (connection == null)
                return new HttpResponse(null, null);
            response = connection.getResponseCode();
            if (response == HttpURLConnection.HTTP_OK)
            {
                String json = getString(connection);
                json = json.trim();
                if (json.length() >= 2)
                    if (json.charAt(0) == '"' && json.charAt(json.length()-1) == '"')
                        json = json.substring(1,json.length()-1);
                json = json.replace("\\", "\"");
                json = json.replace("\"\"", "\"");
                Gson gson = new Gson();
                gson.fromJson(json, listType);
                dto = gson.fromJson(json, listType);
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

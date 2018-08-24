package net.azurewebsites.expenses_by_klaudia.utils;

import com.google.gson.Gson;

import java.io.BufferedOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.net.HttpURLConnection;
import java.net.URL;

public class HttpURLConnectionBuilder {

    private HttpURLConnection mConnection;
    private boolean mIsJson;
    private boolean mWasBuilt;
    private boolean mIsPost;
    private String mMessage;

    public HttpURLConnectionBuilder(String uri, boolean isPost) throws BuildHttpURLConnectionException {
        mConnection = null;
        mIsJson = false;
        mIsPost = isPost;
        mMessage = null;
        startBuilding(uri, isPost);
    }

    public void setJsonInput(Object dto) throws BuildHttpURLConnectionException {
        if (!mIsPost) {
            throw new BuildHttpURLConnectionException("chcesz wysłać jsona getem?", null);
        }
        mMessage = GetJsonStringEntity(dto);
        mConnection.setFixedLengthStreamingMode(mMessage.getBytes().length);
        mConnection.setRequestProperty("Content-Type", "application/json;charset=utf-8");
        mIsJson = true;
    }

    public HttpURLConnection build() throws BuildHttpURLConnectionException {
        if (mWasBuilt) {
            return mConnection;
        }

        OutputStream os = null;
        try {
            mConnection.connect();

            if (mIsJson) {
                os = new BufferedOutputStream(mConnection.getOutputStream());
                os.write(mMessage.getBytes());
                os.flush();
            }
            mWasBuilt = true;
        } catch (Exception ex) {
            throw new BuildHttpURLConnectionException(ex);
        }
        finally {
            if (os != null)
            {
                try {
                    os.close();
                } catch (IOException ex) {
                    throw new BuildHttpURLConnectionException(ex);
                }
            }
        }
        return mConnection;
    }

    private void startBuilding(String uri, boolean isPost) throws BuildHttpURLConnectionException {

        try {
            URL url = new URL(uri);
            mConnection = (HttpURLConnection) url.openConnection();
            mConnection.setReadTimeout( 10000 /*milliseconds*/ );
            mConnection.setConnectTimeout( 15000 /* milliseconds */ );
            mConnection.setRequestMethod(isPost ? "POST" : "GET");
            mConnection.setDoInput(true);
            mConnection.setDoOutput(true);
        } catch (Exception ex) {
            throw new BuildHttpURLConnectionException(ex);
        }
    }

    private String GetJsonStringEntity(Object dto) {
        Gson gson = new Gson();
        return gson.toJson(dto);
    }
}

package net.azurewebsites.expenses_by_klaudia.expensesapp.helpers;

import android.app.Service;
import android.content.Intent;
import android.os.IBinder;

public class AppAuthenticatorService extends Service {
    private AppAuthenticator mAuthenticator;

    @Override
    public void onCreate(){
        mAuthenticator = new AppAuthenticator(this);
    }

    @Override
    public IBinder onBind(Intent intent) {
        return mAuthenticator.getIBinder();
    }
}

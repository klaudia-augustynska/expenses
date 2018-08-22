package net.azurewebsites.expenses_by_klaudia.expensesapp;

import android.content.Intent;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.ProgressBar;

public class LogInActivity extends AppCompatActivity {
    
    private EditText mLogin;
    private EditText mPassword;
    private View mProgress;
    private View mForm;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_log_in);

        Intent intent = getIntent();
        String login = intent.getStringExtra(SignUpActivity.EXTRA_LOGIN);
        
        mLogin = findViewById(R.id.log_in_login);
        mPassword = findViewById(R.id.log_in_password);
        mProgress = findViewById(R.id.log_in_progress);
        mForm = findViewById(R.id.log_in_form);

        if (login != null) {

        }
    }


}

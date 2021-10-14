package taskboard.androidclient.ui;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.os.Bundle;
import android.widget.Button;
import android.widget.EditText;

import taskboard.androidclient.R;

public class ActivityConnect extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_connect);

        Button buttonConnect = findViewById(R.id.buttonConnect);
        buttonConnect.requestFocus();
        EditText editTextApiUrl = findViewById(R.id.editTextApiUrl);

        buttonConnect.setOnClickListener(v -> {
            Intent resultData = new Intent();
            resultData.putExtra("paramApiBaseUrl", editTextApiUrl.getText().toString());
            setResult(RESULT_OK, resultData);
            finish();
        });
    }
}
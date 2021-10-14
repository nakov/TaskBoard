package taskboard.androidclient.ui;

import static taskboard.androidclient.data.Constants.*;

import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import android.content.DialogInterface;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;

import taskboard.androidclient.R;

public class AddTaskActivity extends AppCompatActivity implements AdapterView.OnItemSelectedListener{
    private String board = null;
    EditText editTextTitle, editTextDescription;

    @RequiresApi(api = Build.VERSION_CODES.N)
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_add_task);

        editTextTitle = findViewById(R.id.editTextTitle);
        editTextTitle.requestFocus();
        editTextDescription = findViewById(R.id.editTextDescription);
        Spinner spinnerBoard = (Spinner) findViewById(R.id.spinnerBoard);

        Bundle bundle = getIntent().getExtras();
        String[] boards = bundle.getStringArray("boards");

        ArrayAdapter<String> adapter = new ArrayAdapter<String>(this, android.R.layout.simple_spinner_item, boards);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinnerBoard.setAdapter(adapter);
        spinnerBoard.setOnItemSelectedListener(this);

        Button buttonCancel = findViewById(R.id.buttonCancel);
        buttonCancel.setOnClickListener(v -> {
            setResult(RESULT_CANCELED);
            finish();
        });

        Button buttonCreate = findViewById(R.id.buttonCreate);
        buttonCreate.setOnClickListener(v -> {
            String errors = CheckAllFields();
            if(errors == "") {
                Intent resultData = new Intent();
                resultData.putExtra("title", editTextTitle.getText().toString());
                resultData.putExtra("description", editTextDescription.getText().toString());
                resultData.putExtra("board", board);
                setResult(RESULT_OK, resultData);
                finish();
            }
            else {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.setMessage(errors)
                        .setCancelable(false)
                        .setPositiveButton("Ok", new DialogInterface.OnClickListener() {
                            public void onClick(DialogInterface dialog, int id) {
                            }
                        });
                //Creating dialog box
                AlertDialog alert = builder.create();
                //Setting the title manually
                alert.setTitle("Errors");
                alert.show();
            }
        });
    }

    @Override
    public void onItemSelected(AdapterView<?> parent, View v, int position, long id) {
        board = parent.getItemAtPosition(position).toString();
    }

    @Override
    public void onNothingSelected(AdapterView<?> parent) {
        board = parent.getItemAtPosition(0).toString();
    }

    private String CheckAllFields() {
        StringBuilder errors = new StringBuilder();

        // Title checks
        String title = editTextTitle.getText().toString();
        if (title.length() == 0) {
            errors.append("Title field is required.");
        }

        if (title.length() > 0 && title.length() < MinTitleLength) {
            errors.append(System.lineSeparator());
            errors.append("Title must be at least " + MinTitleLength + " characters long.");
        }

        // Description checks
        String description = editTextDescription.getText().toString();
        if (description.length() == 0) {
            errors.append(System.lineSeparator());
            errors.append("Description field is required.");
        }

        if (description.length() > 0 && description.length() < MinDescriptionLength) {
            errors.append(System.lineSeparator());
            errors.append("Description must be at least " + MinDescriptionLength + " characters long.");
        }

        return  errors.toString();
    }
}
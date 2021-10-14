package taskboard.androidclient.ui;

import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.Bundle;
import android.os.Parcelable;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import java.net.HttpURLConnection;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.concurrent.TimeUnit;
import java.util.stream.Collectors;

import okhttp3.OkHttpClient;
import taskboard.androidclient.R;
import taskboard.androidclient.data.Board;
import taskboard.androidclient.data.LoginResponse;
import taskboard.androidclient.data.Task;
import taskboard.androidclient.data.TaskBoardAPI;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import taskboard.androidclient.data.TaskResponse;
import taskboard.androidclient.data.UserLoginModel;
import taskboard.androidclient.data.UserRegisterModel;

public class ActivityTasks extends AppCompatActivity {
    private static final int REQUEST_CODE_CONNECT = 1001;
    private static final int REQUEST_CODE_LOGIN = 1002;
    private static final int REQUEST_CODE_REGISTER = 1003;
    private static final int REQUEST_CODE_CREATE_TASK = 1004;
    private OkHttpClient client;
    private TextView textViewStatus;
    private Button buttonConnect;
    private Button buttonLogin;
    private Button buttonRegister;
    private Button buttonSearch;
    private Button buttonAdd;
    private Button buttonReload;
    private EditText editTextKeyword;
    private String apiBaseUrl;
    private String token = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tasks);

        client = new OkHttpClient.Builder()
                .connectTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(10, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .build();

        this.textViewStatus = findViewById(R.id.textViewStatus);

        this.buttonConnect = findViewById(R.id.buttonConnect);
        this.buttonConnect.setOnClickListener(v -> {
            this.editTextKeyword.setText("");
            Intent intent = new Intent(this, ActivityConnect.class);
            startActivityForResult(intent, REQUEST_CODE_CONNECT);
        });

        this.buttonLogin = findViewById(R.id.buttonLogin);
        this.buttonLogin.setEnabled(false);
        this.buttonLogin.setOnClickListener(v -> {
            this.editTextKeyword.setText("");
            Intent intent = new Intent(this, LoginActivity.class);
            startActivityForResult(intent, REQUEST_CODE_LOGIN);
        });

        this.buttonRegister = findViewById(R.id.buttonRegister);
        this.buttonRegister.setEnabled(false);
        this.buttonRegister.setOnClickListener(v -> {
            this.editTextKeyword.setText("");
            Intent intent = new Intent(this, RegisterActivity.class);
            startActivityForResult(intent, REQUEST_CODE_REGISTER);
        });

        this.editTextKeyword = findViewById(R.id.editTextKeyword);
        this.editTextKeyword.setEnabled(false);

        this.buttonSearch = findViewById(R.id.buttonSearch);
        this.buttonSearch.setEnabled(false);
        this.buttonSearch.setOnClickListener(v -> {
            // Hide soft keyboard
            View view = this.getCurrentFocus();
            if (view != null) {
                InputMethodManager imm = (InputMethodManager)getSystemService(Context.INPUT_METHOD_SERVICE);
                imm.hideSoftInputFromWindow(view.getWindowToken(), 0);
            }

            String keyword = editTextKeyword.getText().toString();
            searchTasksByKeyword(keyword);
        });

        this.buttonAdd = findViewById(R.id.buttonAdd);
        this.buttonAdd.setEnabled(false);
        this.buttonAdd.setOnClickListener(v -> {
            this.editTextKeyword.setText("");

            try {
                Retrofit retrofit = new Retrofit.Builder()
                        .baseUrl(this.apiBaseUrl)
                        .addConverterFactory(GsonConverterFactory.create())
                        .client(client)
                        .build();
                TaskBoardAPI service = retrofit.create(TaskBoardAPI.class);

                Call<List<Board>> request;

                request = service.getBoards("Bearer " + token);
                try {
                    request.enqueue(new Callback<List<Board>>() {
                        @RequiresApi(api = Build.VERSION_CODES.N)
                        @Override
                        public void onResponse(Call<List<Board>> call, Response<List<Board>> response) {
                            if (response.code() != HttpURLConnection.HTTP_OK) {
                                return;
                            }

                            String[] boardNames= response.body().stream().map(Board::getName)
                                    .toArray(size -> new String[size]);
                            createIntent(boardNames);
                        }

                        @Override
                        public void onFailure(Call<List<Board>> call, Throwable t) {
                            showErrorMsg(t.getMessage());
                        }
                    });
                } catch (Throwable t) {
                    showErrorMsg(t.getMessage());
                }
            } catch (Throwable t) {
                showErrorMsg(t.getMessage());
            }
        });

        this.buttonReload = findViewById(R.id.buttonReload);
        this.buttonReload.setEnabled(false);
        this.buttonReload.setOnClickListener(v -> {
            // Hide soft keyboard
            View view = this.getCurrentFocus();
            if (view != null) {
                InputMethodManager imm = (InputMethodManager)getSystemService(Context.INPUT_METHOD_SERVICE);
                imm.hideSoftInputFromWindow(view.getWindowToken(), 0);
            }
            
            this.editTextKeyword.setText("");
            searchTasksByKeyword("");
        });
    }

    private  void createIntent(String[] boards) {
        Intent intent = new Intent(this, AddTaskActivity.class);
        Bundle b = new Bundle();
        b.putStringArray("boards", boards);
        intent.putExtras(b);
        startActivityForResult(intent, REQUEST_CODE_CREATE_TASK);
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == REQUEST_CODE_CONNECT && resultCode == RESULT_OK) {
            this.apiBaseUrl = data.getStringExtra("paramApiBaseUrl");
            if (!this.apiBaseUrl.endsWith("/"))
                this.apiBaseUrl += "/";
            connect();
        }
        if (requestCode == REQUEST_CODE_LOGIN && resultCode == RESULT_OK) {
            String username = data.getStringExtra("username");
            String password = data.getStringExtra("password");
            authorize(username, password);
        }
        if (requestCode == REQUEST_CODE_REGISTER && resultCode == RESULT_OK) {
            String userName = data.getStringExtra("username");
            String email = data.getStringExtra("email");
            String password = data.getStringExtra("password");
            String confirmPassword = data.getStringExtra("confirmPassword");
            String firstName = data.getStringExtra("firstName");
            String lastName = data.getStringExtra("lastName");
            registerUser(userName, email, password, confirmPassword, firstName, lastName);
        }
        if (requestCode == REQUEST_CODE_CREATE_TASK && resultCode == RESULT_OK) {
            String title = data.getStringExtra("title");
            String description = data.getStringExtra("description");
            String board = data.getStringExtra("board");
            createNewTask(title, description, board);
        }
    }

    private void connect() {
        showStatusMsg("Connecting ...");

        try {
            Retrofit retrofit = new Retrofit.Builder()
                    .baseUrl(this.apiBaseUrl)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
            TaskBoardAPI service = retrofit.create(TaskBoardAPI.class);

            Call<List<Task>> request;

            request = service.getTasks("Bearer ");
            try {
                request.enqueue(new Callback<List<Task>>() {
                    @Override
                    public void onResponse(Call<List<Task>> call, Response<List<Task>> response) {
                        if (response.code() != HttpURLConnection.HTTP_UNAUTHORIZED) {
                            changeLoginUserButtonsAccessibility(false);
                            changeTaskButtonsAccessibility(false);
                            showErrorMsg("Could not connect. Try again.");
                            return;
                        }

                        changeLoginUserButtonsAccessibility(true);
                        changeTaskButtonsAccessibility(false);
                        showSuccessMsg("Connected successfully.");
                    }

                    @Override
                    public void onFailure(Call<List<Task>> call, Throwable t) {
                        changeLoginUserButtonsAccessibility(false);
                        changeTaskButtonsAccessibility(false);
                        showErrorMsg("Could not connect. Try again.");
                    }
                });
            }
            catch (Throwable t) {
                changeLoginUserButtonsAccessibility(false);
                showErrorMsg("Could not connect. Try again.");
            }
        } catch (Throwable t) {
            changeLoginUserButtonsAccessibility(false);
            showErrorMsg("Could not connect. Try again.");
        }
    }

    private void changeLoginUserButtonsAccessibility(Boolean enable) {
        if(!enable) {
            this.buttonLogin.setEnabled(false);
            this.buttonRegister.setEnabled(false);
        }
        else {
            this.buttonLogin.setEnabled(true);
            this.buttonRegister.setEnabled(true);
        }
    }

    private void authorize(String username, String password) {
        showStatusMsg("Authorization ...");

        Retrofit retrofit = new Retrofit.Builder()
                .baseUrl(this.apiBaseUrl)
                .addConverterFactory(GsonConverterFactory.create())
                .client(client)
                .build();
        TaskBoardAPI service = retrofit.create(TaskBoardAPI.class);

        try {
            Call<LoginResponse> loginRequest;

            UserLoginModel user = new UserLoginModel();
            user.setUsername(username);
            user.setPassword(password);
            loginRequest = service.login(user);
            loginRequest.enqueue(new Callback<LoginResponse>() {
                @Override
                public void onResponse(Call<LoginResponse> call, Response<LoginResponse> response) {
                    if (response.code() != HttpURLConnection.HTTP_OK) {
                        changeTaskButtonsAccessibility(false);
                        showErrorMsg("Could not authorize. Try again.");
                        return;
                    }

                    changeTaskButtonsAccessibility(true);
                    showSuccessMsg("Authorized successfully.");
                    token = response.body().getToken();
                    searchTasksByKeyword("");
                }

                @Override
                public void onFailure(Call<LoginResponse> call, Throwable t) {
                    changeTaskButtonsAccessibility(false);
                    showErrorMsg("Could not authorize. Try again.");
                }
            });
        } catch (Throwable t) {
            changeTaskButtonsAccessibility(false);
            showErrorMsg("Could not authorize. Try again.");
        }
    }

    private void changeTaskButtonsAccessibility(Boolean enable) {
        if(!enable) {
            this.editTextKeyword.setEnabled(false);
            this.buttonSearch.setEnabled(false);
            this.buttonAdd.setEnabled(false);
            this.buttonReload.setEnabled(false);
        }
        else {
            this.editTextKeyword.setEnabled(true);
            this.buttonSearch.setEnabled(true);
            this.buttonAdd.setEnabled(true);
            this.buttonReload.setEnabled(true);
        }
    }

    private void searchTasksByKeyword(String keyword) {
        showStatusMsg("Loading tasks ...");
        try {
            Retrofit retrofit = new Retrofit.Builder()
                    .baseUrl(this.apiBaseUrl)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
            TaskBoardAPI service = retrofit.create(TaskBoardAPI.class);

            Call<List<Task>> request;
            if (keyword.length() > 0)
                request = service.findTasksByKeyword(keyword, "Bearer " + token);
            else
                request = service.getTasks("Bearer " + token);

            request.enqueue(new Callback<List<Task>>() {
                @Override
                public void onResponse(Call<List<Task>> call, Response<List<Task>> response) {
                    if (response.code() != HttpURLConnection.HTTP_OK) {
                        showErrorMsg("Error. HTTP code: " + response.code());
                        return;
                    }
                    displayTasks(response.body());
                }

                @Override
                public void onFailure(Call<List<Task>> call, Throwable t) {
                    showErrorMsg(t.getMessage());
                }
            });
        } catch (Throwable t) {
            showErrorMsg(t.getMessage());
        }
    }

    private void displayTasks(List<Task> tasks) {
        showSuccessMsg("Tasks found: " + tasks.size());

        // Lookup the recyclerview in activity layout
        RecyclerView recyclerViewTasks =
                (RecyclerView) findViewById(R.id.recyclerViewTasks);

        TasksAdapter tasksAdapter = new TasksAdapter(tasks);
        // Attach the adapter to the RecyclerView to populate items
        recyclerViewTasks.setAdapter(tasksAdapter);
        // Set layout manager to position the items
        recyclerViewTasks.setLayoutManager(new LinearLayoutManager(this));
    }

    private void registerUser(String userName, String email, String password, String confirmPassword, String firstName, String lastName) {
        showStatusMsg("Registering the new user...");

        try {
            Retrofit retrofit = new Retrofit.Builder()
                    .baseUrl(this.apiBaseUrl)
                    .addConverterFactory(GsonConverterFactory.create())
                    .client(client)
                    .build();
            TaskBoardAPI service = retrofit.create(TaskBoardAPI.class);

            UserRegisterModel user = new UserRegisterModel();
            user.setUsername(userName);
            user.setEmail(email);
            user.setPassword(password);
            user.setConfirmPassword(confirmPassword);
            user.setFirstName(firstName);
            user.setLastName(lastName);

            Call<TaskResponse> request = service.register(user);
            request.enqueue(new Callback<TaskResponse>() {
                @Override
                public void onResponse(Call<TaskResponse> call, Response<TaskResponse> response) {
                    if (response.code() != HttpURLConnection.HTTP_OK) {
                        showErrorMsg("Could not register a user. Try again.");
                        return;
                    }
                    authorize(userName, password);
                }

                @Override
                public void onFailure(Call<TaskResponse> call, Throwable t) {
                    showErrorMsg("Could not register a user. Try again.");
                }
            });
        } catch (Throwable t) {
            showErrorMsg("Could not register a user. Try again.");
        }
    }

    private void createNewTask(String title, String description, String board) {
        showStatusMsg("Creating new task ...");
        try {
            Retrofit retrofit = new Retrofit.Builder()
                    .baseUrl(this.apiBaseUrl)
                    .addConverterFactory(GsonConverterFactory.create())
                    .build();
            TaskBoardAPI service = retrofit.create(TaskBoardAPI.class);

            Task task = new Task();
            task.setTitle(title);
            task.setDescription(description);
            task.setBoard(board);
            Call<TaskResponse> request = service.create(task, "Bearer " + token);
            request.enqueue(new Callback<TaskResponse>() {
                @Override
                public void onResponse(Call<TaskResponse> call, Response<TaskResponse> response) {
                    if (response.code() != HttpURLConnection.HTTP_CREATED) {
                        showErrorMsg("Could not create the new task. Try again.");
                        return;
                    }
                    searchTasksByKeyword("");
                }

                @Override
                public void onFailure(Call<TaskResponse> call, Throwable t) {
                    showErrorMsg("Could not create the new task. Try again.");
                }
            });
        } catch (Throwable t) {
            showErrorMsg("Could not create the new task. Try again.");
        }
    }

    private void showStatusMsg(String msg) {
        textViewStatus.setText(msg);
        textViewStatus.setBackgroundResource(R.color.backgroundStatus);
    }

    private void showSuccessMsg(String msg) {
        textViewStatus.setText(msg);
        textViewStatus.setBackgroundResource(R.color.backgroundSuccess);
    }

    private void showErrorMsg(String errMsg) {
        textViewStatus.setText(errMsg);
        textViewStatus.setBackgroundResource(R.color.backgroundError);
    }
}

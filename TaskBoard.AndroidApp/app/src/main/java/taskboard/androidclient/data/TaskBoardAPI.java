package taskboard.androidclient.data;

import java.util.ArrayList;
import java.util.List;

import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.Header;
import retrofit2.http.Headers;
import retrofit2.http.POST;
import retrofit2.http.Path;

public interface TaskBoardAPI {
    @Headers({ "Content-Type: application/json;charset=UTF-8"})
    @GET("boards")
    Call<List<Board>> getBoards(@Header("Authorization") String auth);

    @Headers({ "Content-Type: application/json;charset=UTF-8"})
    @GET("tasks")
    Call<List<Task>> getTasks(@Header("Authorization") String auth);

    @Headers({ "Content-Type: application/json;charset=UTF-8"})
    @GET("tasks/search/{keyword}")
    Call<List<Task>> findTasksByKeyword(@Path("keyword") String keyword, @Header("Authorization") String auth);

    @Headers({ "Content-Type: application/json;charset=UTF-8"})
    @POST("tasks/create")
    Call<TaskResponse> create(@Body Task task, @Header("Authorization") String auth);

    @POST("users/login")
    Call<LoginResponse> login(@Body UserLoginModel user);

    @POST("users/register")
    Call<TaskResponse> register(@Body UserRegisterModel user);
}

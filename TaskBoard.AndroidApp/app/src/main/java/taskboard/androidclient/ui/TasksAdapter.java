package taskboard.androidclient.ui;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.google.type.DateTime;

import java.util.List;

import taskboard.androidclient.R;
import taskboard.androidclient.data.Task;

public class TasksAdapter extends
        RecyclerView.Adapter<TasksAdapter.ViewHolder> {

    private List<Task> tasks;

    public TasksAdapter(List<Task> tasks) {
        this.tasks = tasks;
    }

    @Override
    public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
        Context context = parent.getContext();
        LayoutInflater inflater = LayoutInflater.from(context);

        // Inflate the custom layout
        View taskView = inflater.inflate(R.layout.fragment_task_data, parent, false);

        // Return a new holder instance
        ViewHolder viewHolder = new ViewHolder(taskView);
        return viewHolder;
    }

    // Populates data into the item through holder
    @Override
    public void onBindViewHolder(@NonNull ViewHolder holder, int position) {
        // Get the data model based on position
        Task task = this.tasks.get(position);

        // Set item views based on your views and data model
        holder.textViewTaskId.setText("" + task.getId());
        holder.textViewTitle.setText(task.getTitle());
        holder.textViewDescription.setText(task.getDescription());
        holder.textViewCreatedOn.setText(task.getCreatedOn());
        holder.textViewBoard.setText(task.getBoard());
        holder.textViewOwner.setText(task.getOwner().getUsername());
    }

    @Override
    public int getItemCount() {
        return this.tasks.size();
    }

    // Provide a direct reference to each of the views within a data item
    // Used to cache the views within the item layout for fast access
    public class ViewHolder extends RecyclerView.ViewHolder {
        // Your holder should contain a member variable
        // for any view that will be set as you render a row
        public TextView textViewTaskId;
        public TextView textViewTitle;
        public TextView textViewDescription;
        public TextView textViewCreatedOn;
        public TextView textViewBoard;
        public TextView textViewOwner;

        // We also create a constructor that accepts the entire item row
        // and does the view lookups to find each subview
        public ViewHolder(View itemView) {
            // Stores the itemView in a public final member variable that can be used
            // to access the context from any ViewHolder instance.
            super(itemView);
            textViewTaskId = (TextView) itemView.findViewById(R.id.textViewTaskId);
            textViewTitle = (TextView) itemView.findViewById(R.id.textViewTitle);
            textViewDescription = (TextView) itemView.findViewById(R.id.textViewDescription);
            textViewCreatedOn = (TextView) itemView.findViewById(R.id.textViewCreatedOn);
            textViewBoard = (TextView) itemView.findViewById(R.id.textViewBoard);
            textViewOwner = (TextView) itemView.findViewById(R.id.textViewOwner);
        }
    }
}

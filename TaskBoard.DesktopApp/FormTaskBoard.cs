using System;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using TaskBoard.DesktopApp.Data;

using RestSharp;
using RestSharp.Serialization.Json;

namespace TaskBoard.DesktopApp
{
    public partial class FormTaskBoard : Form
    {
        private string apiBaseUrl;
        private string token;
        private RestClient restClient;

        public FormTaskBoard()
        {
            InitializeComponent();
            this.toolStripStatusLabel.TextChanged += ToolStripStatusLabel_TextChanged;
        }

        private void ToolStripStatusLabel_TextChanged(object sender, EventArgs e)
        {
            // Intentionally copy the "status label text" into the "accessibility text"
            // to allow the runtime UI inspectors to read the text
            this.toolStripStatusLabel.AccessibleName = toolStripStatusLabel.Text;
        }

        private void TaskBoardForm_Shown(object sender, EventArgs e)
        {
            // Show the [Connect] form again and agin, until connected
            var connected = false;
            while (connected == false)
            {
                var formConnect = new FormConnect();
                if (formConnect.ShowDialog() != DialogResult.OK)
                {
                    Close();
                    break;
                }
                connected = Connect(formConnect.ApiUrl);
            }
        }

        private bool Connect(string apiUrl)
        {
            try
            {
                // Try to connect to the Web API url
                this.apiBaseUrl = apiUrl;
                var homeRequest = new RestRequest("/", Method.GET);
                this.restClient = new RestClient(this.apiBaseUrl) { Timeout = 5000 };
                var homeResponse = this.restClient.Execute(homeRequest);
                if (!homeResponse.IsSuccessful)
                {
                    ShowError(homeResponse);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowErrorMsg(ex.Message);
                return false;
            }

            // Successfully connected to the Web API
            ShowSuccessMsg("Connected to the Web API.");

            buttonLogin.Enabled = true;
            buttonRegister.Enabled = true;

            return true;
        }

        private void buttonRegister_Click(object sender, EventArgs e)
        {
            var formRegister = new FormRegister();
            if (formRegister.ShowDialog() != DialogResult.OK)
                return;

            Register(
                formRegister.Username,
                formRegister.Email,
                formRegister.Password,
                formRegister.ConfirmPassword,
                formRegister.FirstName,
                formRegister.LastName);
        }

        private async void Register(string username, string email,
            string password, string confirmPassword, string firstName, string lastName)
        {
            var registerRequest = new RestRequest("/users/register", Method.POST);
            registerRequest.AddJsonBody(
                new
                {
                    Username = $"{username}",
                    Email = $"{email}",
                    Password = $"{password}",
                    ConfirmPassword = $"{confirmPassword}",
                    FirstName = firstName,
                    LastName = lastName
                });

            try
            {
                var registerResponse = await this.restClient.ExecuteAsync(registerRequest);
                if (!registerResponse.IsSuccessful)
                {
                    TaskButtonsChangeAccessibility(false);
                    ShowError(registerResponse);
                    return;
                }
            }
            catch (Exception ex)
            {
                TaskButtonsChangeAccessibility(false);
                ShowErrorMsg(ex.Message);
                return;
            }

            TaskButtonsChangeAccessibility(true);
            ShowSuccessMsg($"User `{username}` registered.");

            Login(username, password);
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            var formLogin = new FormLogin();
            if (formLogin.ShowDialog() != DialogResult.OK)
                return;

            Login(
                formLogin.Username,
                formLogin.Password);
        }

        private async void Login(string username, string password)
        {
            var loginRequest = new RestRequest("/users/login", Method.POST);
            loginRequest.AddJsonBody(new { Username = $"{username}", Password = $"{password}" });

            try
            {
                var loginResponse = await this.restClient.ExecuteAsync(loginRequest);

                if (!loginResponse.IsSuccessful)
                {
                    TaskButtonsChangeAccessibility(false);
                    ShowError(loginResponse);
                    return;
                }

                var jsonResponse = new JsonDeserializer().Deserialize<LoginResponse>(loginResponse);
                this.token = jsonResponse.Token;
            }
            catch (Exception ex)
            {
                TaskButtonsChangeAccessibility(false);
                ShowErrorMsg(ex.Message);
                return;
            }

            TaskButtonsChangeAccessibility(true);
            ShowSuccessMsg($"User `{username}` successfully logged-in.");

            LoadTasks();
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            LoadTasks(this.textBoxSearchText.Text);
        }

        private void buttonReload_Click(object sender, EventArgs e)
        {
            this.textBoxSearchText.Text = "";
            LoadTasks();
        }

        private async void LoadTasks(string searchKeyword = "")
        {
            var request = new RestRequest("/tasks", Method.GET);
            request.AddParameter("Authorization",
                "Bearer " + this.token, ParameterType.HttpHeader);

            ShowMsg("Loading tasks ...");

            try
            {
                var response = await this.restClient.ExecuteAsync(request);
                if (!response.IsSuccessful)
                {
                    ShowError(response);
                    return;
                }

                if (searchKeyword == "")
                    ShowMsg("Loading tasks ...");
                else
                {
                    ShowMsg($"Searching for tasks by keyword: {searchKeyword} ...");
                    request.Resource = "/tasks/search/{:keyword}";
                    request.AddUrlSegment(":keyword", searchKeyword);
                }

                response = await this.restClient.ExecuteAsync(request);
                // Visualize the returned tasks
                var tasks = new JsonDeserializer().Deserialize<List<Task>>(response);
                DisplayTasksInListView(tasks);

                if (tasks.Count > 0)
                    ShowSuccessMsg($"Search successful: {tasks.Count} tasks loaded.");
                else
                    ShowSuccessMsg($"No tasks match your search.");
            }
            catch (Exception ex)
            {
                ShowErrorMsg(ex.Message);
                return;
            }
        }

        private void DisplayTasksInListView(List<Task> tasks)
        {
            this.listViewTasks.Clear();

            // Create column headers
            var headers = new ColumnHeader[] {
                new ColumnHeader { Text = "Id", Width = 50 },
                new ColumnHeader { Text = "Tile", Width = 200 },
                new ColumnHeader { Text = "Description", Width = 350 },
                new ColumnHeader { Text = "Created On", Width = 200 },
                new ColumnHeader { Text = "Owner", Width = 100 }
            };
            this.listViewTasks.Columns.AddRange(headers);

            // Add items and groups to the ListView control
            var groups = new Dictionary<string, ListViewGroup>();
            foreach (var task in tasks)
            {
                var item = new ListViewItem(new string[] {
                    "" + task.Id, 
                    task.Title, 
                    task.Description, 
                    task.CreatedOn,  
                    task.Owner.UserName });
                if (!groups.ContainsKey(task.Board))
                {
                    var newGroup = new ListViewGroup("board") {
                        Header = task.Board,
                        Tag = task.Board
                    };
                    groups[task.Board] = newGroup;
                }
                item.Group = groups[task.Board];
                this.listViewTasks.Items.Add(item);
            }

            var sortedGroups = groups.Values.OrderByDescending(g => g.Tag).ToArray();
            this.listViewTasks.Groups.AddRange(sortedGroups);
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            var boardsRequest = new RestRequest("/boards", Method.GET);
            boardsRequest.AddParameter("Authorization",
               "Bearer " + this.token, ParameterType.HttpHeader);
            this.restClient = new RestClient(this.apiBaseUrl) { Timeout = 5000 };
            var boardsResponse = this.restClient.Execute(boardsRequest);
            if (!boardsResponse.IsSuccessful)
            {
                ShowError(boardsResponse);
            }
            var jsonResponse = new JsonDeserializer().Deserialize<List<Board>>(boardsResponse);
            var formCreateTask = new FormCreateTask(jsonResponse);
            if (formCreateTask.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var restClient = new RestClient(this.apiBaseUrl) { Timeout = 3000 };
                    var request = new RestRequest("/tasks/create", Method.POST);
                    request.AddParameter("Authorization",
                        "Bearer " + this.token, ParameterType.HttpHeader);
                    request.AddJsonBody(new {
                        title = formCreateTask.Title,
                        description = formCreateTask.Description,
                        board = formCreateTask.Board
                    });
                    ShowMsg($"Creating new task ...");
                    var response = await restClient.ExecuteAsync(request);
                    if (response.IsSuccessful & response.StatusCode == HttpStatusCode.Created)
                    {
                        ShowSuccessMsg($"Task created.");
                        LoadTasks();
                    }
                    else
                        ShowError(response);
                }
                catch (Exception ex)
                {
                    ShowErrorMsg(ex.Message);
                }
            }
        }

        private void ShowError(IRestResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                string errText = $"HTTP error `{response.StatusCode}`.";
                if (!string.IsNullOrWhiteSpace(response.Content))
                    errText += $" Details: {response.Content}";
                ShowErrorMsg(errText);
            }
            else
                ShowErrorMsg($"HTTP error `{response.ErrorMessage}`.");
        }

        private void ShowMsg(string msg)
        {
            toolStripStatusLabel.Text = msg;
            toolStripStatusLabel.ForeColor = SystemColors.ControlText;
            toolStripStatusLabel.BackColor = SystemColors.Control;
        }

        private void ShowSuccessMsg(string msg)
        {
            toolStripStatusLabel.Text = msg;
            toolStripStatusLabel.ForeColor = Color.White;
            toolStripStatusLabel.BackColor = Color.Green;
        }

        private void ShowErrorMsg(string errMsg)
        {
            toolStripStatusLabel.Text = $"Error: {errMsg}";
            toolStripStatusLabel.ForeColor = Color.White;
            toolStripStatusLabel.BackColor = Color.Red;
        }

        private void TaskButtonsChangeAccessibility(bool enable)
        {
            if (enable)
            {
                buttonSearch.Enabled = true;
                buttonAdd.Enabled = true;
                buttonReload.Enabled = true;
                textBoxSearchText.Enabled = true;
            }
            else
            {
                buttonSearch.Enabled = false;
                buttonAdd.Enabled = false;
                buttonReload.Enabled = false;
                textBoxSearchText.Enabled = false;
            }
        }
    }
}

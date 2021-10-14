using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using TaskBoard.DesktopApp.Data;

namespace TaskBoard.DesktopApp
{
    using static DataConstants;
    public partial class FormCreateTask : Form
    {
        public string Title { get => this.textBoxTitle.Text; }
        public string Description { get => this.textBoxDescription.Text; }
        public string Board { get => this.dropDownBoard.SelectedValue.ToString(); }

        public FormCreateTask(List<Board> boards)
        {
            InitializeComponent();
            this.dropDownBoard.DataSource = boards;
        }

        private void FormConnect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void buttonCreate_Click(object sender, EventArgs e)
        {
            var result = TitleIsValid()
                + DescriptionIsValid();

            // If there are no errors - return
            if (this.buttonCreate.DialogResult == DialogResult.OK)
            {
                return;
            }
            if (string.IsNullOrEmpty(result.Trim()))
            {
                this.buttonCreate.DialogResult = DialogResult.OK;
                this.buttonCreate.PerformClick();
            }
            else
            {
                MessageBox.Show(result.Trim(), "Errors");
            }
        }

        private string TitleIsValid()
        {
            var title = this.Title;
            var errors = new StringBuilder();

            if (string.IsNullOrEmpty(title))
            {
                errors.AppendLine("Title field is required.");
            }

            if (title.Length > 0 && title.Length < MinTaskTitle)
            {
                errors.AppendLine($"Title should be at least {MinTaskTitle} characters long.");
            }

            if (title.Length > MaxTaskTitle)
            {
                errors.AppendLine($"Title should not be more than {MaxTaskTitle} characters long.");
            }

            return errors.ToString();
        }

        private string DescriptionIsValid()
        {
            var description = this.Description;
            var errors = new StringBuilder();

            if (string.IsNullOrEmpty(description))
            {
                errors.AppendLine("Description field is required.");
            }

            if (description.Length > 0 && description.Length < MinTaskDescription)
            {
                errors.AppendLine($"Description should be at least {MinTaskDescription} characters long.");
            }

            if (description.Length > MaxTaskDescription)
            {
                errors.AppendLine($"Description should not be more than {MaxTaskDescription} characters long.");
            }

            return errors.ToString();
        }
    }
}

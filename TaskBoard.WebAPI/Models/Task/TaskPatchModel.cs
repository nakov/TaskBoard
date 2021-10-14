using System.ComponentModel.DataAnnotations;

using TaskBoard.Data;

namespace TaskBoard.WebAPI.Models.Task
{
    using static DataConstants;
    public class TaskPatchModel
    {
        [StringLength(MaxTaskTitle, MinimumLength = MinTaskTitle)]
        public string Title { get; init; }

        [StringLength(MaxTaskDescription, MinimumLength = MinTaskDescription)]
        public string Description { get; init; }

        [StringLength(MaxBoardName)]
        public string Board { get; init; }
    }
}

using TaskBoard.Data;

using System.ComponentModel.DataAnnotations;

namespace TaskBoard.WebAPI.Models.Task
{
    using static DataConstants;
    public class TaskBindingModel
    {
        [Required]
        [StringLength(MaxTaskTitle, MinimumLength = MinTaskTitle)]
        public string Title { get; init; }

        [Required]
        [StringLength(MaxTaskDescription, MinimumLength = MinTaskDescription)]
        public string Description { get; init; }

        [Required]
        [StringLength(MaxBoardName)]
        public string Board { get; init; }
    }
}

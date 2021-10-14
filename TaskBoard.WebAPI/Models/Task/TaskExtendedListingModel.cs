using TaskBoard.WebAPI.Models.User;

namespace TaskBoard.WebAPI.Models.Task
{
    public class TaskExtendedListingModel : TaskListingModel
    {
        public string CreatedOn { get; init; }

        public string Board { get; init; }

        public UserListingModel Owner { get; init; }
    }
}

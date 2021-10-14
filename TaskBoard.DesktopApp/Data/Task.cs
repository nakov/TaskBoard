namespace TaskBoard.DesktopApp.Data
{
    public class Task
    {
        public int Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string CreatedOn { get; init; }
        public string Board { get; init; }
        public User Owner { get; init; }
    }
}
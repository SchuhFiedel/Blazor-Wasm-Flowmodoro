namespace Flowmodoro.Client.Models
{
    public class JobModel
    {
        public Guid Id { get; set; }
        public JobStatuses Status { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public DateTime LastUpdated { get; set; }
        public TimeSpan TimeWorkedOnTask { get; set; } = TimeSpan.FromSeconds(0);
    }
    public enum JobStatuses
    {
        Todo,
        Started,
        Completed
    }
}


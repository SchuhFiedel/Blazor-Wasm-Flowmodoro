namespace Flowmodoro.Client.Models
{
    public class JobModel
    {
        public Guid Id  {  get;  set; }
        public JobStatuses Status {  get; set; }
        public string Description  {  get;  set; }
        public DateTime LastUpdated  { get; set; }
    }
    public enum JobStatuses
    {
        Todo,
        Started,
        Completed
    }
}


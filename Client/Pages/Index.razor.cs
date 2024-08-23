using Flowmodoro.Client.Interfaces;
using Flowmodoro.Client.Models;
using Microsoft.AspNetCore.Components;

namespace Flowmodoro.Client.Pages
{
    public partial class Index
    {
 

        [CascadingParameter(Name = "ErrorComponent")]
        protected IToastComponent ToastComponent { get; set; } = null!;

        void LogError(Exception ex)
        {
            this.ToastComponent.ShowError(ex.Message, ex.InnerException?.Message ?? "");
        }

        public JobModel? SelectedJob = null;

        public List<JobModel> JobModelList = new List<JobModel> {
            new JobModel() { Id = Guid.NewGuid(), Name = "Test1", Description = "_ RRE EEieieie iaoiaoai pp pp poeuraru iaosudosiud aosifaoisfjas aslkfjalksfj alkjjskja akasljdaksf_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            new JobModel() { Id = Guid.NewGuid(), Name = "Test3", Description = "_Three_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            new JobModel() { Id = Guid.NewGuid(), Name = "Test4", Description = "_Four_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            new JobModel() { Id = Guid.NewGuid(), Name = "Test5", Description = "_Five_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            new JobModel() { Id = Guid.NewGuid(), Name = "Test6", Description = "_Six_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            new JobModel() { Id = Guid.NewGuid(), Name = "Test7", Description = "_Seven_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            new JobModel() { Id = Guid.NewGuid(), Name = "Test8", Description = "_Eight_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            new JobModel() { Id = Guid.NewGuid(), Name = "Test2", Description = "_Two_", LastUpdated = DateTime.Now, Status = JobStatuses.Todo},
            };

        void OnJobItemSelected(Guid? itemId)
        {
            JobModel? item = JobModelList.Find(x => x.Id == itemId);
            this.SelectedJob = item;
        }
    }

}

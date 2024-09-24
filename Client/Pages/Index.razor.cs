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

        public List<JobModel> JobModelList = new List<JobModel>();

        void OnJobItemSelected(Guid? itemId)
        {
            JobModel? item = JobModelList.Find(x => x.Id == itemId);
            this.SelectedJob = item;
        }
    }

}

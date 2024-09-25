using Flowmodoro.Client.Interfaces;
using Flowmodoro.Client.Models;
using Flowmodoro.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Recurop;

namespace Flowmodoro.Client.Pages
{
    public partial class Index: IDisposable
    {
        [Inject]
        protected Blazored.LocalStorage.ISyncLocalStorageService syncLocalStorageService { get; set; } = null!;

        [CascadingParameter(Name = "ErrorComponent")]
        protected IToastComponent ToastComponent { get; set; } = null!;
        [Inject]
        public RecurringOperationsManager Recurop { get; init; } = null!;

        private static Action callNonStaticFunction = null!;
        private DragAndDropListComponent DragDropListCompRef { get; set; } = null!;
        private bool onRefPopulated = false;

        public JobModel? SelectedJob = null;
        public List<JobModel> JobModelList = new List<JobModel>();
        public RecurringOperation SaveInIntervalsOperation { get; private set; } = null!;

        private void LogError(Exception ex)
        {
            this.ToastComponent.ShowError(ex.Message, ex.InnerException?.Message ?? "");
        }

        private void OnJobItemSelected(Guid? itemId)
        {
            JobModel? item = JobModelList.Find(x => x.Id == itemId);
            this.SelectedJob = item;
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if(onRefPopulated == false && DragDropListCompRef != null)
            {
                onRefPopulated = true;
                var selectedJobId = syncLocalStorageService.GetItem<Guid?>(nameof(SelectedJob));
                if (selectedJobId.HasValue)
                {
                    OnJobItemSelected(selectedJobId.Value);
                    Console.WriteLine("ref", DragDropListCompRef.JobsList.Count());
                    DragDropListCompRef.TrackingClickHandler(selectedJobId.Value);

                }
            }
        }

        protected override void OnParametersSet()
        {
            Console.WriteLine(nameof(OnAfterRender));
            callNonStaticFunction = OnReload;
            var list = syncLocalStorageService.GetItem<List<JobModel>>(nameof(JobModelList));
            if (list != null && list.Any())
            {
                this.JobModelList = list;
            }

            SaveInIntervalsOperation = new("SaveInIntervals");
            SaveInIntervalsOperation.Operation = OnReload;
            SaveInIntervalsOperation.OperationFaulted += LogError;

            Recurop.StartRecurring(SaveInIntervalsOperation, TimeSpan.FromSeconds(30));
            
        }

        [JSInvokable]
        public static void PageAboutToBeReloaded()
        {
            callNonStaticFunction.Invoke();
        }

        public void OnReload()
        {
            syncLocalStorageService.SetItem(nameof(JobModelList), JobModelList);
            syncLocalStorageService.SetItem(nameof(SelectedJob), SelectedJob?.Id);
        }

        public void Dispose()
        {
            Recurop.CancelRecurring(SaveInIntervalsOperation);
        }
    }

}

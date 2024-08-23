using Flowmodoro.Client.Interfaces;
using Flowmodoro.Client.Models;
using Microsoft.AspNetCore.Components;
using Recurop;

namespace Flowmodoro.Client.Shared
{
    public partial class TimerComponent: ComponentBase, IDisposable
    {
        const string StopWatchText = "Work Mode";
        const string TimerText = "Break Mode";

        public RecurringOperation? TimerOpertation { get; private set; }
        public RecurringOperation? BlinkerOperation { get; private set; }
        public RecurringOperation? StopWatchOperation { get; private set; }
        TimeSpan _displayTime = default;
        List<TimeSpan>? _laptimes;
        public int Seconds { get; private set; }
        bool _timerVisible = true;
        bool _isInStopwatchMode = true;

        public string TitleText = StopWatchText;
        public string SubtitleText = " ";

        [CascadingParameter(Name = "ErrorComponent")]
        protected IToastComponent ToastComponent { get; set; } = null!;

        [Inject]
        public RecurringOperationsManager Recurop { get; init; }

        [Parameter]
        public JobModel? CurrentJob { get; set; }
        [Parameter]
        public EventCallback<JobModel> CurrentJobChanged { get; set; }

        protected override void OnInitialized()
        {
            StopWatchOperation = new("stopwatch");
            StopWatchOperation.Operation = IncrementTimer;
            StopWatchOperation.StatusChanged += StopWatchOperationStatusChanged;
            StopWatchOperation.OperationFaulted += LogError;

            TimerOpertation = new("timer");
            TimerOpertation.Operation = DecrementTimer;
            TimerOpertation.StatusChanged += TimerOperationStatusChanged;
            TimerOpertation.OperationFaulted += LogError;

            BlinkerOperation = new("blinker");
            BlinkerOperation.Operation = BlinkTimer;
            BlinkerOperation.StatusChanged += BlinkerOperationStatusChanged;

            _laptimes = new();
        }

        void StartTimer()
        {
            Recurop.CancelRecurring(BlinkerOperation);
            if (_isInStopwatchMode)
            {
                ResetTimer();
                Recurop.StartRecurring(StopWatchOperation, TimeSpan.FromSeconds(1));
            }
            else
            {
                Recurop.StartRecurring(TimerOpertation, TimeSpan.FromSeconds(1));
            }
        }

        void StopTimer()
        {
            Recurop.CancelRecurring(BlinkerOperation);
            if (_isInStopwatchMode)
            {
                Recurop.CancelRecurring(StopWatchOperation);
            }
            else
            {
                Recurop.CancelRecurring(TimerOpertation);
            }
        }

        async void IncrementTimer()
        {
            Seconds++;
            _displayTime = TimeSpan.FromSeconds(Seconds);
            int tmpCalcSeconds = CalculateSeconds(Seconds);
            this.SubtitleText = $"Current {TimerText} Time: {TimeSpan.FromSeconds(tmpCalcSeconds)}";
            Console.WriteLine(this.CurrentJob?.Name);
            if(this.CurrentJob != null)
            {
                CurrentJob.TimeWorkedOnTask += TimeSpan.FromSeconds(1);
                CurrentJob.LastUpdated = DateTime.Now;
                Console.WriteLine(this.CurrentJob?.TimeWorkedOnTask);
                Console.WriteLine(this.CurrentJob?.LastUpdated);
                await CurrentJobChanged.InvokeAsync(CurrentJob);
            }
            StateHasChanged();
        }


        void DecrementTimer()
        {
            //Decrement until zero
            //if zero Stop Timer and start to play sound -> Play Sound Operation and Blink Operation
            if (Seconds > 0)
            {

                Seconds--;
                _displayTime = TimeSpan.FromSeconds(Seconds);
            }
            else
            {
                StopTimer();
                Recurop.StartRecurring(BlinkerOperation, TimeSpan.FromSeconds(0.5));
            }
            StateHasChanged();
        }

        void ResetTimer()
        {
            Seconds = 0;
            _displayTime = TimeSpan.Zero;
            Recurop.CancelRecurring(BlinkerOperation);
            Recurop.CancelRecurring(StopWatchOperation);
            Recurop.CancelRecurring(TimerOpertation);
            StateHasChanged();
        }

        void BlinkTimer()
        {
            _timerVisible = !_timerVisible;
            StateHasChanged();
        }

        void BlinkerOperationStatusChanged()
        {
            switch (BlinkerOperation?.Status)
            {
                case RecurringOperationStatus.Idle:
                case RecurringOperationStatus.Paused:
                case RecurringOperationStatus.Executing:
                    break;
                case RecurringOperationStatus.Cancelled:
                    _timerVisible = true;
                    break;
                default:
                    break;
            }
        }

        void StopWatchOperationStatusChanged()
        {
            switch (StopWatchOperation?.Status)
            {
                case RecurringOperationStatus.Idle:
                case RecurringOperationStatus.Paused:
                case RecurringOperationStatus.Executing:
                    break;
                case RecurringOperationStatus.Cancelled:
                    this.onTimerStop();
                    break;
                default:
                    break;
            }
        }

        void TimerOperationStatusChanged()
        {
            switch (TimerOpertation?.Status)
            {
                case RecurringOperationStatus.Idle:
                case RecurringOperationStatus.Paused:
                case RecurringOperationStatus.Executing:
                    break;
                case RecurringOperationStatus.Cancelled:
                    this.onTimerStop();
                    break;
                default:
                    break;
            }
        }

        void onTimerStop()
        {
            this.ToastComponent.ShowSuccess($"{TitleText} Stopped!", _displayTime.ToString());
            Console.WriteLine(this.CurrentJob?.TimeWorkedOnTask);
            //_displayTime = TimeSpan.Zero;
            //_elapsedSeconds = 0;
            _laptimes?.Clear();
        }

        void SwapMode()
        {
            string prevText = this.TitleText;
            if (_isInStopwatchMode)
            {
                int tmpDisplaySeconds = Seconds;
                Seconds = CalculateSeconds(Seconds);
                _displayTime = TimeSpan.FromSeconds(Seconds);
                this.TitleText = $"{TimerText} {_displayTime}";
                this.SubtitleText = $"Last Work Session: {TimeSpan.FromSeconds(tmpDisplaySeconds)}";
            }
            else
            {
                this.TitleText = StopWatchText;
                this.SubtitleText = " ";
            }

            this.ToastComponent.ShowInfo($"Swap {prevText} -> {TitleText}");
            this._isInStopwatchMode = !this._isInStopwatchMode;
        }

        bool CanBeSwapped()
        {
            bool isRunning = StopWatchOperation.IsRecurring || TimerOpertation.IsRecurring;
            if ((!isRunning && _isInStopwatchMode) || (!isRunning && !_isInStopwatchMode && this.Seconds == 0))
                return true;

            return false;

        }

        #region poo

        public void Dispose()
        {
            Recurop.CancelRecurring(StopWatchOperation);
            Recurop.CancelRecurring(BlinkerOperation);
        }
        void LogError(Exception ex)
        {
            this.ToastComponent.ShowError(ex.Message, ex.InnerException?.Message ?? "");
        }

        public int CalculateSeconds(int seconds)
        {
            int newSeconds = seconds / 5;
            return newSeconds;
        }

        #endregion
    }
}

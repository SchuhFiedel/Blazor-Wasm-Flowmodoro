using Flowmodoro.Client.Interfaces;
using Microsoft.AspNetCore.Components;
using Recurop;

namespace Flowmodoro.Client.Pages
{
    public partial class Index
    {
        const string StopWatchText = "Work Mode";
        const string TimerText = "Break Mode";

        public RecurringOperation? TimerOpertation { get; private set;  }
        public RecurringOperation? BlinkerOperation { get; private set; }
        public RecurringOperation? StopWatchOperation { get; private set; }
        TimeSpan _displayTime = default;
        List<TimeSpan>? _laptimes;
        public int Seconds { get; private set; }
        bool _timerVisible = true;
        bool _isInStopwatchMode = true;

        public string TitleText = StopWatchText;

        [CascadingParameter(Name = "ErrorComponent")]
        protected IToastComponent ToastComponent { get; set; } = null!;

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

        void IncrementTimer()
        {
            Seconds++;
            _displayTime = TimeSpan.FromSeconds(Seconds);
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

        void LapTime()
        {
            _laptimes?.Add(_displayTime);
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

            //_displayTime = TimeSpan.Zero;
            //_elapsedSeconds = 0;
            _laptimes?.Clear();
        }

        void SwapMode()
        {
            string prevText = this.TitleText;
            if (_isInStopwatchMode)
            {
                Seconds = this.Seconds / 5;
                _displayTime = TimeSpan.FromSeconds(Seconds);
                this.TitleText = $"{TimerText} {_displayTime}";
            }
            else
            {
                this.TitleText = StopWatchText;
            }

            this.ToastComponent.ShowInfo($"Swap {prevText} -> {TitleText}");
            this._isInStopwatchMode = !this._isInStopwatchMode;
        }

        bool CanBeSwapped()
        {
            bool isRunning = StopWatchOperation.IsRecurring || TimerOpertation.IsRecurring;
            Console.WriteLine("is Running {0}", isRunning);
            Console.WriteLine("isInStopWatchMode {0}, ", _isInStopwatchMode);
            Console.WriteLine((!isRunning && _isInStopwatchMode) || (!isRunning && !_isInStopwatchMode && this.Seconds == 0));
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

        #endregion
    }
}

using Flowmodoro.Client.Interfaces;
using Microsoft.AspNetCore.Components;
using Recurop;

namespace Flowmodoro.Client.Pages
{
    public partial class Index
    {
        RecurringOperation? _timerOperation;
        RecurringOperation? _blinkerOperation;
        RecurringOperation? _stopWatchOperation;
        TimeSpan _displayTime = default;
        List<TimeSpan>? _laptimes;
        int _elapsedSeconds;
        bool _timerVisible = true;
        bool _isInStopwatchMode = true;

        public string TitleText = "Stopwatch";

        [CascadingParameter(Name = "ErrorComponent")]
        protected IToastComponent ToastComponent { get; set; } = null!;

        protected override void OnInitialized()
        {
            _stopWatchOperation = new("stopwatch");
            _stopWatchOperation.Operation = IncrementTimer;
            _stopWatchOperation.StatusChanged += StopWatchOperationStatusChanged;
            _stopWatchOperation.OperationFaulted += LogError;

            _timerOperation = new("timer");
            _timerOperation.Operation = DecrementTimer;
            _timerOperation.StatusChanged += TimerOperationStatusChanged;
            _timerOperation.OperationFaulted += LogError;

            _blinkerOperation = new("blinker");
            _blinkerOperation.Operation = BlinkTimer;
            _blinkerOperation.StatusChanged += BlinkerOperationStatusChanged;

            _laptimes = new();
        }

        void StartTimer()
        {
            Recurop.CancelRecurring(_blinkerOperation);
            if (_isInStopwatchMode)
            {
                _elapsedSeconds = 0;
                _displayTime = TimeSpan.Zero;
                Recurop.StartRecurring(_stopWatchOperation, TimeSpan.FromSeconds(1));
            }
            else
            {
                Recurop.StartRecurring(_timerOperation, TimeSpan.FromSeconds(1));
            }
        }

        void PauseTimer()
        {
            if (_isInStopwatchMode)
            {
                Recurop.PauseRecurring(_stopWatchOperation);
            }
            else
            {
                Recurop.PauseRecurring(_timerOperation);
            }

            Recurop.StartRecurring(_blinkerOperation, TimeSpan.FromSeconds(0.5));
        }

        void ResumeTimer()
        {
            if (_isInStopwatchMode)
            {
                Recurop.ResumeRecurring(_stopWatchOperation);
            }
            else 
            { 
                Recurop.ResumeRecurring(_timerOperation);
            }

            Recurop.CancelRecurring(_blinkerOperation);
        }

        void StopTimer()
        {
            Recurop.CancelRecurring(_blinkerOperation);
            if (_isInStopwatchMode)
            {
                Recurop.CancelRecurring(_stopWatchOperation);
            }
            else
            {
                Recurop.CancelRecurring(_timerOperation);
            }
        }

        void IncrementTimer()
        {
            _elapsedSeconds++;
            _displayTime = TimeSpan.FromSeconds(_elapsedSeconds);
            StateHasChanged();
        }

        void DecrementTimer()
        {
            //Decrement until zero
            //if zero Stop Timer and start to play sound -> Play Sound Operation and Blink Operation
            if (_elapsedSeconds > 0)
            {
                
                _elapsedSeconds--;
                _displayTime = TimeSpan.FromSeconds(_elapsedSeconds);
            }
            else
            {
                StopTimer();
                Recurop.StartRecurring(_blinkerOperation, TimeSpan.FromSeconds(0.5));
            }
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
            switch (_blinkerOperation?.Status)
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
            switch (_stopWatchOperation?.Status)
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
            switch (_timerOperation?.Status)
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
            this.ToastComponent.ShowSuccess("Timer Stopped!", _displayTime.ToString());
            //_displayTime = TimeSpan.Zero;
            //_elapsedSeconds = 0;
            _laptimes?.Clear();
        }

        void SwapMode()
        {
            this.ToastComponent.ShowInfo($"Swap {_isInStopwatchMode} -> {!_isInStopwatchMode}");
            this.TitleText = this._isInStopwatchMode ? "Timer" : "Stopwatch";
            this._isInStopwatchMode = !this._isInStopwatchMode;
        }

        #region poo

        public void Dispose()
        {
            Recurop.CancelRecurring(_stopWatchOperation);
            Recurop.CancelRecurring(_blinkerOperation);
        }
        void LogError(Exception ex)
        {
            this.ToastComponent.ShowError(ex.Message, ex.InnerException?.Message ?? "");
        }

        #endregion
    }
}

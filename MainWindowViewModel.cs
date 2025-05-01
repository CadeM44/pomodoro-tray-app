using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using PomodoroTrayApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PomodoroTrayApp;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private TimeSpan _currentDuration = TimeSpan.FromMinutes(TimerService.DEFAULT_DURATION_MINS);
    /// <summary> Represents the configured duration when stopped/finished, or the remaining time when running. </summary>
    public TimeSpan CurrentDuration
    {
        get => _currentDuration;
        private set
        {
            var validValue = value >= TimeSpan.Zero ? value : TimeSpan.Zero;

            if (SetProperty(ref _currentDuration, validValue))
            {
                OnPropertyChanged(nameof(CurrentDurationString));
                OnPropertyChanged(nameof(IsRunning));
            }
        }
    }

    public string CurrentDurationString => CurrentDuration.ToString(@"mm\:ss");
    public bool IsRunning => _timerService?.CurrentState == TimerState.Running;

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand ExitCommand { get; }
    public ICommand DecrementMinutesCommand { get; }
    public ICommand IncrementMinutesCommand { get; }
    public ICommand SetIntervalCommand { get; }

    private readonly ITimerService _timerService;
    private TaskbarIcon? _trayIcon;

    public MainWindowViewModel(ITimerService timer)
    {
        _timerService = timer;
        StartCommand = new RelayCommand(() => _timerService.Start(CurrentDuration));
        StopCommand = new RelayCommand(() => _timerService.Stop());

        DecrementMinutesCommand = new RelayCommand(
            execute: () => SetDuration(TimeSpan.FromMinutes(CurrentDuration.TotalMinutes - 1)),
            canExecute: () => _timerService.CurrentState != TimerState.Running
            );

        IncrementMinutesCommand = new RelayCommand(
             execute: () => SetDuration(TimeSpan.FromMinutes(CurrentDuration.TotalMinutes + 1)),
             canExecute: () => _timerService.CurrentState != TimerState.Running
            );

        SetIntervalCommand = new RelayCommand<string>(
            execute: (param) =>
            {
                if (TimeSpan.TryParse(param, out TimeSpan parsedDuration))
                    SetDuration(parsedDuration);
            },
            canExecute: (param) => _timerService.CurrentState != TimerState.Running
            );

        ShowWindowCommand = new RelayCommand(() => Application.Current.MainWindow?.Show());
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

        _timerService.StateChanged += _timerService_StateChanged;
        _timerService.Tick += _timerService_Tick;
    }

    public void Loaded()
    {
        _trayIcon = Application.Current.TryFindResource("TrayIcon") as TaskbarIcon;
        if (_trayIcon != null)
        {
            _trayIcon.Visibility = Visibility.Visible;
            _trayIcon.DataContext = this;
        }
    }

    private void _timerService_Tick(object? sender, TimeSpan remaining)
    {
        CurrentDuration = remaining;
    }

    private void _timerService_StateChanged(object? sender, TimerState newState)
    {
        if (newState == TimerState.Stopped || newState == TimerState.Finished)
        {
            int intendedMinutes = (int)Math.Max(1, Math.Round(CurrentDuration.TotalMinutes));
            if (newState == TimerState.Finished)
            {
                CurrentDuration = TimeSpan.FromMinutes(intendedMinutes);
                _trayIcon?.ShowBalloonTip("Pomodoro Complete!", $"Finished a session. Time for a break!", BalloonIcon.Info);
            }
        }

        (StartCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (StopCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (IncrementMinutesCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (DecrementMinutesCommand as RelayCommand)?.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsRunning));
    }

    private void SetDuration(TimeSpan newDuration)
    {
        if (_timerService.CurrentState == TimerState.Running)
            return;

        double totalMinutes = Math.Clamp(newDuration.TotalMinutes, 1, 120);
        CurrentDuration = TimeSpan.FromMinutes(totalMinutes);
        (StartCommand as RelayCommand)?.NotifyCanExecuteChanged();
    }

    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (!Equals(field, newValue))
        {
            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        return false;
    }

}


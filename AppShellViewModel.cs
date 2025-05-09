using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using PomodoroTrayApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace PomodoroTrayApp;

public class AppShellViewModel : INotifyPropertyChanged
{
    private const double WINDOW_WIDTH = 350;
    private const double WINDOW_HEIGHT = 225;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private TimeSpan _currentDuration;
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

    private TimeSpan _configuredDuration = TimeSpan.FromMinutes(TimerService.DEFAULT_DURATION_MINS);
    private readonly ITimerService _timerService;
    private readonly Func<MainWindow> _mainWindowFactory;
    private MainWindow? _currentWindowInstance;
    private TaskbarIcon? _trayIcon;


    public AppShellViewModel(ITimerService timer, Func<MainWindow> windowFactory)
    {
        CurrentDuration = _configuredDuration;
        _timerService = timer;
        _mainWindowFactory = windowFactory;

        StartCommand = new RelayCommand(StartTimer);
        StopCommand = new RelayCommand(() => _timerService.Stop());

        DecrementMinutesCommand = new RelayCommand(
            execute: () => SetDuration(TimeSpan.FromMinutes(CurrentDuration.TotalMinutes - 1)),
            canExecute: () => !IsRunning
            );

        IncrementMinutesCommand = new RelayCommand(
             execute: () => SetDuration(TimeSpan.FromMinutes(CurrentDuration.TotalMinutes + 1)),
             canExecute: () => !IsRunning
            );

        SetIntervalCommand = new RelayCommand<string>(
            execute: (param) =>
            {
                if (TimeSpan.TryParse(param, out TimeSpan parsedDuration))
                    SetDuration(parsedDuration);
            },
            canExecute: (param) => !IsRunning
            );

        ShowWindowCommand = new RelayCommand(ShowWindow);
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

        _timerService.StateChanged += _timerService_StateChanged;
        _timerService.Tick += _timerService_Tick;
    }

    public void SetTrayIcon(TaskbarIcon trayIcon)
    {
        _trayIcon ??= trayIcon; // assigns arg to _trayIcon only if _trayIcon is currently null
    }

    private void StartTimer()
    {
        _configuredDuration = CurrentDuration;
        _timerService.Start(CurrentDuration);
    }

    private void ShowWindow()
    {
        if (_currentWindowInstance != null && _currentWindowInstance.IsLoaded)
        {
            if (_currentWindowInstance.WindowState == WindowState.Minimized)
                _currentWindowInstance.WindowState = WindowState.Normal;

            _currentWindowInstance.Activate();
        }
        else
        {
            _currentWindowInstance = _mainWindowFactory();
            _currentWindowInstance.DataContext = this;
            _currentWindowInstance.Closed += MainWindow_Closed;
            _currentWindowInstance.Height = WINDOW_HEIGHT;
            _currentWindowInstance.Width = WINDOW_WIDTH;
            _currentWindowInstance.Show();
            _currentWindowInstance.Activate();
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        if (sender is MainWindow window)
        {
            window.Closed -= MainWindow_Closed;
            _currentWindowInstance = null;
        }
    }

    private void _timerService_Tick(object? sender, TimeSpan remaining)
    {
        CurrentDuration = remaining;
    }

    private void _timerService_StateChanged(object? sender, TimerState newState)
    {
        if (newState == TimerState.Finished)
        {
            CurrentDuration = _configuredDuration;
            _trayIcon?.ShowBalloonTip("Pomodoro Complete!", $"Finished a session. Time for a break!", BalloonIcon.Info);
        }

        (StartCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (StopCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (IncrementMinutesCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (DecrementMinutesCommand as RelayCommand)?.NotifyCanExecuteChanged();
        (SetIntervalCommand as RelayCommand<string>)?.NotifyCanExecuteChanged();
        OnPropertyChanged(nameof(IsRunning));
    }

    private void SetDuration(TimeSpan newDuration)
    {
        if (IsRunning)
            return;

        double totalMinutes = Math.Clamp(newDuration.TotalMinutes, 1, 120);
        var duration = TimeSpan.FromMinutes(totalMinutes);
        CurrentDuration = duration;
        _configuredDuration = duration;
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


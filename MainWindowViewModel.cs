using CommunityToolkit.Mvvm.Input;
using Hardcodet.Wpf.TaskbarNotification;
using PomodoroTrayApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace PomodoroTrayApp;

public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private TimeSpan _displayTime;
    public TimeSpan DisplayTime
    {
        get => _displayTime;
        set { _displayTime = value; OnPropertyChanged(); }
    }

    public ICommand StartCommand { get; }
    public ICommand StopCommand { get; }
    public ICommand ShowWindowCommand { get; }
    public ICommand ExitCommand { get; }

    private readonly ITimerService _timerService;
    private TaskbarIcon? _trayIcon;

    public MainWindowViewModel(ITimerService timer)
    {
        _timerService = timer;
        StartCommand = new RelayCommand(() => _timerService.Start(TimeSpan.FromMinutes(25)));
        StopCommand = new RelayCommand(() => _timerService.Stop());
        ShowWindowCommand = new RelayCommand(() => Application.Current.MainWindow?.Show());
        ExitCommand = new RelayCommand(() => Application.Current.Shutdown());

        _timerService.Tick += (_, remaining) => DisplayTime = remaining;
        //_timer.SessionCompleted += (_, __) =>
        //    new ToastContentBuilder()
        //        .AddText("Pomodoro Complete!")
        //        .Show();

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
}


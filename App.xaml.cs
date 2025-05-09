using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PomodoroTrayApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PomodoroTrayApp;

public partial class App : Application
{
    private readonly IHost _appHost;
    private TaskbarIcon? _trayIconAndMenu;

    public App()
    {
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddTransient<MainWindow>();
        builder.Services.AddSingleton<Func<MainWindow>>(sp => () => sp.GetRequiredService<MainWindow>());
        builder.Services.AddSingleton<ITimerService, TimerService>();
        builder.Services.AddSingleton<AppShellViewModel>();
        _appHost = builder.Build();
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (_appHost == null)
            return;

        await _appHost.StartAsync();
        CreateNotifyIcon(_appHost.Services.GetRequiredService<AppShellViewModel>());
    }

    private void CreateNotifyIcon(AppShellViewModel mainWindowViewModel)
    {
        _trayIconAndMenu = Current.TryFindResource("TrayIcon") as TaskbarIcon;

        if (_trayIconAndMenu == null)
            throw new Exception("Cannot find tray");

        try
        {
            Func<MainWindow> windowFactory = () => _appHost.Services.GetRequiredService<MainWindow>();
            Uri iconUri = new("pack://application:,,,Assets/timer-icon.png", UriKind.RelativeOrAbsolute);
            _trayIconAndMenu.IconSource = new BitmapImage(iconUri);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading icon: {ex.Message}");
        }

        _trayIconAndMenu.DataContext = mainWindowViewModel;
        _trayIconAndMenu.Visibility = Visibility.Visible;
        mainWindowViewModel.SetTrayIcon(_trayIconAndMenu);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIconAndMenu?.Dispose();
        base.OnExit(e);
    }
}
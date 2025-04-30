using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PomodoroTrayApp.Services;
using System.Windows;

namespace PomodoroTrayApp;

public partial class App : Application
{
    private readonly IHost _appHost;

    public App()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<ITimerService, TimerService>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        _appHost = builder.Build();
    }

    protected async override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (_appHost == null)
            return;

        double windowWidth = 450;
        double windowHeight = 250;

        await _appHost.StartAsync();
        var mainWindow = _appHost.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _appHost.Services.GetRequiredService<MainWindowViewModel>();
        mainWindow.Width = windowWidth;
        mainWindow.Height = windowHeight;
        Current.MainWindow = mainWindow;
        Current.MainWindow.Show();
        //Current.MainWindow.Hide();
    }
}
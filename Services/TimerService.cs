using System.Windows.Threading;

namespace PomodoroTrayApp.Services;

public class TimerService : ITimerService
{
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private TimeSpan _remaining;

    public event EventHandler<TimeSpan>? Tick;
    public event EventHandler? SessionCompleted;

    public void Start(TimeSpan interval)
    {
        _remaining = interval;
        _timer.Tick += OnTick;
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
        _timer.Tick -= OnTick;
    }

    private void OnTick(object sender, EventArgs e)
    {
        _remaining -= TimeSpan.FromSeconds(1);
        Tick?.Invoke(this, _remaining);

        if (_remaining <= TimeSpan.Zero)
        {
            Stop();
            SessionCompleted?.Invoke(this, EventArgs.Empty);
        }
    }
}



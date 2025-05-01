using System.Windows.Threading;

namespace PomodoroTrayApp.Services;

public class TimerService : ITimerService
{
    public const int DEFAULT_DURATION_MINS = 25;

    public event EventHandler<TimerState>? StateChanged;
    public event EventHandler<TimeSpan>? Tick;

    public TimerState CurrentState { get; private set; } = TimerState.Stopped;

    private readonly DispatcherTimer _timer;
    private TimeSpan _remaining;
    private bool _disposed;

    public TimerService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;
    }

    public void Start(TimeSpan interval)
    {
        if (CurrentState == TimerState.Running)
            return;

        _remaining = interval;
        SetState(TimerState.Running);
        Tick?.Invoke(this, _remaining);
        _timer.Start();
    }

    public void Stop()
    {
        if (CurrentState != TimerState.Running)
            return;

        _timer.Stop();
        SetState(TimerState.Stopped);
    }

    private void OnTick(object? sender, EventArgs _)
    {
        try
        {
            if (CurrentState != TimerState.Running)
                return;

            _remaining = _remaining.Subtract(TimeSpan.FromSeconds(1));
            Tick?.Invoke(this, _remaining);

            if (_remaining <= TimeSpan.Zero)
            {
                _timer.Stop();
                SetState(TimerState.Finished);
            }
        }
        catch
        {
            // swallow to keep the timer alive
        }
    }

    private void SetState(TimerState newState)
    {
        if (CurrentState == newState)
            return;

        CurrentState = newState;
        StateChanged?.Invoke(this, CurrentState);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) 
            return;

        if (disposing)
        {
            if (_timer.IsEnabled)
                _timer.Stop();

            _timer.Tick -= OnTick;
            StateChanged = null;
            Tick = null;
        }

        _disposed = true;
    }
}

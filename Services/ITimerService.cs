namespace PomodoroTrayApp.Services;

public enum TimerState
{
    Stopped,
    Running,
    Finished
    // Paused
}

public interface ITimerService :  IDisposable
{
    event EventHandler<TimerState> StateChanged;
    event EventHandler<TimeSpan> Tick;
    TimerState CurrentState { get; }
    void Start(TimeSpan interval);
    void Stop();
}


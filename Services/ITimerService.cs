namespace PomodoroTrayApp.Services;

public interface ITimerService
{
    /// <summary> Fires every second </summary>
    event EventHandler<TimeSpan> Tick;
    /// <summary> Fires when interval elapses </summary>
    event EventHandler SessionCompleted;
    void Start(TimeSpan interval);
    void Stop();
}


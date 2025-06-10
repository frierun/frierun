using Frierun.Server.Handlers;
using Newtonsoft.Json;

namespace Frierun.Server;

public class StateManager
{
    public bool Ready { get; private set; } = true;
    public string TaskName { get; private set; } = string.Empty;
    public HandlerExceptionResult? Error => Exception?.Result;
    [JsonIgnore] public HandlerException? Exception { get; set; }
    
    private readonly object _lock = new();
    
    /// <summary>
    /// Starts task with given name. Returns false if task is already running.
    /// </summary>
    public bool StartTask(string taskName)
    {
        lock (_lock)
        {
            if (!Ready)
            {
                return false;
            }
            TaskName = taskName;
            Ready = false;
            Exception = null;
            return true;
        }
    }
    
    /// <summary>
    /// Finishes current task.
    /// </summary>
    public void FinishTask()
    {
        lock (_lock)
        {
            Ready = true;
            TaskName = string.Empty;
        }
    }
}
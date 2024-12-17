using NScript.Supervisor.Utils;

namespace NScript.Supervisor.Hubs;

public interface IMonitorClient
{  

    Task Status(Message msg);  

    /// <summary>
    /// 输出4096B大小的日志
    /// </summary>   
    /// <returns></returns>
    Task LastLogs(Message msg);

}

public enum ProcessStatus
{
    Unknown = 0,
    Running,
    Stoped,
    Restart,
}

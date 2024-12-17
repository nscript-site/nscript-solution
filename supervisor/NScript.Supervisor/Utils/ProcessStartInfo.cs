using System.Diagnostics;

namespace NScript.Supervisor.Utils;

/// <summary>
/// Process线程敏感，这里解耦下
/// </summary>
public class ProcessStartInfo
{
    public ProcessStartInfo(Process p)
    {
        try
        {
            Id = p?.Id.ToString();
            ProcessName = p?.ProcessName;
            StartTime = p?.StartTime ?? DateTime.Now;
        }
        catch { StartTime = DateTime.Now; }
    }
    public string Id { get; set; }
    public string ProcessName { get; set; }
    public DateTime StartTime { get; set; }
}

using Microsoft.AspNetCore.SignalR;
using Topshelf.Logging;
using NScript.Supervisor.Hubs;
using NScript.Supervisor.Utils;

namespace NScript.Supervisor;

public class DaemonService : BackgroundService, IDisposable
{
    private readonly LogWriter _logger;
    private readonly IConfiguration _config;
    private readonly IHubContext<MonitorHub> hubContext;
    private readonly MessageQueueService queueService;
    private List<GuardServiceConfig> _gsc;

    public DaemonService(ILogger<DaemonService> logger, IConfiguration config, IHubContext<MonitorHub> hubContext, MessageQueueService queueService)
    {
        _logger = HostLogger.Current.Get("DaemonService");
        this._config = config;
        this.hubContext = hubContext;
        this.queueService = queueService;
        _gsc = ParseGuardServiceConfig.Load(_config);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pes = _gsc.Select(x => new ProcessExecutor(x)).ToList();
        int.TryParse(_config["CheckInterval"], out var nInterval);
        
        if(nInterval <= 1000)
        {
            nInterval = 1000;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Info("检查配置的程序是否启动...");
            foreach (var c in pes)
            {
                try
                {
                    c.Execute();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    _logger.Error(ex);
                }

                if (stoppingToken.IsCancellationRequested) break;
            }

            if(queueService.Reader.TryRead(out var command))
            {
                try
                {
                    await ExecuteCommand(command, pes);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    _logger.Error(ex);
                }
            }
            else
            {
                await Task.Delay(nInterval);
            }
        }

        pes.ForEach(x => x.Dispose());
    }

    private async Task NotifyStatus(string name, ProcessStartInfo p)
    {
        ProcessRunStatus status;
        var isCn = LocalizationConstants.Lang == "zh-CN";
        
        var s = p?.Id != null ? "运行" : "停止";
        if (!isCn)
        {
            s = p?.Id != null ? "Running" : "Stop";
        }
        status = new ProcessRunStatus
        {
            Status = s,
            Pid = p?.Id,
            UpTime = (p?.Id !=null) ? (DateTime.Now - p.StartTime).ToString(@"dd\.hh\:mm\:ss") :"",
        };
        await hubContext.Clients.All.SendAsync("Status", new Message
        {
            Command = "Status",
            ProcessName = name ?? p?.ProcessName,
            Content = s,
            Status = status
        });

    }
    private async Task  ExecuteCommand(Message command, List<ProcessExecutor> pes)
    {
        //操作所有进程
        if (command.ProcessName == "[all]" || string.IsNullOrEmpty(command.ProcessName))
        {
            foreach(var pe in pes)
            {
                var p = pe.ExecuteCommand(command.Command, command.Content);

                if (command.Command == "LastLogs")
                {
                    await hubContext.Clients.Client(command.ClientId).SendAsync("LastLogs", new Message
                    {
                        Command = command.Command,
                        ProcessName = pe.Name,
                        Content = p?.ToString()
                    });
                }
                else if(command.Command == "ClearLogs")
                { }
                else if (command.Command == "Status")
                {
                    await NotifyStatus(pe.Name, p as ProcessStartInfo);
                }
                else
                {
                    await NotifyStatus(pe.Name, p as ProcessStartInfo);
                }
            }
        }
        else
        {
            var pe = pes.FirstOrDefault(x => x.Name == command.ProcessName);
            var p = pe.ExecuteCommand(command.Command, command.Content);

            if(command.Command == "LastLogs")
            {
                await hubContext.Clients.Client(command.ClientId).SendAsync("LastLogs", new Message
                {
                    Command = command.Command,
                    ProcessName = command.ProcessName,
                    Content = p?.ToString()
                });
            }
            else if (command.Command == "ClearLogs")
            { }
            else if (command.Command == "Status")
            {
                await NotifyStatus(pe.Name, p as ProcessStartInfo);
            }
            else
            {
                await NotifyStatus(pe.Name, p as ProcessStartInfo);
            }
        }
    }
}

public enum DaemonCommand { 
    Init = 0,
    Stop = 1,
    Restart = 2,
    Start = 3
}

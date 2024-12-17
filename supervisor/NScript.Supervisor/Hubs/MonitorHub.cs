using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NScript.Supervisor.Utils;

namespace NScript.Supervisor.Hubs;

public class MonitorHub : Hub<IMonitorClient>
{
    public MonitorHub(MessageQueueService service, IConfiguration config, IOptions<RequestLocalizationOptions> options)
    {
        this.service = service;
        this.config = config;
        this.options = options.Value;
    }
    private static int _timeout = 30000;
    private readonly MessageQueueService service;
    private readonly IConfiguration config;
    private readonly RequestLocalizationOptions options;

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        //await this.Status(null);
        LocalizationConstants.Lang = System.Globalization.CultureInfo.CurrentUICulture.Name;
    }

    public async Task ClearLogs(string processName)
    {
        CancellationTokenSource timeoutSource = new CancellationTokenSource(_timeout);
        CancellationTokenSource waitSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, Context.ConnectionAborted);
        await service.SendCommand(new Message { 
            ClientId = Context.ConnectionId,
            Command = "ClearLogs",
            Content = processName,
            ProcessName = processName,
        }, waitSource.Token);

        
    }
    public List<string> GetServices()
    {
        var gsc = ParseGuardServiceConfig.Load(config);
        return gsc.Select(x=>x.Name).ToList();
    }
    public async Task LastLogs(string processName)
    {
        CancellationTokenSource timeoutSource = new CancellationTokenSource(_timeout);
        CancellationTokenSource waitSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, Context.ConnectionAborted);
        await service.SendCommand(new Message
        {
            ClientId = Context.ConnectionId,
            Command = "LastLogs",
            Content = processName,
            ProcessName = processName,
        }, waitSource.Token);

        if (waitSource.IsCancellationRequested)
        {
            await Clients.Caller.LastLogs(new Message
            {
                Command = "ClearLog",
                ProcessName = processName,
                Content = "未知"
            });
        }

    }

    public async Task Restart(string processName)
    {
        CancellationTokenSource timeoutSource = new CancellationTokenSource(_timeout);
        CancellationTokenSource waitSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, Context.ConnectionAborted);
        var isCn = System.Globalization.CultureInfo.CurrentUICulture.Name =="zh-CN";
        await Clients.Caller.Status(new Message
        {
            Command = "Restart",
            ProcessName = processName,
            Content = "重启中",
            Status = new ProcessRunStatus
            {
                Status = isCn? "重启中" :"Restarting",                
            }
        });        
       
        await service.SendCommand(new Message
        {
            ClientId = Context.ConnectionId,
            Command = "Restart",
            Content = processName,
            ProcessName = processName,
        }, waitSource.Token);

        if (waitSource.IsCancellationRequested)
        {
            
        }
    }

    public async Task Start(string processName)
    {
        CancellationTokenSource timeoutSource = new CancellationTokenSource(_timeout);
        CancellationTokenSource waitSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, Context.ConnectionAborted);
        await service.SendCommand(new Message
        {
            ClientId = Context.ConnectionId,
            Command = "Start",
            Content = processName,
            ProcessName = processName,
        }, waitSource.Token);

    }

    public async Task Status(string processName)
    {
        CancellationTokenSource timeoutSource = new CancellationTokenSource(_timeout);
        CancellationTokenSource waitSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, Context.ConnectionAborted);
        await service.SendCommand(new Message
        {
            ClientId = Context.ConnectionId,
            Command = "Status",
            Content = processName,
            ProcessName = processName,
        }, waitSource.Token);

    }

    public async Task Stop(string processName)
    {
        CancellationTokenSource timeoutSource = new CancellationTokenSource(_timeout);
        CancellationTokenSource waitSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, Context.ConnectionAborted);
        await service.SendCommand(new Message
        {
            ClientId = Context.ConnectionId,
            Command = "Stop",
            Content = processName,
            ProcessName = processName,
        }, waitSource.Token);

    }
}
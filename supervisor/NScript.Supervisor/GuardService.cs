using Topshelf.Logging;
namespace NScript.Supervisor;

public class GuardService 
{
    private readonly LogWriter _logger;
    private readonly string[] args;
    private bool _stopRequested;
    private IHost _webHost;

    public GuardService(string[] args)
    {
        this._logger = HostLogger.Current.Get("GuardService");     
        this.args = args;
    }

    private IHost CreateBuilder()
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(builder =>
            {
                builder.SetBasePath(AppContext.BaseDirectory);
                builder.AddIniFile("services.ini", false);
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                var url = webBuilder.GetSetting("WebUrl");
                webBuilder.UseStartup<Startup>().UseUrls();
            })
            .Build();
        return builder;
    }

    public void Start() 
    {
        this._logger.Log(LoggingLevel.Info, "开启服务...");

        var builder = CreateBuilder();

        var lifeTime = builder.Services.GetRequiredService<IHostApplicationLifetime>();
        lifeTime.ApplicationStopped.Register(() =>
           {
               if (!_stopRequested)
                   Stop();
           });
        builder.Start();

        _webHost = builder;
    }

    public void Stop() {
        this._logger.Log(LoggingLevel.Info, "停止服务...");
        _stopRequested = true;
        _webHost?.Dispose();
    }

    public void Shutdown()
    {
        _stopRequested = true;
        _webHost?.Dispose();

        this._logger.Log(LoggingLevel.Warn, "计算机关机");
    }
}

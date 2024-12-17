using NScript.Supervisor.Hubs;
using NScript.Supervisor.Middleware;
using NScript.Supervisor.Utils;
using Topshelf.Logging;

namespace NScript.Supervisor;

public class Startup
{
    private readonly LogWriter _logger;
    public Startup(IWebHostEnvironment env)
    {
        this._logger = HostLogger.Current.Get("GuardService");
        HostingEnvironment = env;
    }

    public IWebHostEnvironment HostingEnvironment { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalR();
        services.AddSingleton<MessageQueueService>();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.AddSupportedUICultures(LocalizationConstants.SupportedLanguages.Select(x => x.Code).ToArray());
            options.FallBackToParentUICultures = true;
        });
        //services.AddMvc();
        services.AddRazorPages().AddViewLocalization(options=>options.ResourcesPath="Resources");
        services.AddHostedService<DaemonService>();
        services.AddScoped<RequestLocalizationCookiesMiddleware>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        env.ContentRootPath = AppContext.BaseDirectory;
        var config = app.ApplicationServices.GetService<IConfiguration>();
        bool.TryParse(config["EnableWeb"], out var bEnable);
        var url = config["Kestrel:Endpoints:Http:Url"] ?? config["Kestrel:Endpoints:Https:Url"];
        //var addresses = app.ServerFeatures.Get<IServerAddressesFeature>().Addresses;
        _logger.Info(bEnable ? $"激活web服务 {url}" : "未激活web服务");
        if (bEnable)
        {
            try
            {
                app.UseRequestLocalization();
                app.UseRequestLocalizationCookies();
                // Configure the HTTP request pipeline.
                if (!env.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                }
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthorization();
                app.UseEndpoints(endpoints =>
                {                      
                    endpoints.MapHub<MonitorHub>("/monitor");
                    endpoints.MapRazorPages();
                    endpoints.MapControllerRoute("default", "{controller=Index}/{action=Index}");                   
                });
                _logger.Info($"web服务: {string.Join(",", url)}");

            }
            catch (Exception ex)
            {
                _logger.Error($"激活web服务 {string.Join(",", url)} 失败,", ex);
            }
        }
    }
}

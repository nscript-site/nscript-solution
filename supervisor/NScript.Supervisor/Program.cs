using NScript.Supervisor;

using Microsoft.AspNetCore;

using System;
using Topshelf.Logging;
using Topshelf;
using System.Security.Principal;

if(OperatingSystem.IsWindows())
{
    var identity = WindowsIdentity.GetCurrent();
    var principal = new WindowsPrincipal(identity);

    if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
    {
        Console.WriteLine("服务安装需要使用管理员权限运行！");
        Console.WriteLine("请右键使用管理员权限重新运行！");
        Console.WriteLine("按任意键退出程序！");
        Console.ReadKey();
        Environment.Exit(exitCode: 0);
    }

    var logger = HostLogger.Current.Get("UseWindowService");
    var rc = HostFactory.Run(x =>
    {
        x.UseNLog();
        x.Service<GuardService>(s =>
        {
            s.ConstructUsing(name => new GuardService(args));
            s.WhenStarted(tc => tc.Start());
            s.WhenStopped(tc => tc.Stop());
            s.WhenShutdown(tc => tc.Shutdown());
        });
        x.RunAsLocalSystem();
        x.EnablePowerEvents();

        x.SetDescription("nsupervisor 进程监测服务");
        x.SetDisplayName("nsupervisor service");
        x.SetServiceName("nsupervisor");
    });

    var exitCode = (TypeCode)Convert.ChangeType(rc, rc.GetTypeCode());
    Environment.ExitCode = (int)exitCode;

    if (exitCode != 0)
    {
        logger.Error($"程序非正常退出： {exitCode.ToString()}");
    }
}
else
{
    new GuardService(args).Start();
}


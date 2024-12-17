using System;

namespace NScript.Supervisor;

public class GuardServiceConfig
{
    public string Directory { get; set; }
    public string Command { get; set; }
    public string Arguments { get; set; }
    public string StdErrorFile { get; set; }
    public string StdOutFile { get; set; }
    //分号隔开的环境变量  a=b;b=c
    public string Env { get; set; }
    public string Name { get; set; }

    public bool ShowUI { get; set; }

    public Dictionary<string, string> GetEnvironmentVariables()
    {
        var env = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(Env))
        {
            return env;
        }

        var list = Env.Split(";", StringSplitOptions.RemoveEmptyEntries);
        foreach(var item in list)
        {
            var vs = item.Split("=", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            env.Add(vs[0], vs[1]);
        }
        return env;
    }

    internal static GuardServiceConfig Load(IConfiguration config, string  key)
    {
        bool.TryParse(config[$"{key}:showui"],out var showUI);
        return new GuardServiceConfig
        {
            Name = key.Split(":")[1],
            Arguments = config[$"{key}:arguments"],
            Command = config[$"{key}:command"],
            Directory = config[$"{key}:directory"],
            StdErrorFile = config[$"{key}:stderr_logfile"],
            StdOutFile = config[$"{key}:stdout_logfile"],
            Env = config[$"{key}:env"],
            ShowUI = showUI,

        };
    }
}

public static class ParseGuardServiceConfig
{
    public static List<GuardServiceConfig> Load(IConfiguration config)
    {
        List<GuardServiceConfig> gsc = new List<GuardServiceConfig>();
        var programs = config.AsEnumerable().Where(x => x.Key.StartsWith("program:", StringComparison.OrdinalIgnoreCase) && x.Key.Split(":").Length == 2);
        foreach (var c in programs)
        {
            var wsc = GuardServiceConfig.Load(config, c.Key);
            wsc.Name = c.Key.Split(":")[1];
            if (!string.IsNullOrEmpty(wsc.Name))
            {
                gsc.Add(wsc);
            }
        }
        return gsc;
    }
}

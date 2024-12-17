using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using NScript.Supervisor.Data;
using Topshelf.Logging;

namespace NScript.Supervisor.Utils;

public class ProcessExecutor : IDisposable
{
    public class ProcessInfo
    {
        public int Id { get; private set; } = -1;
        
        public string ConfigName { get; private set; }

        public long StartTimeTicks { get; private set; }

        public ProcessInfo(string configName)
        {
            ConfigName = configName;
            LoadFromHistory();
        }

        private void LoadFromHistory()
        {
            var lastData = ProcessLastStartDataService.Find(ConfigName);
            if (lastData != null)
            {
                Id = lastData.ProcessId;
                StartTimeTicks = lastData.StartTimeTicks;
            }
        }

        public void Save(Process p)
        {
            if (p == null) return;

            Id = p.Id;
            StartTimeTicks = p.StartTime.Ticks;
            ProcessLastStartDataService.Update(ConfigName, Id, StartTimeTicks);
        }

        public Process? GetProcess()
        {
            if (Id < 0) return null;

            try
            {
                var p = Process.GetProcessById(Id);
                if (Match(p))
                    return p;
                else
                    return null;
            }
            catch(Exception ex)
            {
            }

            return null;
        }

        public bool Match(Process? process)
        {
            if(process == null) return false;

            bool match = false;
            try
            {
                match = process.Id == Id && process.StartTime.Ticks == StartTimeTicks;
            }
            catch(Exception ex)
            {
            }
            return match;
        }
    }

    private readonly GuardServiceConfig config;

    private ProcessInfo CurrentProcessInfo;

    private FileStream stdoutStream;
    private FileStream stderrorStream;
    private readonly LogWriter _logger;
    private bool _isManualStop = false;

    public ProcessExecutor(GuardServiceConfig config)
    {
        _logger = HostLogger.Current.Get("ProcessExecutor");
        this.config = config;

        CurrentProcessInfo = new ProcessInfo(config.Name);

        var p = CurrentProcessInfo.GetProcess();
        if(p!=null)
        {
            Console.WriteLine($"[{config.Name}] 相关进程已存在，无需重新启动");
        }

        var redirectStdOut = !string.IsNullOrEmpty(this.config.StdOutFile);
        var redirectStdErr = !string.IsNullOrEmpty(this.config.StdErrorFile);

        if (redirectStdOut)
        {
            try
            {
                stdoutStream = new FileStream(this.config.StdOutFile, FileMode.OpenOrCreate | FileMode.Append,
                FileAccess.Write, FileShare.ReadWrite);
            }
            catch
            {
                _logger.Warn($"打开文件 {this.config.StdOutFile} 失败，禁用输出日志");
                stdoutStream = null;
            }

        }
        if (redirectStdErr)
        {
            try
            {
                stderrorStream = new FileStream(this.config.StdErrorFile, FileMode.OpenOrCreate | FileMode.Append,
                FileAccess.Write, FileShare.ReadWrite);
            }
            catch
            {
                _logger.Warn($"打开文件 {this.config.StdErrorFile} 失败，禁用错误输出日志");
                stderrorStream = null;
            }
        }
    }

    public string Name => this.config.Name;
    /// <summary>
    /// 定时检查执行
    /// </summary>
    /// <returns></returns>
    public virtual ProcessStartInfo Execute()
    {
        if (_isManualStop) return null;

        var bDir = !string.IsNullOrEmpty(this.config.Directory);

        var p = CurrentProcessInfo.GetProcess();

        if (p == null)
            return StartProcess();
        else 
            return new ProcessStartInfo(p);
    }

    private ProcessStartInfo StopProcess()
    {
        Process p = CurrentProcessInfo.GetProcess();
        if (p == null)
        {
            _logger.Info($" {this.config.Name}已经关闭.");
        }
        else 
        {
            try
            {
                _logger.Info($"关闭程序 {this.config.Name} ...");
                // kill & restart
                p.Kill(true);                
                _logger.Info($"关闭程序 {this.config.Name} 成功");
            }
            catch (Exception ex)
            {
                _logger.Error($"关闭程序 {this.config.Name} 时失败! ", ex);
            }
        }           
        return new ProcessStartInfo(p);
    }

    private ProcessStartInfo RestartProcess()
    {
        ProcessStartInfo p = null;
        try
        {
            p = this.StopProcess();
            p = this.StartProcess();                
        }
        catch (Exception ex)
        {
            _logger.Error($"执行启动 {this.config.Name} 时失败! ", ex);
        }           
        return p;
    }

    private ProcessStartInfo StartProcess()
    {
        /***********************
         * 启动进程时，在进程的环境变量里添加 NSUPERVISOR_CONFIG_NAME: {config.Name}，这样，当 supervisor 重启后，也能找到之前创建的进程
         **********************/

        _logger.Info($"开始程序 {this.config.Name}...");
        var bDir = !string.IsNullOrEmpty(this.config.Directory);
        Process p = CurrentProcessInfo.GetProcess();
        if (p == null)
        {
            var startInfo =  new System.Diagnostics.ProcessStartInfo
            {
                FileName = this.config.Command,
                UseShellExecute = false,
                RedirectStandardOutput = stdoutStream != null,
                RedirectStandardError = stderrorStream != null,
                WorkingDirectory = bDir ? this.config.Directory : AppDomain.CurrentDomain.BaseDirectory,
                Arguments = this.config.Arguments,
                CreateNoWindow = true,   
                WindowStyle = ProcessWindowStyle.Hidden,
                //StandardOutputEncoding = Encoding.UTF8,
                //StandardErrorEncoding = Encoding.UTF8,
            };

            foreach (var (key, value) in this.config.GetEnvironmentVariables())
            {
                if (value is not null)
                {
                    startInfo.Environment[key] = value;
                }
                else
                {
                    //https://github.com/dotnet/runtime/blob/212fb547303cc9c46c5e0195f530793c30b67669/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/Process.Windows.cs
                    // Null value means we should remove the variable
                    // https://github.com/Tyrrrz/CliWrap/issues/109
                    // https://github.com/dotnet/runtime/issues/34446
                    startInfo.Environment.Remove(key);
                }
            }
            Console.WriteLine($"start {config.Name}  ....");
            p = new Process
            {
                StartInfo =startInfo,
            };                
            
            p.ErrorDataReceived += P_ErrorDataReceived;
            p.OutputDataReceived += P_OutputDataReceived;

            try
            {
                if (!p.Start())
                {
                    throw new InvalidOperationException(
                        $"Failed to start a process with file path '{p.StartInfo.FileName}'. " +
                        "Target file is not an executable or lacks execute permissions."
                    );
                }

                if (stdoutStream != null) p.BeginOutputReadLine();
                if (stderrorStream != null) p.BeginErrorReadLine();
                _logger.Info($"程序 {this.config.Name} 启动成功.");
            }
            catch (Win32Exception ex)
            {
                throw new Win32Exception(
                    $"Failed to start a process with file path '{p.StartInfo.FileName}'. " +
                    "Target file or working directory doesn't exist, or the provided credentials are invalid.",
                    ex
                );
            }
        }

        Console.WriteLine($"[{DateTime.Now}][{config.Name}] 启动成功" );

        CurrentProcessInfo.Save(p);

        return new ProcessStartInfo(p) ;
    }
    private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e?.Data != null)
        {
            try
            {
                stdoutStream.Write(Encoding.UTF8.GetBytes(e.Data));
                stdoutStream.Write(Encoding.UTF8.GetBytes("\r\n"));
                stdoutStream.Flush();
            }
            catch {
                _logger.Warn($"程序 {this.config.Name} 输出日志失败! ");
            }
        }
    }

    private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e?.Data != null)
        {
            try
            {
                stderrorStream.Write(Encoding.UTF8.GetBytes(e.Data));
                stdoutStream.Write(Encoding.UTF8.GetBytes("\r\n"));
                stdoutStream.Flush();
            }
            catch { _logger.Warn($"程序 {this.config.Name} 输出错误日志失败! "); }
        }
    }

    public void Dispose()
    {
        if (stdoutStream != null)
        {
            stdoutStream.Close();
            stdoutStream.Dispose();
            _logger.Warn($"程序 {this.config.Name} 输出日志关闭! ");
        }
        if (stderrorStream != null)
        {
            stderrorStream.Close();
            stderrorStream.Dispose();
            _logger.Warn($"程序 {this.config.Name} 输出错误日志关闭! ");
        }
    }

    public object ExecuteCommand(string command, string content)
    {
        var redirectStdOut = !string.IsNullOrEmpty(this.config.StdOutFile);
        var redirectStdErr = !string.IsNullOrEmpty(this.config.StdErrorFile);
        ProcessStartInfo p = null;
        if(command == "Start")
        {
            _isManualStop = false;
            p = this.StartProcess();
        }
        else if (command == "Stop")
        {
            _isManualStop = true;
            p = this.StopProcess();
        }
        else if (command == "Restart")
        {
            _isManualStop = false;
            p = this.RestartProcess();
        }
        else if(command == "LastLogs")
        {
            if (redirectStdOut)
            {
                try
                {         
                    byte[] buffer = new byte[8192];
                    using (var stream = new FileStream(this.config.StdOutFile, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite))
                    {
                        long len = stream.Length;
                        if (len >= 8192)
                        {
                            stream.Seek(len - 8192, SeekOrigin.Begin);
                            stream.Read(buffer);
                            var log = Encoding.UTF8.GetString(buffer);
                            return log;
                        }
                        else if(len == 0)
                        {
                            return "";
                        }
                        else
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.Read(buffer);
                            var log = Encoding.UTF8.GetString(buffer,0,(int)len);
                            return log;
                        }
                        
                    }
                    
                }
                catch
                {
                    _logger.Warn($"打开文件 {this.config.StdOutFile} 失败");                        
                }
                return null;
            }
        }
        else if(command == "ClearLogs")
        {
            if (redirectStdOut)
            {
                try
                {
                    using (var stream = new FileStream(this.config.StdOutFile,  FileMode.Truncate,
                        FileAccess.Write, FileShare.ReadWrite))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);
                        stream.Flush();
                    }
                    stdoutStream.Close();
                    stdoutStream = new FileStream(this.config.StdOutFile, FileMode.OpenOrCreate | FileMode.Append,
                FileAccess.Write, FileShare.ReadWrite);
                    _logger.Warn($"清空文件 {this.config.StdOutFile} 内容");
                }
                catch
                {
                    _logger.Warn($"清空文件 {this.config.StdOutFile} 失败");
                    
                }

            }
            if (redirectStdErr)
            {
                try
                {                        
                    using (var stream = new FileStream(this.config.StdErrorFile, FileMode.Truncate,
                        FileAccess.Write, FileShare.ReadWrite))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.SetLength(0);
                        stream.Flush();
                    }
                    stderrorStream.Close();
                    stderrorStream = new FileStream(this.config.StdErrorFile, FileMode.OpenOrCreate | FileMode.Append,
                FileAccess.Write, FileShare.ReadWrite);
                    
                    _logger.Warn($"清空文件 {this.config.StdErrorFile} 内容");
                }
                catch
                {
                    _logger.Warn($"清空文件 {this.config.StdErrorFile} 失败");

                }
            }
        }
        else if(command == "Status")
        {
            p = new ProcessStartInfo(CurrentProcessInfo.GetProcess());
        }
        return p;
    }
}

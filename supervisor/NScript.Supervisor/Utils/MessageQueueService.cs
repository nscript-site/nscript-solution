using System.Threading.Channels;

namespace NScript.Supervisor.Utils;

/// <summary>
/// signalR 和服务通信
/// </summary>
public class MessageQueueService : IDisposable
{
    private Channel<Message> _channelR2S;

    public ChannelReader<Message> Reader => _channelR2S.Reader;
    public MessageQueueService()
    {
        var options = new BoundedChannelOptions(10)
        {
            AllowSynchronousContinuations = true,   
        };
        _channelR2S = Channel.CreateBounded<Message>(options) ;
    }

    /// <summary>
    /// 从 hub 发送命令，不需要等待完成
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public ValueTask SendCommandNoReturn(Message msg,CancellationToken token)
    {
        return _channelR2S.Writer.WriteAsync(msg, token);
    }
    /// <summary>
    /// 从hub发送命令
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async ValueTask SendCommand(Message msg, CancellationToken token)
    {
        await _channelR2S.Writer.WriteAsync(msg, token);        

        
    }

    public void Dispose()
    {
        _channelR2S.Writer.Complete();
    }
}

public class Message
{
    public string Command { get; set; }

    public string ProcessName { get; set; }
    public string Content { get; set; }  
    
    public string ClientId { get; set; }

    public ProcessRunStatus Status { get; set; }
}

public class ProcessRunStatus
{
    public string Status { get; set; }
    public string Pid { get; set; }
    public string UpTime { get; set; }
}

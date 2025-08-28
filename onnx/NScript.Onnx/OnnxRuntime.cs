using System.Text;
using Microsoft.ML.OnnxRuntime;

namespace NScript.Onnx;

internal enum OnnxProvider
{
    Cpu,
    DirectML,
    OpenVino,
    CoreML,
    Cuda
}

public enum OnnxRuntimeMode
{
    Adaptive = 0,
    Cpu = 1,
}

public class OnnxRuntime
{
    private OrtLoggingLevel _Loglevel;

    public Action<SessionOptions>? OnSessionOptionCreate;

    public OnnxRuntime WithSessionOptions(Action<SessionOptions> onSessionOptions)
    {
        this.OnSessionOptionCreate = onSessionOptions;
        return this;
    }

    public OnnxRuntime(OnnxRuntimeMode mode = OnnxRuntimeMode.Adaptive, OrtLoggingLevel logLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR)
    {
        this._Loglevel = logLevel;
        this.availableProviders = OrtEnv.Instance().GetAvailableProviders();
        if (availableProviders == null || availableProviders.Length == 0)
        {
            Console.WriteLine("[OnnxRuntime] No onnx provider");
        }
        else
        {
            if(mode == OnnxRuntimeMode.Adaptive)
            {
                foreach (var p in availableProviders)
                    Console.WriteLine($"[OnnxRuntime] Find Provider: {p}");

                String usingProvider = String.Empty;
                foreach (var p in availableProviders)
                {
                    usingProvider = p;
                    if (p.StartsWith("OPENVINO"))
                    {
                        Provider = OnnxProvider.OpenVino;
                        break;
                    }
                    else if (p.StartsWith("DML") || p.StartsWith("Dml"))
                    {
                        Provider = OnnxProvider.DirectML;
                        break;
                    }
                    else if (p.StartsWith("CoreML"))
                    {
                        Provider = OnnxProvider.CoreML;
                        break;
                    }
                }
                Console.WriteLine($"[OnnxRuntime] Using {usingProvider}");
            }
            else
            {
                Provider = OnnxProvider.Cpu;
                Console.WriteLine($"[OnnxRuntime] Using cpu");
            }
        }
    }

    public string GetOnnxProviders()
    {
        if (availableProviders == null || availableProviders.Length == 0) return "None";
        StringBuilder builder = new StringBuilder();
        foreach (var p in availableProviders)
        {
            if (builder.Length > 0) builder.Append(';');

            builder.Append(p);
        }
        return builder.ToString();
    }

    public SessionOptions CreateSessionOptions()
    {
        SessionOptions op = new SessionOptions() { LogSeverityLevel = this._Loglevel };

        var provider = this.Provider;

        if (provider == OnnxProvider.DirectML)
        {
            op.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
            //op.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
            //op.EnableMemoryPattern = false;
            int dmlDeviceId = 0;
            op.AppendExecutionProvider_DML(dmlDeviceId);
            Console.WriteLine($"[OnnxRuntime] Use DirectMl Device {dmlDeviceId}, InterOpNumThreads - {op.InterOpNumThreads}, IntraOpNumThreads - {op.IntraOpNumThreads}");
        }
        else if (provider == OnnxProvider.OpenVino)
        {
            String openvinoDevice = "0";
            op.AppendExecutionProvider_OpenVINO(openvinoDevice);
            Console.WriteLine($"Use OpenVino Device {openvinoDevice}, InterOpNumThreads - {op.InterOpNumThreads}, IntraOpNumThreads - {op.IntraOpNumThreads}");
        }
        else if (provider == OnnxProvider.CoreML)
        {
            op.AppendExecutionProvider_CoreML(CoreMLFlags.COREML_FLAG_ONLY_ENABLE_DEVICE_WITH_ANE);
            Console.WriteLine($"Use CoreML Device, InterOpNumThreads - {op.InterOpNumThreads}, IntraOpNumThreads - {op.IntraOpNumThreads}");
        }
        else
        {
            op.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
        }

        if (OnSessionOptionCreate != null) OnSessionOptionCreate(op);

        return op;
    }

    private string[] availableProviders { get; set; }

    internal OnnxProvider Provider { get; set; } = OnnxProvider.Cpu;

    public OnnxModel CreateModel(string modelPath, int numThread = 2)
    {
        var model = new OnnxModel() { Runtime = this };
        model.InitModel(modelPath, numThread);
        return model;
    }

    public OnnxModel CreateModel(byte[] modelData, int numThread = 2)
    {
        var model = new OnnxModel() { Runtime = this };
        model.InitModel(modelData, numThread);
        return model;
    }

}

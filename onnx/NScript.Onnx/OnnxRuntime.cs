using System.Text;
using Microsoft.ML.OnnxRuntime;

namespace NScript.Onnx;

internal enum OnnxProvider
{
    Cpu,
    DirectML,
    OpenVino,
    CoreML
}

public class OnnxRuntime
{
    public OnnxRuntime()
    {
        this.availableProviders = OrtEnv.Instance().GetAvailableProviders();
        if (availableProviders == null || availableProviders.Length == 0)
        {
            Console.WriteLine("[OnnxRuntime] No onnx provider");
        }
        else
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
                else if (p.StartsWith("DML"))
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

    private string[] availableProviders { get; set; }

    internal OnnxProvider Provider { get; set; } = OnnxProvider.Cpu;

    public OnnxModel CreateModel(string modelPath, int numThread = 2)
    {
        var model = new OnnxModel() { Runtime = this };
        model.InitModel(modelPath, numThread);
        return model;
    }
}

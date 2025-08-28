using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace NScript.Onnx;

public class OnnxModel : IDisposable
{
    private InferenceSession model;
    private List<string> inputNames;

    internal OnnxRuntime Runtime { get; set; }
    
    public IReadOnlyList<string> InputNames { get; private set; }
    public IReadOnlyList<string> OutputNames { get; private set; }

    public IReadOnlyDictionary<string, NodeMetadata> Inputs { get; private set; }
    public IReadOnlyDictionary<string, NodeMetadata> Outputs { get; private set; }

    public string FirstInputName { get; private set; } = String.Empty;
    public string FirstOutputName { get; private set; } = String.Empty;

    internal void InitModel(string modelPath, int numThread)
    {
        model = CreateModel(Runtime, modelPath, numThread);
        LoadModel(numThread);
    }

    internal void InitModel(byte[] modelData, int numThread)
    {
        model = CreateModel(Runtime, modelData, numThread);
        LoadModel(numThread);
    }

    private void LoadModel(int numThread)
    {
        try
        {
            InputNames = model.InputNames;
            OutputNames = model.OutputNames;
            Inputs = model.InputMetadata;
            Outputs = model.OutputMetadata;

            if (InputNames.Count > 0) FirstInputName = InputNames[0];
            if (OutputNames.Count > 0) FirstOutputName = OutputNames[0];
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
            if (ex.InnerException != null)
            {
                Console.WriteLine(ex.InnerException.Message);
                Console.WriteLine(ex.InnerException.StackTrace);
            }
            throw;
        }
    }

    public void Infer(IReadOnlyCollection<NamedOnnxValue> inputs, Action<DisposableNamedOnnxValue[]?> onResults)
    {
        try
        {
            using (IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = model.Run(inputs))
            {
                var resultsArray = results.ToArray();
                onResults(resultsArray);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message + ex.StackTrace);
        }
    }

    public IReadOnlyCollection<NamedOnnxValue> CreateInputs(float[] feature, ReadOnlySpan<int> shape, string? name = null)
    {
        if (name == null) name = FirstInputName;
        Tensor<float> inputTensors = new DenseTensor<float>(feature, shape);
        return new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(name, inputTensors) };
    }

    public void Dispose()
    {
        model?.Dispose();
        model = null;
    }

    public static InferenceSession CreateModel(OnnxRuntime runtime, string modelPath, int numThread)
    {
        InferenceSession model = null;

        var (encrypted, path) = GetFileInfo(modelPath);
        Console.WriteLine("[OnnxRuntime] Load Model: " + path);

        model = new InferenceSession(path, runtime.CreateSessionOptions());
        return model;
    }

    public static InferenceSession CreateModel(OnnxRuntime runtime, byte[] modelBytes, int numThread)
    {
        Console.WriteLine($"[OnnxRuntime] Load Model from buff: {modelBytes.Length} bytes");
        var model = new InferenceSession(modelBytes, runtime.CreateSessionOptions());
        return model;
    }

    private static (bool, string) GetFileInfo(String modelPath)
    {
        if (modelPath.EndsWith(".onnx"))
            return (false, modelPath);
        else if (modelPath.EndsWith(".onnxe"))
            return (true, modelPath);
        else
        {
            String onnxeInt8Path = Path.Combine(modelPath, "inference.int8.onnxe");
            String onnxePath = Path.Combine(modelPath, "inference.onnxe");
            String onnxPath = Path.Combine(modelPath, "inference.onnx");
            if (File.Exists(onnxeInt8Path))
                return (true, onnxeInt8Path);
            else if (File.Exists(onnxePath))
                return (true, onnxePath);
            else
                return (false, onnxPath);
        }
    }
}

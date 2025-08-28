// See https://aka.ms/new-console-template for more information
using NScript.Onnx;

void LoadModel(string modelPath, bool loadBytes)
{
    try
    {
        var runtime = new OnnxRuntime(OnnxRuntimeMode.Cpu);
        OnnxModel? model = null;

        if (loadBytes)
        {
            byte[] modelData = System.IO.File.ReadAllBytes(modelPath);
            model = runtime.CreateModel(modelData, 2);
        }
        else
        {
            model = runtime.CreateModel(modelPath, 2);
        }

        if (model == null) Console.WriteLine("Model is null");
        else Console.WriteLine($"Model loaded: {modelPath}, Inputs: {string.Join(", ", model.InputNames)}, Outputs: {string.Join(", ", model.OutputNames)}");
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        if (ex.InnerException != null)
        {
            Console.WriteLine(ex.InnerException.Message);
            Console.WriteLine(ex.InnerException.StackTrace);
        }
    }
}

LoadModel("D:\\models\\rivers_ai\\vit-b-16.img.fp32.onnx", false);
LoadModel("D:\\models\\rivers_ai\\vit-b-16.img.fp32.onnx", true);
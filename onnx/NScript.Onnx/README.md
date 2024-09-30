# NScript.Onnx

这是一个简单的 Onnx helper 库，可以简化相关的开发。

以 campplus 声纹识别模型，示例使用方法：

```csharp
public class CampPlusModule
{
    private OnnxRuntime _runtime;
    private OnnxModel _model;
    public CampPlusModule(OnnxRuntime runtime)
    {
        _runtime = runtime;
        _model = runtime.CreateModel("path_of_campplus.onnx");
    }

    public float[] GetFeature(float[] fbankFeature)
    {
        float[]? val = null;
        Tensor<float> inputTensors = new DenseTensor<float>(fbankFeature, new[] { 1, fbankFeature.Length/80, 80 });
        var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(_model.FirstInputName, inputTensors) };
        _model.Infer(inputs, results =>
        {
            val = results[0].AsEnumerable<float>().ToArray();
        });
        return val!;
    }
}

...

var runtime = new OnnxRuntime();
var camplusModule = new CampPlusModule(runtime);
var fbankFeature = new float[500 * 80];  // 模拟的 fbank 特征
var camplusFeature = camplusModule.GetFeature(fbankFeature);

```
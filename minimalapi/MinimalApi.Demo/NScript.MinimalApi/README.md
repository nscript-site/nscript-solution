# NScript.MinimalApi

这是一个简单的 Minimal Api 服务开发库，可以简化 Api 服务的开发。

库中，提供了 BaseWebApi 基类，继承自该基类的 Api，所提供的路由，实现了 Aop 的基于 tokens 的鉴权服务。可通过配置文件来配置token项目，例如：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  ...
  "Auth": {
    "Enable": true,
    "Tokens": [
      "test@111"
    ]
  }
}

```

token 建议设置为 userid@xxxx 的格式，方便辨别 token 所属用户。默认 token 通过 Header 来传递，key 为 "ApiToken"，可通过修改 AuthConfig.ApiTokenKey 值来修改 key。

本库的使用示例如下：

```csharp
using Microsoft.OpenApi.Models;
using MinimalApi.Demo.Api;
using NScript.MinimalApi;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // 修改上传文件大小
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 1024;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Minimal Api Demo",
        Description = "这是一个 Minimal Api 示范项目，这里填写项目的简介"
    });

    AuthConfig.Build(c);

    // 包含 xml 文档，方便 swagger ui 显示相关的注释
    var filePath = Path.Combine(AppContext.BaseDirectory, "MinimalApi.Demo.xml");
    c.IncludeXmlComments(filePath);
});

var app = builder.Build();

AuthConfig.Build(builder.Configuration);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 所提供的API放这里
new UploadFileApi().Map(app);

app.Run();
```

其中，核心是下面的代码：
```csharp
    AuthConfig.Build(c); // 开启 swagger ui 中的token输入功能
    ...
    AuthConfig.Build(builder.Configuration); // 读取配置文件中tokens配置
```

新增 api 示例如下：

```
using Microsoft.AspNetCore.Mvc;
using NScript.MinimalApi;

namespace MinimalApi.Demo.Api;

public class UploadFileApi:BaseWebApi
{
    protected override void Map()
    {
        this.MapPost("/uploadfile", ([FromForm] UploadFileForm form, [FromQuery] string username) =>
        {
            return $"Action: {form.Action}, VideoFile: {form.VideoFile.Name}, AudioFile: {form.AudioFile.Name}";
        }).WithOpenApi(op =>
        {
            op.Summary = "UploadFile 示范";
            op.Description = "本接口演示了怎么撰写 UploadFile Api";
            op.WithParameter("username", p =>
            {
                p.Description = "用户名称";
            });
            return op;
        }).WithTags("UploadFile").DisableAntiforgery();
    }

    public class UploadFileForm
    {
        /// <summary>
        /// 需要进行的操作指令
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 视频文件
        /// </summary>
        public IFormFile VideoFile { get; set; }
        /// <summary>
        /// 音频文件
        /// </summary>
        public IFormFile AudioFile { get; set; }
    }
}

```
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
new HelloWorldApi().Map(app);
new UploadFileApi().Map(app);

app.Run();


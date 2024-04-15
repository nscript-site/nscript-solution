using Microsoft.OpenApi.Models;
using MinimalApi.Demo.Api;
using NScript.MinimalApi;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    // �޸��ϴ��ļ���С
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 1024;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Minimal Api Demo",
        Description = "����һ�� Minimal Api ʾ����Ŀ��������д��Ŀ�ļ��"
    });

    AuthConfig.Build(c);

    // ���� xml �ĵ������� swagger ui ��ʾ��ص�ע��
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

// ���ṩ��API������
new HelloWorldApi().Map(app);
new UploadFileApi().Map(app);

app.Run();


using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
namespace NScript.MinimalApi;

public class BaseWebApi
{
    private WebApplication _app;
    public void Map(WebApplication app)
    {
        this._app = app;
        this.Map();
    }

    protected RouteHandlerBuilder MapGet(string pattern, Delegate handler)
    {
        return _app.MapGet(pattern, handler).AddEndpointFilter(Filter);
    }

    protected RouteHandlerBuilder MapPost(string pattern, Delegate handler)
    {
        return _app.MapPost(pattern, handler).AddEndpointFilter(Filter);
    }

    protected RouteHandlerBuilder MapDelete(string pattern, Delegate handler)
    {
        return _app.MapDelete(pattern, handler).AddEndpointFilter(Filter);
    }

    protected RouteHandlerBuilder MapPut(string pattern, Delegate handler)
    {
        return _app.MapPut(pattern, handler).AddEndpointFilter(Filter);
    }

    protected RouteHandlerBuilder Map(string pattern, Delegate handler)
    {
        return _app.Map(pattern, handler).AddEndpointFilter(Filter);
    }

    protected async ValueTask<object?> Filter(EndpointFilterInvocationContext efiContext, EndpointFilterDelegate next)
    {
        if (efiContext.HttpContext.Request.CheckAuth(AuthConfig.ApiTokenKey) == false)
            return Results.Unauthorized();

        return await next(efiContext);
    }

    protected virtual void Map()
    {
    }
}

public static class MinimalApiClassHelper
{
    public static OpenApiOperation WithParameter(this OpenApiOperation op, string paramName, Action<OpenApiParameter> action)
    {
        var find = op.Parameters.FirstOrDefault(x => x.Name == paramName);
        if (find != null && action != null) action(find);
        return op;
    }

    public static String GetAuth(this HttpRequest request, string paramName = "ApiToken")
    {
        return request.Headers["ApiToken"].FirstOrDefault(String.Empty)!;
    }

    public static bool CheckAuth(this HttpRequest request, string paramName = "ApiToken")
    {
        var auth = request.GetAuth(paramName);
        return AuthConfig.MatchAuthToken(auth);
    }
}

public class AuthConfig
{
    public bool Enable { get; set; }
    public List<String> Tokens { get; set; }

    /// <summary>
    /// header 中的token的key，默认为为 "ApiToken"，若需要制定其它的 key，请修改此项
    /// </summary>
    public static string ApiTokenKey { get; set; } = "ApiToken";

    protected static AuthConfig? Instance { get; set; } = null;

    /// <summary>
    /// 从配置文件中构建授权策略。
    /// </summary>
    /// <remarks>
    /// 配置文件中需要增加 Auth 节，示例如下：
    ///   "Auth": {
    ///       "Enable": true,
    ///       "Tokens": [
    ///         "test@123456"
    ///       ]
    ///   }
    /// </remarks>
    /// <param name="cfg"></param>
    public static void Build(ConfigurationManager cfg)
    {
        var cfgAuth = cfg.GetSection("Auth");
        Instance = cfgAuth.Get<AuthConfig>();

        if (Instance != null)
        {
            Console.WriteLine($"[Auth] Enable: {Instance.Enable}");
            if (Instance.Tokens != null)
            {
                int num = 0;
                foreach (var token in Instance.Tokens)
                {
                    num++;
                    Console.WriteLine($"[Auth] Token {num}/{Instance.Tokens.Count}: {token}");
                }
            }
            if (AuthConfig.NeedAuth() == false) Console.WriteLine("您的 Api 任何人都可以访问。若需设置授权访问，请在配置文件中将 Auth:Enable 设置为 true，并在 Auth:Tokens 中添加响应的 token");
        }
    }

    /// <summary>
    /// 生成 Swagger UI 鉴权功能
    /// </summary>
    /// <param name="c"></param>
    public static void Build(SwaggerGenOptions c)
    {
        if (AuthConfig.NeedAuth())
        {
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "请输入token",
                Name = ApiTokenKey,
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            {
                new OpenApiSecurityScheme{
                    Reference =new OpenApiReference{
                        Type = ReferenceType.SecurityScheme,
                        Id ="Bearer"
                    }
                },new string[]{ }
            }
        });
        }
    }

    public static bool NeedAuth()
    {
        return Instance != null && Instance.Enable == true;
    }

    public static bool MatchAuthToken(string token)
    {
        if (NeedAuth() == false) return true;
        else if (Instance.Tokens == null) return false;

        return Instance.Tokens.Contains(token);
    }
}
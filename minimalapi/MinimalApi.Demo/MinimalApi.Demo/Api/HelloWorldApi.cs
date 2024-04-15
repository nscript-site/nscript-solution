
using NScript.MinimalApi;

namespace MinimalApi.Demo.Api;

public class HelloWorldApi:BaseWebApi
{
    protected override void Map()
    {
        this.MapGet("/helloworld", ()=>"Hello World!").WithOpenApi(op=>
        {
            op.Summary = "Get 示范";
            op.Description = "本接口演示了怎么撰写 Get Api";
            return op;
        }).WithTags("HelloWorld");

        this.MapGet("/hello/{name}", (string name) => $"Hello, {name}").WithOpenApi(op =>
        {
            op.Summary = "路由绑定 示范";
            op.Description = "本接口演示了怎么路由绑定参数";
            return op;
        }).WithTags("HelloWorld");

        this.MapPost("helloworld", (Person person) => new EchoResult<Person>(person)).WithOpenApi(op =>
        {
            op.Summary = "Post 示范";
            op.Description = "本接口演示了怎么撰写 Post Api，通过 json 传值，返回 json 结果";
            return op;
        }).WithTags("HelloWorld");
    }

    public class EchoResult<T>
    {
        public EchoResult(T result) { data = result; }

        /// <summary>
        /// 输入内容
        /// </summary>
        public T? data { get; set; }

        /// <summary>
        /// 当前时间
        /// </summary>
        public DateTime time { get; set; } = DateTime.Now;
    }

    public class Person
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// 年龄
        /// </summary>
        public int age { get; set; }
    }
}

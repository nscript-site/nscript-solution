
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

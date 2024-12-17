namespace NScript.Supervisor.Utils;

public static class LocalizationConstants
{
    public static string Lang { get; set; } = "en-US";
    public const string ResourcesPath = "Resources";
    public static readonly LanguageCode[] SupportedLanguages = {
            new LanguageCode
            {
                Code = "en-US",
                DisplayName= "English"
            },           
            new LanguageCode
            {
                Code = "zh-CN",
                DisplayName = "中文"
            }
        };
    public class LanguageCode
    {
        public string DisplayName { get; set; }
        public string Code { get; set; }
    }
}

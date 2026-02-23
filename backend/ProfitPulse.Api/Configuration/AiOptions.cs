namespace ProfitPulse.Api.Configuration;

public class AiOptions
{
    public const string Section = "AI";
    public string ClaudeApiKey { get; set; } = string.Empty;
    public string ClaudeModel { get; set; } = "claude-3-haiku-20240307";
    public int MaxTokens { get; set; } = 2048;
}

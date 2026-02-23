namespace ProfitPulse.Api.Configuration;

public class AuthOptions
{
    public const string Section = "Auth";
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ProfitPulse";
    public string Audience { get; set; } = "ProfitPulse";
    public string FrontendUrl { get; set; } = "http://localhost:3000";
    public int TokenExpiryHours { get; set; } = 24;
}

namespace ProfitPulse.Api.Services.AI;

public interface IAiProvider
{
    string ProviderName { get; }
    Task<string> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken ct = default);
}

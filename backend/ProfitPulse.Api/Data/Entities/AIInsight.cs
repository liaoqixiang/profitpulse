namespace ProfitPulse.Api.Data.Entities;

public class AIInsight
{
    public Guid Id { get; set; }
    public Guid CafeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string DetailedAnalysis { get; set; } = string.Empty;
    public string? RecommendedAction { get; set; }
    public InsightCategory Category { get; set; }
    public InsightPriority Priority { get; set; }
    public InsightStatus Status { get; set; } = InsightStatus.New;
    public decimal? PotentialImpact { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cafe Cafe { get; set; } = null!;
}

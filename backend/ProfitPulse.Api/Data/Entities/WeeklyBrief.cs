namespace ProfitPulse.Api.Data.Entities;

public class WeeklyBrief
{
    public Guid Id { get; set; }
    public Guid CafeId { get; set; }
    public DateOnly WeekStarting { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Highlights { get; set; } = string.Empty;
    public string Concerns { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cafe Cafe { get; set; } = null!;
}

namespace ProfitPulse.Api.Data.Entities;

public class Cafe
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal GstRate { get; set; } = 0.15m;
    public decimal TargetFoodCostPercent { get; set; } = 35m;
    public decimal TargetLabourCostPercent { get; set; } = 30m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DemoUser> Users { get; set; } = [];
    public ICollection<MenuItem> MenuItems { get; set; } = [];
    public ICollection<StaffMember> Staff { get; set; } = [];
    public ICollection<DailySummary> DailySummaries { get; set; } = [];
    public ICollection<AIInsight> Insights { get; set; } = [];
    public ICollection<WeeklyBrief> WeeklyBriefs { get; set; } = [];
}

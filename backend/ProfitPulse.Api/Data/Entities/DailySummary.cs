namespace ProfitPulse.Api.Data.Entities;

public class DailySummary
{
    public Guid Id { get; set; }
    public Guid CafeId { get; set; }
    public DateOnly Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal FoodCost { get; set; }
    public decimal LabourCost { get; set; }
    public decimal OtherCosts { get; set; }
    public int CustomerCount { get; set; }
    public int TransactionCount { get; set; }

    public Cafe Cafe { get; set; } = null!;

    public decimal GrossProfit => TotalRevenue - FoodCost;
    public decimal NetProfit => TotalRevenue - FoodCost - LabourCost - OtherCosts;
    public decimal FoodCostPercent => TotalRevenue > 0 ? FoodCost / TotalRevenue * 100 : 0;
    public decimal LabourCostPercent => TotalRevenue > 0 ? LabourCost / TotalRevenue * 100 : 0;
}

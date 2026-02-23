using Microsoft.EntityFrameworkCore;
using ProfitPulse.Api.Data;

namespace ProfitPulse.Api.Services.Analysis;

public class TrendsService(AppDbContext db)
{
    public async Task<TrendsResponse> GetTrendsAsync(Guid cafeId, int days = 30)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        var dailyData = await db.DailySummaries
            .Where(d => d.CafeId == cafeId && d.Date >= since)
            .OrderBy(d => d.Date)
            .Select(d => new DailyTrendPoint(
                d.Date,
                Math.Round(d.TotalRevenue, 2),
                d.TotalRevenue > 0 ? Math.Round(d.FoodCost / d.TotalRevenue * 100, 1) : 0,
                d.TotalRevenue > 0 ? Math.Round(d.LabourCost / d.TotalRevenue * 100, 1) : 0,
                Math.Round(d.TotalRevenue - d.FoodCost - d.LabourCost - d.OtherCosts, 2),
                d.CustomerCount,
                d.TransactionCount
            ))
            .ToListAsync();

        // Weekly aggregates
        var weeklyData = dailyData
            .GroupBy(d => GetWeekStart(d.Date))
            .Select(g => new WeeklyTrendPoint(
                g.Key,
                Math.Round(g.Sum(d => d.Revenue), 2),
                Math.Round(g.Average(d => d.FoodCostPercent), 1),
                Math.Round(g.Average(d => d.LabourCostPercent), 1),
                Math.Round(g.Sum(d => d.NetProfit), 2),
                g.Sum(d => d.Customers),
                g.Count()
            ))
            .OrderBy(w => w.WeekStart)
            .ToList();

        var avgDailyRevenue = dailyData.Count > 0 ? Math.Round(dailyData.Average(d => d.Revenue), 2) : 0;
        var avgFoodCost = dailyData.Count > 0 ? Math.Round(dailyData.Average(d => d.FoodCostPercent), 1) : 0;
        var avgLabourCost = dailyData.Count > 0 ? Math.Round(dailyData.Average(d => d.LabourCostPercent), 1) : 0;

        return new TrendsResponse(
            Daily: dailyData,
            Weekly: weeklyData,
            AvgDailyRevenue: avgDailyRevenue,
            AvgFoodCostPercent: avgFoodCost,
            AvgLabourCostPercent: avgLabourCost,
            Days: days
        );
    }

    private static DateOnly GetWeekStart(DateOnly date)
    {
        var diff = (int)date.DayOfWeek - 1;
        if (diff < 0) diff = 6; // Sunday
        return date.AddDays(-diff);
    }
}

public record TrendsResponse(
    List<DailyTrendPoint> Daily,
    List<WeeklyTrendPoint> Weekly,
    decimal AvgDailyRevenue,
    decimal AvgFoodCostPercent,
    decimal AvgLabourCostPercent,
    int Days
);

public record DailyTrendPoint(
    DateOnly Date,
    decimal Revenue,
    decimal FoodCostPercent,
    decimal LabourCostPercent,
    decimal NetProfit,
    int Customers,
    int Transactions
);

public record WeeklyTrendPoint(
    DateOnly WeekStart,
    decimal Revenue,
    decimal AvgFoodCostPercent,
    decimal AvgLabourCostPercent,
    decimal NetProfit,
    int Customers,
    int DaysInWeek
);

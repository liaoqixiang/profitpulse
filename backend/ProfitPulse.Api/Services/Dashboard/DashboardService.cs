using Microsoft.EntityFrameworkCore;
using ProfitPulse.Api.Data;

namespace ProfitPulse.Api.Services.Dashboard;

public class DashboardService(AppDbContext db)
{
    public async Task<DashboardResponse> GetDashboardAsync(Guid cafeId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = today.AddDays(-(int)today.DayOfWeek + 1); // Monday
        var lastWeekStart = weekStart.AddDays(-7);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var summaries = await db.DailySummaries
            .Where(d => d.CafeId == cafeId && d.Date >= lastWeekStart)
            .ToListAsync();

        var todaySummary = summaries.FirstOrDefault(d => d.Date == today);
        var thisWeek = summaries.Where(d => d.Date >= weekStart && d.Date <= today).ToList();
        var lastWeek = summaries.Where(d => d.Date >= lastWeekStart && d.Date < weekStart).ToList();

        // Also get month data (may need wider query)
        var monthSummaries = today.Month == weekStart.Month
            ? summaries.Where(d => d.Date >= monthStart).ToList()
            : await db.DailySummaries
                .Where(d => d.CafeId == cafeId && d.Date >= monthStart && d.Date <= today)
                .ToListAsync();

        var weekRevenue = thisWeek.Sum(d => d.TotalRevenue);
        var lastWeekRevenue = lastWeek.Sum(d => d.TotalRevenue);
        var monthRevenue = monthSummaries.Sum(d => d.TotalRevenue);

        var weekFoodCost = weekRevenue > 0 ? thisWeek.Sum(d => d.FoodCost) / weekRevenue * 100 : 0;
        var weekLabourCost = weekRevenue > 0 ? thisWeek.Sum(d => d.LabourCost) / weekRevenue * 100 : 0;
        var weekOtherCost = weekRevenue > 0 ? thisWeek.Sum(d => d.OtherCosts) / weekRevenue * 100 : 0;
        var netMargin = 100 - weekFoodCost - weekLabourCost - weekOtherCost;

        var lastWeekFoodPct = lastWeekRevenue > 0 ? lastWeek.Sum(d => d.FoodCost) / lastWeekRevenue * 100 : 0;
        var lastWeekLabourPct = lastWeekRevenue > 0 ? lastWeek.Sum(d => d.LabourCost) / lastWeekRevenue * 100 : 0;

        var weekCustomers = thisWeek.Sum(d => d.CustomerCount);
        var lastWeekCustomers = lastWeek.Sum(d => d.CustomerCount);
        var weekTransactions = thisWeek.Sum(d => d.TransactionCount);

        var metrics = new DashboardMetrics(
            TodayRevenue: todaySummary?.TotalRevenue ?? 0,
            WeekRevenue: weekRevenue,
            MonthRevenue: monthRevenue,
            FoodCostPercent: Math.Round(weekFoodCost, 1),
            LabourCostPercent: Math.Round(weekLabourCost, 1),
            NetProfitMargin: Math.Round(netMargin, 1),
            TodayCustomers: todaySummary?.CustomerCount ?? 0,
            AvgTransactionValue: weekTransactions > 0 ? Math.Round(weekRevenue / weekTransactions, 2) : 0
        );

        var revTrend = lastWeekRevenue > 0 ? (weekRevenue - lastWeekRevenue) / lastWeekRevenue * 100 : 0;
        var foodTrend = lastWeekFoodPct > 0 ? weekFoodCost - lastWeekFoodPct : 0;
        var labourTrend = lastWeekLabourPct > 0 ? weekLabourCost - lastWeekLabourPct : 0;
        var custTrend = lastWeekCustomers > 0 ? (decimal)(weekCustomers - lastWeekCustomers) / lastWeekCustomers * 100 : 0;

        var trends = new DashboardTrends(
            RevenueVsLastWeek: Math.Round(revTrend, 1),
            FoodCostVsLastWeek: Math.Round(foodTrend, 1),
            LabourCostVsLastWeek: Math.Round(labourTrend, 1),
            CustomersVsLastWeek: Math.Round(custTrend, 1)
        );

        var alerts = new List<DashboardAlert>();
        if (weekFoodCost > 35)
            alerts.Add(new("food_cost", $"Food cost at {weekFoodCost:F1}% — above 35% target", "warning"));
        if (weekLabourCost > 32)
            alerts.Add(new("labour_cost", $"Labour cost at {weekLabourCost:F1}% — above 30% target", "warning"));
        if (revTrend < -10)
            alerts.Add(new("revenue_drop", $"Revenue down {Math.Abs(revTrend):F1}% vs last week", "danger"));
        if (revTrend > 10)
            alerts.Add(new("revenue_up", $"Revenue up {revTrend:F1}% vs last week — great work!", "success"));

        return new DashboardResponse(metrics, trends, alerts);
    }

    public async Task<WeeklyBriefResponse?> GetLatestBriefAsync(Guid cafeId)
    {
        var brief = await db.WeeklyBriefs
            .Where(w => w.CafeId == cafeId)
            .OrderByDescending(w => w.WeekStarting)
            .FirstOrDefaultAsync();

        if (brief is null) return null;

        return new WeeklyBriefResponse(
            brief.Summary,
            brief.Highlights,
            brief.Concerns,
            brief.Recommendations,
            brief.WeekStarting
        );
    }
}

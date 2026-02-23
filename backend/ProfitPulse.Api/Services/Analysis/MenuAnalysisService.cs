using Microsoft.EntityFrameworkCore;
using ProfitPulse.Api.Data;

namespace ProfitPulse.Api.Services.Analysis;

public class MenuAnalysisService(AppDbContext db)
{
    public async Task<MenuPerformanceResponse> GetPerformanceAsync(Guid cafeId, int days = 7)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        var items = await db.MenuItems
            .Where(m => m.CafeId == cafeId && m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.Name,
                Category = m.Category.ToString(),
                m.Price,
                m.CostToMake,
                TotalSold = m.Sales.Where(s => s.Date >= since).Sum(s => s.QuantitySold),
                Revenue = m.Sales.Where(s => s.Date >= since).Sum(s => s.QuantitySold) * m.Price,
                TotalCost = m.Sales.Where(s => s.Date >= since).Sum(s => s.QuantitySold) * m.CostToMake
            })
            .OrderByDescending(m => m.Revenue)
            .ToListAsync();

        var totalRevenue = items.Sum(i => i.Revenue);

        var itemPerformances = items.Select(i => new MenuItemPerformance(
            Id: i.Id,
            Name: i.Name,
            Category: i.Category,
            Price: i.Price,
            CostToMake: i.CostToMake,
            MarginPercent: i.Price > 0 ? Math.Round((i.Price - i.CostToMake) / i.Price * 100, 1) : 0,
            TotalSold: i.TotalSold,
            Revenue: Math.Round(i.Revenue, 2),
            Profit: Math.Round(i.Revenue - i.TotalCost, 2),
            RevenueShare: totalRevenue > 0 ? Math.Round(i.Revenue / totalRevenue * 100, 1) : 0
        )).ToList();

        var avgMargin = itemPerformances.Count > 0
            ? Math.Round(itemPerformances.Average(i => i.MarginPercent), 1)
            : 0;

        var bestItem = itemPerformances.MaxBy(i => i.Profit);
        var worstItem = itemPerformances.Where(i => i.TotalSold > 0).MinBy(i => i.MarginPercent);

        return new MenuPerformanceResponse(
            Items: itemPerformances,
            TotalRevenue: Math.Round(totalRevenue, 2),
            AverageMargin: avgMargin,
            BestPerformer: bestItem?.Name ?? "",
            WorstMargin: worstItem?.Name ?? "",
            Days: days
        );
    }
}

public record MenuPerformanceResponse(
    List<MenuItemPerformance> Items,
    decimal TotalRevenue,
    decimal AverageMargin,
    string BestPerformer,
    string WorstMargin,
    int Days
);

public record MenuItemPerformance(
    Guid Id,
    string Name,
    string Category,
    decimal Price,
    decimal CostToMake,
    decimal MarginPercent,
    int TotalSold,
    decimal Revenue,
    decimal Profit,
    decimal RevenueShare
);

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProfitPulse.Api.Data;
using ProfitPulse.Api.Data.Entities;
using ProfitPulse.Api.Services.Analysis;
using ProfitPulse.Api.Services.Dashboard;

namespace ProfitPulse.Api.Services.AI;

public class InsightService(
    IAiProvider aiProvider,
    AppDbContext db,
    MenuAnalysisService menuService,
    StaffCostService staffService,
    DashboardService dashboardService,
    ILogger<InsightService> logger)
{
    public async Task<List<AIInsight>> GenerateInsightsAsync(Guid cafeId, CancellationToken ct = default)
    {
        // Gather data from all services
        var dashboard = await dashboardService.GetDashboardAsync(cafeId);
        var menu = await menuService.GetPerformanceAsync(cafeId, 7);
        var staff = await staffService.GetCostsAsync(cafeId, 7);

        var dataContext = $"""
            === DASHBOARD METRICS (This Week) ===
            Week Revenue: ${dashboard.Metrics.WeekRevenue:N2}
            Month Revenue: ${dashboard.Metrics.MonthRevenue:N2}
            Food Cost: {dashboard.Metrics.FoodCostPercent}%
            Labour Cost: {dashboard.Metrics.LabourCostPercent}%
            Net Profit Margin: {dashboard.Metrics.NetProfitMargin}%
            Avg Transaction: ${dashboard.Metrics.AvgTransactionValue:N2}
            Revenue vs Last Week: {dashboard.Trends.RevenueVsLastWeek:+#.#;-#.#;0}%

            === MENU PERFORMANCE (7 Days) ===
            Total Menu Revenue: ${menu.TotalRevenue:N2}
            Average Margin: {menu.AverageMargin}%
            Best Performer: {menu.BestPerformer}
            Worst Margin Item: {menu.WorstMargin}
            Top 5 Items by Revenue:
            {string.Join("\n", menu.Items.Take(5).Select(i => $"  - {i.Name}: ${i.Revenue:N2} revenue, {i.MarginPercent}% margin, {i.TotalSold} sold"))}
            Bottom 3 Items by Margin:
            {string.Join("\n", menu.Items.Where(i => i.TotalSold > 0).OrderBy(i => i.MarginPercent).Take(3).Select(i => $"  - {i.Name}: {i.MarginPercent}% margin, ${i.Revenue:N2} revenue"))}

            === STAFF COSTS (7 Days) ===
            Total Labour Cost: ${staff.TotalLabourCost:N2}
            Labour Cost %: {staff.LabourCostPercent}%
            Total Overtime Hours: {staff.TotalOvertimeHours}h
            Staff with Overtime:
            {string.Join("\n", staff.Staff.Where(s => s.HasOvertime).Select(s => $"  - {s.Name} ({s.Role}): {s.OvertimeHours}h overtime"))}

            === ALERTS ===
            {string.Join("\n", dashboard.Alerts.Select(a => $"  - [{a.Severity}] {a.Message}"))}
            """;

        var prompt = ProfitPulsePrompts.BuildInsightPrompt(dataContext);

        try
        {
            var response = await aiProvider.GenerateAsync(ProfitPulsePrompts.SystemPrompt, prompt, ct);

            // Strip markdown code fences if present
            response = response.Trim();
            if (response.StartsWith("```"))
            {
                var firstNewline = response.IndexOf('\n');
                response = response[(firstNewline + 1)..];
                if (response.EndsWith("```"))
                    response = response[..^3];
                response = response.Trim();
            }

            var insights = JsonSerializer.Deserialize<List<InsightDto>>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (insights is null or { Count: 0 })
                return [];

            var entities = insights.Select(i => new AIInsight
            {
                Id = Guid.NewGuid(),
                CafeId = cafeId,
                Title = i.Title,
                Summary = i.Summary,
                DetailedAnalysis = i.DetailedAnalysis,
                RecommendedAction = i.RecommendedAction,
                Category = Enum.TryParse<InsightCategory>(i.Category, true, out var cat) ? cat : InsightCategory.Revenue,
                Priority = Enum.TryParse<InsightPriority>(i.Priority, true, out var pri) ? pri : InsightPriority.Medium,
                PotentialImpact = i.PotentialImpact,
                Status = InsightStatus.New,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            db.AIInsights.AddRange(entities);
            await db.SaveChangesAsync(ct);

            return entities;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate AI insights for cafe {CafeId}", cafeId);
            throw;
        }
    }

    public async Task<List<AIInsight>> GetInsightsAsync(Guid cafeId, InsightStatus? status = null)
    {
        var query = db.AIInsights
            .Where(i => i.CafeId == cafeId);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<bool> UpdateInsightStatusAsync(Guid insightId, Guid cafeId, InsightStatus newStatus)
    {
        var insight = await db.AIInsights
            .FirstOrDefaultAsync(i => i.Id == insightId && i.CafeId == cafeId);

        if (insight is null) return false;

        insight.Status = newStatus;
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<WeeklyBrief?> GenerateWeeklyBriefAsync(Guid cafeId, CancellationToken ct = default)
    {
        var dashboard = await dashboardService.GetDashboardAsync(cafeId);
        var menu = await menuService.GetPerformanceAsync(cafeId, 7);
        var staff = await staffService.GetCostsAsync(cafeId, 7);

        var dataContext = $"""
            Week Revenue: ${dashboard.Metrics.WeekRevenue:N2}
            Food Cost: {dashboard.Metrics.FoodCostPercent}%
            Labour Cost: {dashboard.Metrics.LabourCostPercent}%
            Net Margin: {dashboard.Metrics.NetProfitMargin}%
            Revenue vs Last Week: {dashboard.Trends.RevenueVsLastWeek:+#.#;-#.#;0}%
            Customer Trend: {dashboard.Trends.CustomersVsLastWeek:+#.#;-#.#;0}%
            Best Menu Item: {menu.BestPerformer}
            Worst Margin: {menu.WorstMargin} ({menu.Items.Where(i => i.Name == menu.WorstMargin).Select(i => i.MarginPercent).FirstOrDefault()}%)
            Overtime Hours: {staff.TotalOvertimeHours}h
            Alerts: {string.Join("; ", dashboard.Alerts.Select(a => a.Message))}
            """;

        var prompt = ProfitPulsePrompts.BuildWeeklyBriefPrompt(dataContext);

        try
        {
            var response = await aiProvider.GenerateAsync(ProfitPulsePrompts.SystemPrompt, prompt, ct);

            response = response.Trim();
            if (response.StartsWith("```"))
            {
                var firstNewline = response.IndexOf('\n');
                response = response[(firstNewline + 1)..];
                if (response.EndsWith("```"))
                    response = response[..^3];
                response = response.Trim();
            }

            var briefDto = JsonSerializer.Deserialize<WeeklyBriefDto>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (briefDto is null) return null;

            var weekStart = DateOnly.FromDateTime(DateTime.UtcNow);
            weekStart = weekStart.AddDays(-(int)weekStart.DayOfWeek + 1);

            var entity = new WeeklyBrief
            {
                Id = Guid.NewGuid(),
                CafeId = cafeId,
                WeekStarting = weekStart,
                Summary = briefDto.Summary,
                Highlights = briefDto.Highlights,
                Concerns = briefDto.Concerns,
                Recommendations = briefDto.Recommendations,
                CreatedAt = DateTime.UtcNow
            };

            // Replace existing brief for this week
            var existing = await db.WeeklyBriefs
                .FirstOrDefaultAsync(w => w.CafeId == cafeId && w.WeekStarting == weekStart, ct);
            if (existing is not null)
                db.WeeklyBriefs.Remove(existing);

            db.WeeklyBriefs.Add(entity);
            await db.SaveChangesAsync(ct);

            return entity;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate weekly brief for cafe {CafeId}", cafeId);
            throw;
        }
    }
}

file record InsightDto(
    string Title,
    string Summary,
    string DetailedAnalysis,
    string? RecommendedAction,
    string Category,
    string Priority,
    decimal? PotentialImpact
);

file record WeeklyBriefDto(
    string Summary,
    string Highlights,
    string Concerns,
    string Recommendations
);

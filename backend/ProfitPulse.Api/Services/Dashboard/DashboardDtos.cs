namespace ProfitPulse.Api.Services.Dashboard;

public record DashboardResponse(
    DashboardMetrics Metrics,
    DashboardTrends Trends,
    List<DashboardAlert> Alerts
);

public record DashboardMetrics(
    decimal TodayRevenue,
    decimal WeekRevenue,
    decimal MonthRevenue,
    decimal FoodCostPercent,
    decimal LabourCostPercent,
    decimal NetProfitMargin,
    int TodayCustomers,
    decimal AvgTransactionValue
);

public record DashboardTrends(
    decimal RevenueVsLastWeek,
    decimal FoodCostVsLastWeek,
    decimal LabourCostVsLastWeek,
    decimal CustomersVsLastWeek
);

public record DashboardAlert(
    string Type,
    string Message,
    string Severity
);

public record WeeklyBriefResponse(
    string Summary,
    string Highlights,
    string Concerns,
    string Recommendations,
    DateOnly WeekStarting
);

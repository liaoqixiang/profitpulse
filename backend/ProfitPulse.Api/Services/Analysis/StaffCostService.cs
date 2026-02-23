using Microsoft.EntityFrameworkCore;
using ProfitPulse.Api.Data;

namespace ProfitPulse.Api.Services.Analysis;

public class StaffCostService(AppDbContext db)
{
    public async Task<StaffCostResponse> GetCostsAsync(Guid cafeId, int days = 7)
    {
        var since = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-days);

        var staff = await db.StaffMembers
            .Where(s => s.CafeId == cafeId && s.IsActive)
            .Select(s => new
            {
                s.Id,
                s.Name,
                Role = s.Role.ToString(),
                PayType = s.PayType.ToString(),
                s.HourlyRate,
                s.AnnualSalary,
                TotalHours = s.Shifts.Where(sh => sh.Date >= since).Sum(sh => sh.HoursWorked),
                OvertimeHours = s.Shifts.Where(sh => sh.Date >= since).Sum(sh => sh.OvertimeHours),
                ShiftCost = s.Shifts.Where(sh => sh.Date >= since).Sum(sh => sh.TotalCost),
                DaysWorked = s.Shifts.Count(sh => sh.Date >= since)
            })
            .OrderByDescending(s => s.ShiftCost)
            .ToListAsync();

        var totalRevenue = await db.DailySummaries
            .Where(d => d.CafeId == cafeId && d.Date >= since)
            .SumAsync(d => d.TotalRevenue);

        var staffBreakdown = staff.Select(s =>
        {
            // For salary staff, calculate the period cost
            var periodCost = s.PayType == "Salary" && s.AnnualSalary.HasValue
                ? Math.Round(s.AnnualSalary.Value / 365 * days, 2)
                : s.ShiftCost;

            return new StaffCostBreakdown(
                Id: s.Id,
                Name: s.Name,
                Role: s.Role,
                PayType: s.PayType,
                HourlyRate: s.HourlyRate,
                TotalHours: Math.Round(s.TotalHours, 1),
                OvertimeHours: Math.Round(s.OvertimeHours, 1),
                DaysWorked: s.DaysWorked,
                PeriodCost: periodCost,
                HasOvertime: s.OvertimeHours > 0,
                AvgHoursPerDay: s.DaysWorked > 0 ? Math.Round(s.TotalHours / s.DaysWorked, 1) : 0
            );
        }).ToList();

        var totalLabourCost = staffBreakdown.Sum(s => s.PeriodCost);
        var totalOvertimeHours = staffBreakdown.Sum(s => s.OvertimeHours);
        var labourCostPercent = totalRevenue > 0 ? Math.Round(totalLabourCost / totalRevenue * 100, 1) : 0;

        return new StaffCostResponse(
            Staff: staffBreakdown,
            TotalLabourCost: Math.Round(totalLabourCost, 2),
            LabourCostPercent: labourCostPercent,
            TotalOvertimeHours: Math.Round(totalOvertimeHours, 1),
            TotalRevenue: Math.Round(totalRevenue, 2),
            Days: days
        );
    }
}

public record StaffCostResponse(
    List<StaffCostBreakdown> Staff,
    decimal TotalLabourCost,
    decimal LabourCostPercent,
    decimal TotalOvertimeHours,
    decimal TotalRevenue,
    int Days
);

public record StaffCostBreakdown(
    Guid Id,
    string Name,
    string Role,
    string PayType,
    decimal HourlyRate,
    decimal TotalHours,
    decimal OvertimeHours,
    int DaysWorked,
    decimal PeriodCost,
    bool HasOvertime,
    decimal AvgHoursPerDay
);

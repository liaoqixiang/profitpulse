using Microsoft.EntityFrameworkCore;
using ProfitPulse.Api.Data.Entities;

namespace ProfitPulse.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Cafe> Cafes => Set<Cafe>();
    public DbSet<DemoUser> Users => Set<DemoUser>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<MenuItemSales> MenuItemSales => Set<MenuItemSales>();
    public DbSet<DailySummary> DailySummaries => Set<DailySummary>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<StaffShift> StaffShifts => Set<StaffShift>();
    public DbSet<AIInsight> AIInsights => Set<AIInsight>();
    public DbSet<WeeklyBrief> WeeklyBriefs => Set<WeeklyBrief>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cafe
        modelBuilder.Entity<Cafe>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.GstRate).HasPrecision(5, 4);
            e.Property(c => c.TargetFoodCostPercent).HasPrecision(5, 2);
            e.Property(c => c.TargetLabourCostPercent).HasPrecision(5, 2);
        });

        // DemoUser
        modelBuilder.Entity<DemoUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasOne(u => u.Cafe).WithMany(c => c.Users).HasForeignKey(u => u.CafeId);
        });

        // MenuItem
        modelBuilder.Entity<MenuItem>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Category).HasConversion<string>();
            e.Property(m => m.Price).HasPrecision(10, 2);
            e.Property(m => m.CostToMake).HasPrecision(10, 2);
            e.HasOne(m => m.Cafe).WithMany(c => c.MenuItems).HasForeignKey(m => m.CafeId);
            e.Ignore(m => m.Margin);
        });

        // MenuItemSales
        modelBuilder.Entity<MenuItemSales>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.MenuItemId, s.Date }).IsUnique();
            e.HasOne(s => s.MenuItem).WithMany(m => m.Sales).HasForeignKey(s => s.MenuItemId);
        });

        // DailySummary
        modelBuilder.Entity<DailySummary>(e =>
        {
            e.HasKey(d => d.Id);
            e.HasIndex(d => new { d.CafeId, d.Date }).IsUnique();
            e.Property(d => d.TotalRevenue).HasPrecision(12, 2);
            e.Property(d => d.FoodCost).HasPrecision(12, 2);
            e.Property(d => d.LabourCost).HasPrecision(12, 2);
            e.Property(d => d.OtherCosts).HasPrecision(12, 2);
            e.HasOne(d => d.Cafe).WithMany(c => c.DailySummaries).HasForeignKey(d => d.CafeId);
            e.Ignore(d => d.GrossProfit);
            e.Ignore(d => d.NetProfit);
            e.Ignore(d => d.FoodCostPercent);
            e.Ignore(d => d.LabourCostPercent);
        });

        // StaffMember
        modelBuilder.Entity<StaffMember>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Role).HasConversion<string>();
            e.Property(s => s.PayType).HasConversion<string>();
            e.Property(s => s.HourlyRate).HasPrecision(8, 2);
            e.Property(s => s.AnnualSalary).HasPrecision(12, 2);
            e.HasOne(s => s.Cafe).WithMany(c => c.Staff).HasForeignKey(s => s.CafeId);
        });

        // StaffShift
        modelBuilder.Entity<StaffShift>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.HoursWorked).HasPrecision(5, 2);
            e.Property(s => s.OvertimeHours).HasPrecision(5, 2);
            e.Property(s => s.TotalCost).HasPrecision(10, 2);
            e.HasIndex(s => new { s.StaffMemberId, s.Date }).IsUnique();
            e.HasOne(s => s.StaffMember).WithMany(m => m.Shifts).HasForeignKey(s => s.StaffMemberId);
        });

        // AIInsight
        modelBuilder.Entity<AIInsight>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Category).HasConversion<string>();
            e.Property(i => i.Priority).HasConversion<string>();
            e.Property(i => i.Status).HasConversion<string>();
            e.Property(i => i.PotentialImpact).HasPrecision(12, 2);
            e.HasOne(i => i.Cafe).WithMany(c => c.Insights).HasForeignKey(i => i.CafeId);
        });

        // WeeklyBrief
        modelBuilder.Entity<WeeklyBrief>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => new { w.CafeId, w.WeekStarting }).IsUnique();
            e.HasOne(w => w.Cafe).WithMany(c => c.WeeklyBriefs).HasForeignKey(w => w.CafeId);
        });
    }
}

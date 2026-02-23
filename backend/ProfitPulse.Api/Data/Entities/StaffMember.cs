namespace ProfitPulse.Api.Data.Entities;

public class StaffMember
{
    public Guid Id { get; set; }
    public Guid CafeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public StaffRole Role { get; set; }
    public PayType PayType { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal? AnnualSalary { get; set; }
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; } = true;

    public Cafe Cafe { get; set; } = null!;
    public ICollection<StaffShift> Shifts { get; set; } = [];
}

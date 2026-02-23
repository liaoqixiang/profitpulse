namespace ProfitPulse.Api.Data.Entities;

public class StaffShift
{
    public Guid Id { get; set; }
    public Guid StaffMemberId { get; set; }
    public DateOnly Date { get; set; }
    public decimal HoursWorked { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal TotalCost { get; set; }

    public StaffMember StaffMember { get; set; } = null!;
}

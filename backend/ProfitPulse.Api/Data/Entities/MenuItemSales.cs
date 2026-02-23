namespace ProfitPulse.Api.Data.Entities;

public class MenuItemSales
{
    public Guid Id { get; set; }
    public Guid MenuItemId { get; set; }
    public DateOnly Date { get; set; }
    public int QuantitySold { get; set; }

    public MenuItem MenuItem { get; set; } = null!;
}

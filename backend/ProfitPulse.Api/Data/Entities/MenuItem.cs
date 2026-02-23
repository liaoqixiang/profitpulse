namespace ProfitPulse.Api.Data.Entities;

public class MenuItem
{
    public Guid Id { get; set; }
    public Guid CafeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public MenuCategory Category { get; set; }
    public decimal Price { get; set; }
    public decimal CostToMake { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cafe Cafe { get; set; } = null!;
    public ICollection<MenuItemSales> Sales { get; set; } = [];

    public decimal Margin => Price > 0 ? (Price - CostToMake) / Price * 100 : 0;
}

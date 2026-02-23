namespace ProfitPulse.Api.Data.Entities;

public class DemoUser
{
    public Guid Id { get; set; }
    public Guid CafeId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cafe Cafe { get; set; } = null!;
}

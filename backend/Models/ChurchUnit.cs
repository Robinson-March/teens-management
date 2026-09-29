namespace TeensChurch.API.Models;

public class ChurchUnit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string BadgeColor { get; set; } = "purple";

    public string? IconKey { get; set; } = "sparkles";

    public bool IsDefault { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

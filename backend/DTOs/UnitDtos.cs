namespace TeensChurch.API.DTOs;

public class CreateUnitDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BadgeColor { get; set; }
    public string? IconKey { get; set; }
}

public class UpdateUnitDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? BadgeColor { get; set; }
    public string? IconKey { get; set; }
}

public class UnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string BadgeColor { get; set; } = "purple";
    public string? IconKey { get; set; }
    public bool IsDefault { get; set; }
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

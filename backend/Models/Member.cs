namespace TeensChurch.API.Models;

public class Member
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public int? Age { get; set; }

    public string? PhoneNumber { get; set; }

    public string? AcademicLevel { get; set; }

    public string? Departments { get; set; }

    public string ServiceTime { get; set; } = "8:30 service";

    public string? GuardianName { get; set; }

    public string Status { get; set; } = "active";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

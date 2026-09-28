using Microsoft.EntityFrameworkCore;
using TeensChurch.API.Models;

namespace TeensChurch.API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            // Try to ensure database and tables exist
            await context.Database.EnsureCreatedAsync();

            if (!await context.Members.AnyAsync())
            {
                logger.LogInformation("Seeding initial church members into database...");

                var members = new List<Member>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Daniel Adeyemi",
                        Age = 16,
                        PhoneNumber = "0803 241 9912",
                        AcademicLevel = "Senior Secondary (SS3)",
                        Departments = "Media & Tech",
                        ServiceTime = "8:30 service",
                        GuardianName = "Mrs. Adeyemi",
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-20)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Miracle Praise",
                        Age = 15,
                        PhoneNumber = "0812 400 1122",
                        AcademicLevel = "Senior Secondary (SS2)",
                        Departments = "Choir & Vocals",
                        ServiceTime = "8:30 service",
                        GuardianName = "Deaconess Esther",
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-15)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Joshua Olatunji",
                        Age = 17,
                        PhoneNumber = "0809 111 8844",
                        AcademicLevel = "Pre-Varsity (A-Level)",
                        Departments = "Sanctuary Keepers",
                        ServiceTime = "6:30 service",
                        GuardianName = "Elder Tunji",
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        FullName = "Kemi Balogun",
                        Age = 13,
                        PhoneNumber = "0802 884 7711",
                        AcademicLevel = "Junior Secondary (JSS2)",
                        Departments = "Drama & Creative",
                        ServiceTime = "8:30 service",
                        GuardianName = "Dr. Balogun",
                        Status = "attention",
                        CreatedAt = DateTime.UtcNow.AddDays(-5)
                    }
                };

                await context.Members.AddRangeAsync(members);
                await context.SaveChangesAsync();
                logger.LogInformation("Database seeded successfully with {Count} initial members.", members.Count);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not initialize database automatically. PostgreSQL connection may need configuration.");
        }
    }
}

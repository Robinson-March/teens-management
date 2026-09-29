using Microsoft.EntityFrameworkCore;
using TeensChurch.API.Models;

namespace TeensChurch.API.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(AppDbContext context, ILogger logger)
    {
        try
        {
            // Ensure database and tables exist
            await context.Database.EnsureCreatedAsync();

            // Create Units table if it doesn't exist yet (handles pre-existing database without migrations)
            var createUnitsSql = @"
                CREATE TABLE IF NOT EXISTS ""Units"" (
                    ""Id"" uuid NOT NULL PRIMARY KEY,
                    ""Name"" character varying(100) NOT NULL,
                    ""Description"" character varying(500),
                    ""BadgeColor"" character varying(30) NOT NULL DEFAULT 'purple',
                    ""IconKey"" character varying(50) NOT NULL DEFAULT 'sparkles',
                    ""IsDefault"" boolean NOT NULL DEFAULT FALSE,
                    ""CreatedAt"" timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP
                );
                CREATE UNIQUE INDEX IF NOT EXISTS ""IX_Units_Name"" ON ""Units"" (""Name"");
            ";
            await context.Database.ExecuteSqlRawAsync(createUnitsSql);

            // Seed default units if none exist
            if (!await context.Units.AnyAsync())
            {
                var defaultUnits = new List<ChurchUnit>
                {
                    new() { Name = "Sanctuary Keepers", Description = "Auditorium care, sanctuary arrangement & cleanliness", BadgeColor = "amber", IconKey = "sparkles", IsDefault = true },
                    new() { Name = "Library", Description = "Study materials, reading club & literature resources", BadgeColor = "emerald", IconKey = "book", IsDefault = true },
                    new() { Name = "Choir", Description = "Choral worship, vocals & harmonies", BadgeColor = "purple", IconKey = "mic", IsDefault = true },
                    new() { Name = "Music", Description = "Instrumentalists, audio band & music development", BadgeColor = "indigo", IconKey = "music", IsDefault = true },
                    new() { Name = "Media", Description = "AV desk, camera, projection & streaming", BadgeColor = "blue", IconKey = "camera", IsDefault = true },
                    new() { Name = "Publicity", Description = "Noticeboards, design flyers & communications", BadgeColor = "sky", IconKey = "sparkles", IsDefault = true },
                    new() { Name = "Bible Study", Description = "Scripture discussion, Bible teachers & discipleship", BadgeColor = "teal", IconKey = "book", IsDefault = true },
                    new() { Name = "Conducting", Description = "Service moderation, order of service & prayer anchoring", BadgeColor = "rose", IconKey = "users", IsDefault = true },
                    new() { Name = "Teens Ushering", Description = "Auditorium hospitality, welcoming & teen guidance", BadgeColor = "orange", IconKey = "users", IsDefault = true },
                    new() { Name = "Drama & Creative", Description = "Skit presentations, creative expressions & arts", BadgeColor = "pink", IconKey = "sparkles", IsDefault = true },
                    new() { Name = "General Assembly", Description = "General teen fellowship / undecided members", BadgeColor = "slate", IconKey = "users", IsDefault = true }
                };

                await context.Units.AddRangeAsync(defaultUnits);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default church units into database.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not initialize database automatically. PostgreSQL connection may need configuration.");
        }
    }
}

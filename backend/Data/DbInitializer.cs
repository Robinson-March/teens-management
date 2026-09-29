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
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not initialize database automatically. PostgreSQL connection may need configuration.");
        }
    }
}

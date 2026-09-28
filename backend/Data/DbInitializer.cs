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
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not initialize database automatically. PostgreSQL connection may need configuration.");
        }
    }
}

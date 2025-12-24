using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QuestionService.Data.Extensions;

public static class MigrationExtensions
{
    extension(WebApplication app)
    {
        public async Task MigrateDatabase(bool resetDatabase = false)
        {
            using var scope = app.Services.CreateScope();

            try
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<QuestionDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<QuestionDbContext>>();

                if (resetDatabase)
                {
                    logger.LogWarning("Resetting database - all data will be deleted!");
                    await dbContext.Database.EnsureDeletedAsync();
                    logger.LogInformation("Database deleted successfully.");
                }

                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully.");
            }
            catch (Exception e)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebApplication>>();
                logger.LogError(e, "An error occurred while migrating or seeding the database.");
            }
        }

        public async Task MigrateDatabaseWithOptions()
        {
            var configuration = app.Services.GetRequiredService<IConfiguration>();
            var resetDatabase = configuration.GetValue<bool>("Database:ResetOnStartup");

            await app.MigrateDatabase(resetDatabase);
        }
    }
}

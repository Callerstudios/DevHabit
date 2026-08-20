using DevHabit.API.Database;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Database Migration Successful");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An Error Occured");
            throw;
        }
    }
}

using DevHabit.API.Database;
using DevHabit.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await using ApplicationIdentityDbContext identityDbContext = scope.ServiceProvider.GetRequiredService<ApplicationIdentityDbContext>();

        try
        {
            await applicationDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("Application Database Migration Successful");
            await identityDbContext.Database.MigrateAsync();
            app.Logger.LogInformation("identity Database Migration Successful");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An Error Occured");
            throw;
        }
    }
    public static async Task SeedInitialDataAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();

        RoleManager<IdentityRole> roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles =
        [
            Roles.Admin,
            Roles.Member
        ];
        try
        {
            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            app.Logger.LogInformation("Successfully created roles");
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "An Error occured while seeding initial data");
            throw;
        }
    }
}

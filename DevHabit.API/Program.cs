using DevHabit.API.Middlewares;
using DevHabit.API;
using DevHabit.API.Extensions;
using DevHabit.API.Settings;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddApiServices()
    .AddErrorHandling()
    .AddDatabase()
    .AddObservability()
    .AddApplicationServices()
    .AddAuthenticationServices()
    .AddCorsPolicy();


WebApplication app = builder.Build();

Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.ApplyMigrationsAsync();

    await app.SeedInitialDataAsync();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors(CorsOptionsExtension.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ETagMiddleware>();

app.MapControllers();

await app.RunAsync();

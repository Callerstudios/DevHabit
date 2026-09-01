namespace DevHabit.API.Settings;

public class CorsOptionsExtension
{
    public const string PolicyName = "DevHabitCorsPolicy";
    public const string SectionName = "Cors";

    public required IReadOnlyList<string> AllowedOrigins { get; init; }
}

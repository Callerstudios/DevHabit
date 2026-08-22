namespace DevHabit.API.Entities;

public sealed class HabitTag
{
    public string HabitId { get; set; } = string.Empty;
    public string TagId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

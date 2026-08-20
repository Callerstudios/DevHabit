using DevHabit.API.Entities;

namespace DevHabit.API.DTOs.Habits;

public sealed record UpdateHabitDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required HabitType Type { get; init; }
    public required FrequencyDto Frequency { get; init; }
    public required TargetDto Target { get; init; }
    public DateOnly? EndDate { get; init; }
    public MilestoneDto? Milestone { get; init; }
}
public sealed record UpdateMilestoneDto
{
    public required int Target { get; init; }
}

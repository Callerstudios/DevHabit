using DevHabit.API.DTOs.Habits;

namespace DevHabit.API.DTOs.Tags;

public sealed record TagDto
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
}

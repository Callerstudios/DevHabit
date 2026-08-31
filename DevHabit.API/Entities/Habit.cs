namespace DevHabit.API.Entities;

public sealed class Habit
{
    private readonly List<HabitTag> _habitTags = [];
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HabitType Type { get; set; }
    public Frequency Frequency { get; set; } = new();
    public Target Target { get; set; } = new();
    public HabitStatus Status { get; set; }
    public bool IsArchived { get; set; }
    public DateOnly? EndDate { get; set; }
    public Milestone? Milestone { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastCompletedAtUtc { get; set; }

    public IReadOnlyCollection<HabitTag> HabitTags => _habitTags;
    public IReadOnlyCollection<Tag> Tags { get; } = [];

    public void RemoveTagsExcept(IEnumerable<string> tagIds)
    {
        _habitTags.RemoveAll(ht => !tagIds.Contains(ht.TagId));
    }
    public void AddTagIdsRange(IEnumerable<string> tagIds, string habitId)
    {
        _habitTags.AddRange(tagIds.Select(tagId =>
        {
            return new HabitTag
            {
                HabitId = habitId,
                TagId = tagId,
                CreatedAtUtc = DateTime.UtcNow,
            };
        }));
    }
}

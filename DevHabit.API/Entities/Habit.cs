namespace DevHabit.API.Entities;

public sealed class Habit
{
    private readonly List<HabitTag> _habitTags = [];
    public string Id { get; set; } = String.Empty;
    public string Name { get; set; } = String.Empty;
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
    public IReadOnlyCollection<Tag> Tags { get; } = new List<Tag>();

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

public enum HabitType
{
    None = 0,
    Binary = 1,
    Measurable = 2,
}

public enum FrequencyType
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
}
public enum HabitStatus
{
    None = 0,
    Ongoing = 1,
    Completed = 2,
}

public sealed class Frequency
{
    public FrequencyType Type { get; set; }
    public int TimesPerPeriod { get; set; }

}
public sealed class Target
{
    public int Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}
public sealed class Milestone
{
    public int Target { get; set; }
    public int Current { get; set; }
}

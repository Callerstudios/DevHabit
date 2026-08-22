using System.Collections.ObjectModel;

namespace DevHabit.API.DTOs.HabitTags
{
    public sealed record UpsertHabitTagsDto
    {
        public required ReadOnlyCollection<string> TagIds { get; init; }
    }
}

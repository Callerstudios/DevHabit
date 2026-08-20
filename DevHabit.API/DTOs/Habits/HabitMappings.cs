using DevHabit.API.Entities;

namespace DevHabit.API.DTOs.Habits;

internal static class HabitMappings
{
    public static HabitDto ToDto(this Habit habit)
    {
        return new HabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Description = habit.Description,
            Type = habit.Type,

            Frequency = new FrequencyDto
            {
                Type = habit.Frequency.Type,
                TimesPerPeriod = habit.Frequency.TimesPerPeriod
            },

            Target = new TargetDto
            {
                Value = habit.Target.Value,
                Unit = habit.Target.Unit
            },

            Status = habit.Status,
            IsArchived = habit.IsArchived,
            EndDate = habit.EndDate,

            Milestone = habit.Milestone is not null
                ? new MilestoneDto
                {
                    Target = habit.Milestone.Target,
                    Current = habit.Milestone.Current
                }
                : null,

            CreatedAtUtc = habit.CreatedAtUtc,
            UpdatedAtUtc = habit.UpdatedAtUtc,
            LastCompletedAtUtc = habit.LastCompletedAtUtc
        };
    }
    public static Habit ToEntity(this CreateHabitDto dto)
    {
        DateTime now = DateTime.UtcNow;

        return new Habit
        {
            Id = $"h_{Guid.CreateVersion7()}",
            Name = dto.Name,
            Description = dto.Description,

            Type = dto.Type,

            Frequency = new Frequency
            {
                Type = dto.Frequency.Type,
                TimesPerPeriod = dto.Frequency.TimesPerPeriod
            },

            Target = new Target
            {
                Value = dto.Target.Value,
                Unit = dto.Target.Unit
            },

            Status = HabitStatus.Ongoing,
            IsArchived = false,

            EndDate = dto.EndDate,

            Milestone = dto.Milestone is not null
                ? new Milestone
                {
                    Target = dto.Milestone.Target,
                    Current = dto.Milestone.Current
                }
                : null,

            CreatedAtUtc = now,
            UpdatedAtUtc = null,
            LastCompletedAtUtc = null
        };
    }
    public static void UpdateFromDto(this Habit habit, UpdateHabitDto dto)
    {
        habit.Name = dto.Name;
        habit.Description = dto.Description;
        habit.Type = dto.Type;

        habit.Frequency = new Frequency
        {
            Type = dto.Frequency.Type,
            TimesPerPeriod = dto.Frequency.TimesPerPeriod
        };

        habit.Target = new Target
        {
            Value = dto.Target.Value,
            Unit = dto.Target.Unit
        };

        habit.EndDate = dto.EndDate;

        habit.Milestone = dto.Milestone is not null
            ? new Milestone
            {
                Target = dto.Milestone.Target,
                Current = habit.Milestone?.Current ?? 0
            }
            : null;

        habit.UpdatedAtUtc = DateTime.UtcNow;
    }
}

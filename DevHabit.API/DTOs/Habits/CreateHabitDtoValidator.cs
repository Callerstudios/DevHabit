using DevHabit.API.Entities;
using FluentValidation;

namespace DevHabit.API.DTOs.Habits;

public sealed class CreateHabitDtoValidator : AbstractValidator<CreateHabitDto>
{
    private static readonly string[] AllowedUnits =
    [
        "count",
        "minutes",
        "hours",
        "kilometers",
        "meters",
        "calories"
    ];

    private static readonly string[] AllowedUnitsForBinaryHabits =
    [
        "sessions",
        "tasks"
    ];

    public CreateHabitDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 100)
            .WithMessage("Habit name must be between 3 and 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.Frequency)
            .NotNull();

        RuleFor(x => x.Target)
            .NotNull();

        RuleFor(x => x.Target.Unit)
            .Must(unit => AllowedUnits.Contains(unit))
            .WithMessage("Invalid target unit.");

        When(x => x.Type == HabitType.Binary, () =>
        {
            RuleFor(x => x.Target.Unit)
                .Must(unit => AllowedUnitsForBinaryHabits.Contains(unit))
                .WithMessage("Invalid target unit for a binary habit.");
        });

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.EndDate.HasValue);
    }
}

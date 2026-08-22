using DevHabit.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.API.DTOs.Habits
{
    public sealed record HabitsQueryParameters
    {
        [FromQuery(Name = "q")]
        public string? Search { get; set; }
        public HabitType? Type { get; init; }
        public HabitStatus? Status { get; init; }
        public string? Sort { get; init; }
    }
}

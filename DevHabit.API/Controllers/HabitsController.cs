using System.Collections;
using System.Dynamic;
using System.Linq.Expressions;
using DevHabit.API.Database;
using DevHabit.API.DTOs.Common;
using DevHabit.API.DTOs.Habits;
using DevHabit.API.Entities;
using DevHabit.API.Services;
using DevHabit.API.Services.Sorting;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevHabit.API.Controllers;

[ApiController]
[Route("habits")]
public sealed class HabitsController(ApplicationDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService)
    {
        if (!sortMappingProvider.Validatemappings<HabitDto, Habit>(query.Sort))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided sort parameter is not valid: '{query.Sort}'");
        }

        if (!dataShapingService.Validate<HabitDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shape fields is not valid: '{query.Fields}'");
        }

        query.Search = query.Search?.Trim().ToLowerInvariant();

        IReadOnlyList<SortMapping> sortMappings = sortMappingProvider.GetMappings<HabitDto, Habit>();

        IQueryable<HabitDto> habitsQuery = dbContext.Habits
                .Where(h => query.Search == null || h.Name.ToLower().Contains(query.Search) ||
                        h.Description != null && h.Description.ToLower().Contains(query.Search))
                .Where(h => query.Type == null || h.Type == query.Type)
                .Where(h => query.Status == null || h.Status == query.Status)
                .ApplySort(query.Sort, sortMappings.ToArray())
                .Select(HabitQueries.ProjectToDto());

        int totalCount = await habitsQuery.CountAsync();
        IReadOnlyCollection<HabitDto> habits = await habitsQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();
        var paginationResult = new PaginationResult<ExpandoObject>
        {
            Items = dataShapingService.ShapeDataCollection(habits, query.Fields),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalCount
        };
        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHabit(string id, string? fields, DataShapingService dataShapingService)
    {
        if (!dataShapingService.Validate<HabitDto>(fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shape fields is not valid: '{fields}'");
        }
        HabitwithTagsDto? habit = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto())
            .FirstOrDefaultAsync();
        if (habit == null)
        {
            return NotFound();
        }
        return Ok(habit);
    }

    [HttpPost]
    public async Task<ActionResult> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        await validator.ValidateAndThrowAsync(createHabitDto);
        Habit habit = createHabitDto.ToEntity();
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();
        HabitDto habitDto = habit.ToDto();
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        habit.UpdateFromDto(updateHabitDto);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult> PatchHabit(string id, JsonPatchDocument<HabitDto> patchDocument)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        HabitDto habitDto = habit.ToDto();
        patchDocument.ApplyTo(habitDto, ModelState);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        habit.Name = habitDto.Name;
        habit.Description = habitDto.Description;
        habit.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHabit(string id)
    {
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null)
        {
            return NotFound();
        }
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}

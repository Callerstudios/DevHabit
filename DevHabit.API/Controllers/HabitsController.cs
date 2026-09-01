using System.Dynamic;
using System.Net.Mime;
using Asp.Versioning;
using DevHabit.API.Database;
using DevHabit.API.DTOs.Common;
using DevHabit.API.DTOs.Habits;
using DevHabit.API.Entities;
using DevHabit.API.Services;
using DevHabit.API.Services.Sorting;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DevHabit.API.Controllers;

[Authorize]
[ApiController]
[Route("habits")]
[ApiVersion(1.0)]
[Produces(
    MediaTypeNames.Application.Json,
    CustomMediaTypeNames.Application.JsonV1,
    CustomMediaTypeNames.Application.JsonV2,
    CustomMediaTypeNames.Application.HateoasJson,
    CustomMediaTypeNames.Application.HateoasJsonV1,
    CustomMediaTypeNames.Application.HateoasJsonV2)]
public sealed class HabitsController(ApplicationDbContext dbContext, 
    LinkService linkService,
    UserContext userContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits(
        [FromQuery] HabitsQueryParameters query,
        SortMappingProvider sortMappingProvider,
        DataShapingService dataShapingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
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
                .Where(h => h.UserId == userId)
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
            Items = dataShapingService.ShapeDataCollection(habits, 
            query.Fields, 
            query.IncludeLinks ? h => CreateLinksForHabit(h.Id, query.Fields) : null),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = totalCount
        };
        if (query.IncludeLinks)
        {
            paginationResult.Links = CreateLinksForHabits(query, paginationResult.HasNextPage, paginationResult.HasPreviousPage);
        }
        return Ok(paginationResult);
    }

    [HttpGet("{id}")]
    [MapToApiVersion(1.0)]
    public async Task<IActionResult> GetHabit(string id,
        [FromQuery] HabitsQueryParameters query,
        DataShapingService dataShapingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        if (!dataShapingService.Validate<HabitwithTagsDto>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shape fields is not valid: '{query.Fields}'");
        }

        HabitwithTagsDto? habit = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDto())
            .FirstOrDefaultAsync();
        if (habit == null || id != userId)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, query.Fields);

        if (query.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForHabit(id, query.Fields);

            shapedHabitDto.TryAdd("links", links);
        }

        return Ok(shapedHabitDto);
    }
    [HttpGet("{id}")]
    [MapToApiVersion(2.0)]
    public async Task<IActionResult> GetHabitV2(string id,
        [FromQuery] HabitsQueryParameters query,
        DataShapingService dataShapingService)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        if (!dataShapingService.Validate<HabitwithTagsDtoV2>(query.Fields))
        {
            return Problem(
                statusCode: StatusCodes.Status400BadRequest,
                detail: $"The provided data shape fields is not valid: '{query.Fields}'");
        }

        HabitwithTagsDtoV2? habit = await dbContext.Habits
            .Where(h => h.Id == id)
            .Select(HabitQueries.ProjectToHabitWithTagsDtoV2())
            .FirstOrDefaultAsync();
        if (habit == null || id != userId)
        {
            return NotFound();
        }

        ExpandoObject shapedHabitDto = dataShapingService.ShapeData(habit, query.Fields);

        if (query.IncludeLinks)
        {
            List<LinkDto> links = CreateLinksForHabit(id, query.Fields);

            shapedHabitDto.TryAdd("links", links);
        }

        return Ok(shapedHabitDto);
    }

    [HttpPost]
    public async Task<ActionResult> CreateHabit(CreateHabitDto createHabitDto, IValidator<CreateHabitDto> validator)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        ValidationResult result = await validator.ValidateAsync(createHabitDto);

        if (!result.IsValid)
        {
            var problemDetails = new ValidationProblemDetails(result.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred."
            };

            return BadRequest(problemDetails);
        }
        Habit habit = createHabitDto.ToEntity(userId);
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync();
        HabitDto habitDto = habit.ToDto();
        habitDto.Links = CreateLinksForHabit(habit.Id, null);
        return CreatedAtAction(nameof(GetHabit), new { id = habitDto.Id }, habitDto);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateHabit(string id, UpdateHabitDto updateHabitDto)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null || id != userId)
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
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        Habit? habit = await dbContext.Habits.FirstOrDefaultAsync(h => h.Id == id);
        if (habit is null || id != userId)
        {
            return NotFound();
        }
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private List<LinkDto> CreateLinksForHabit(string id, string? fields)
    {
        LinkDto[] links =  {
            linkService.Create(nameof(GetHabit), "self", HttpMethods.Get, new { id, fields }),
            linkService.Create(nameof(UpdateHabit), "update", HttpMethods.Put, new { id }),
            linkService.Create(nameof(PatchHabit), "partial-update", HttpMethods.Patch, new { id }),
            linkService.Create(nameof(DeleteHabit), "delete", HttpMethods.Delete, new { id }),
            linkService.Create(nameof(HabitTagsController.UpsertHabitTags), "upsert-tags", HttpMethods.Put, new { habitId = id }, HabitTagsController.Name),
        };
        return links.ToList();
    }
    private List<LinkDto> CreateLinksForHabits(HabitsQueryParameters parameters,
        bool hasNextPage,
        bool hasPrevPage)
    {
        List<LinkDto> links =
        [
            linkService.Create(nameof(GetHabits), "self", HttpMethods.Get, new
            {
                page = parameters.Page,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status,
            }),
            linkService.Create(nameof(CreateHabit), "create", HttpMethods.Post),
        ];
        if (hasNextPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "next-page", HttpMethods.Get, new
            {
                page = parameters.Page + 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status,
            }));
        }
        if (hasPrevPage)
        {
            links.Add(linkService.Create(nameof(GetHabits), "previous-page", HttpMethods.Get, new
            {
                page = parameters.Page - 1,
                pageSize = parameters.PageSize,
                fields = parameters.Fields,
                q = parameters.Search,
                sort = parameters.Sort,
                type = parameters.Type,
                status = parameters.Status,
            }));
        }
        return links;
    }
}

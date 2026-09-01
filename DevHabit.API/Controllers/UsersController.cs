using System.Security.Claims;
using DevHabit.API.Database;
using DevHabit.API.DTOs.Users;
using DevHabit.API.Entities;
using DevHabit.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Controllers;

[Authorize(Roles = $"{Roles.Member}")]
[Authorize]
[ApiController]
[Route("users")]
public sealed class UsersController(ApplicationDbContext dbContext, UserContext userContext): ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        UserDto? user = await dbContext.Users
            .Where(x => x.Id == id)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if(user is null || user.Id != userId)
        {
            return NotFound();
        }
        return Ok(user);
    }
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        string? userId = await userContext.GetUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        if (User.IsInRole(Roles.Member))
        {
            Console.WriteLine("User is a member");
        }

        UserDto? user = await dbContext.Users
            .Where(x => x.Id == userId)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if(user is null)
        {
            return NotFound();
        }
        if (User.IsInRole(Roles.Member))
        {
            return Ok("Member");
        }
        return Ok(user);
    }
}

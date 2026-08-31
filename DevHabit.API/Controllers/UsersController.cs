using DevHabit.API.Database;
using DevHabit.API.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Controllers;

[Authorize]
[ApiController]
[Route("users")]
public sealed class UsersController(ApplicationDbContext dbContext): ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(string id)
    {
        UserDto? user = await dbContext.Users
            .Where(x => x.Id == id)
            .Select(UserQueries.ProjectToDto())
            .FirstOrDefaultAsync();

        if(user is null)
        {
            return NotFound();
        }
        return Ok(user);
    }
}

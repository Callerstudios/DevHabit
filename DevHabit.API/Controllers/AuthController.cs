using DevHabit.API.Database;
using DevHabit.API.DTOs.Auth;
using DevHabit.API.DTOs.Users;
using DevHabit.API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DevHabit.API.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext applicationDbContext) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
    {
        using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();
        applicationDbContext.Database.SetDbConnection(identityDbContext.Database.GetDbConnection());
        await applicationDbContext.Database.UseTransactionAsync(transaction.GetDbTransaction());

        IdentityUser identityUser = new IdentityUser
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.Name
        };
        IdentityResult identityResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);
        if (!identityResult.Succeeded)
        {
            var extensions = new Dictionary<string, object?>
            {
                {
                    "errors", identityResult.Errors 
                }
            };
            return Problem(
                detail: "Unable to register the user, please try again",
                statusCode: StatusCodes.Status400BadRequest,
                extensions: extensions);
        }
        // Create App User
        User user = registerUserDto.ToEntity();

        user.IdentityId = identityUser.Id;

        applicationDbContext.Users.Add(user);

        await applicationDbContext.SaveChangesAsync();

        await transaction.CommitAsync();

        return Ok(user.Id);
    }
}

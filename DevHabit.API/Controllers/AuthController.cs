using DevHabit.API.Database;
using DevHabit.API.DTOs.Auth;
using DevHabit.API.DTOs.Users;
using DevHabit.API.Entities;
using DevHabit.API.Services;
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
    ApplicationDbContext applicationDbContext,
    TokenProvider tokenProvider) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto registerUserDto)
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

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email);
        var accessTokens = tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);
    }
    [HttpPost("login")]
    public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto loginUserDto)
    {
        IdentityUser? identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);

        if(identityUser is null || !await userManager.CheckPasswordAsync(identityUser, loginUserDto.Password))
        {
            return Unauthorized();
        }
        TokenRequest tokenRequest = new(identityUser.Id, identityUser.Email!);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        return Ok(accessTokens);
    }
}

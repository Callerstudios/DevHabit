using DevHabit.API.Database;
using DevHabit.API.DTOs.Auth;
using DevHabit.API.DTOs.Users;
using DevHabit.API.Entities;
using DevHabit.API.Services;
using DevHabit.API.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;

namespace DevHabit.API.Controllers;

[ApiController]
[Route("auth")]
[AllowAnonymous]
public sealed class AuthController(UserManager<IdentityUser> userManager,
    ApplicationIdentityDbContext identityDbContext,
    ApplicationDbContext applicationDbContext,
    TokenProvider tokenProvider,
    IOptions<JwtAuthOptions> options) : ControllerBase
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

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

        var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email);
        var accessTokens = tokenProvider.Create(tokenRequest);

        RefreshToken refreshToken = new()
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.Refreshtoken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays),
            User = identityUser
        };
        identityDbContext.RefreshTokens.Add(refreshToken);

        await identityDbContext.SaveChangesAsync();
        await transaction.CommitAsync();

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
        Console.WriteLine($"login Id: {identityUser.Id} Email: {identityUser.Email}");
        TokenRequest tokenRequest = new(identityUser.Id, identityUser.Email!);
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        RefreshToken refreshToken = new()
        {
            Id = Guid.CreateVersion7(),
            UserId = identityUser.Id,
            Token = accessTokens.Refreshtoken,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays),
            User = identityUser
        };
        identityDbContext.RefreshTokens.Add(refreshToken);
        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokensDto>> Refresh(RefreshTokenDto refreshTokenDto)
    {
        RefreshToken? refreshToken = await identityDbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == refreshTokenDto.RefreshToken);
        if(refreshToken is null || (refreshToken.ExpiresAtUtc < DateTime.UtcNow))
        {
            return Unauthorized();
        }
        TokenRequest tokenRequest = new(refreshToken.User.Id, refreshToken.User.Email!);
        Console.WriteLine($"Id: {tokenRequest.UserId} Email: {tokenRequest.Email}");
        AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

        refreshToken.Token = accessTokens.Refreshtoken;
        refreshToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(_jwtAuthOptions.RefreshTokenExpirationDays);

        await identityDbContext.SaveChangesAsync();

        return Ok(accessTokens);
    }
}

using DevHabit.API.DTOs.Common;
using DevHabit.API.DTOs.GitHub;
using DevHabit.API.Entities;
using DevHabit.API.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DevHabit.API.Controllers;

[EnableRateLimiting("default")]
[Authorize(Roles = Roles.Member)]
[Route("github")]
[ApiController]
public class GitHubController(
    GitHubAccessTokenService gitHubAccessTokenService,
    RefitGitHubService gitHubService,
    UserContext userContext,
    LinkService linkService) : ControllerBase
{
    [HttpPut("personal-access-token")]
    public async Task<IActionResult> StoreAccessToken(StoreGitHubAccessTokenDto storeGitHubAccessTokenDto, IValidator<StoreGitHubAccessTokenDto> validator)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            Console.WriteLine($"Access Denied for user");
            return Unauthorized();
        }
        ValidationResult validationResult = await validator.ValidateAsync(storeGitHubAccessTokenDto);
        if (!validationResult.IsValid)
        {
            ValidationProblemDetails validationProblemDetails = new ValidationProblemDetails();
            foreach (var error in validationResult.Errors)
            {
                validationProblemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
            }
            return BadRequest(validationProblemDetails);
        }
        Console.WriteLine($"Storing GitHub access token for user {userId}");
        await gitHubAccessTokenService.StoreAsync(userId, storeGitHubAccessTokenDto);
        return NoContent();
    }
    [HttpDelete("personal-access-token")]
    public async Task<IActionResult> RevokeAccessToken()
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        await gitHubAccessTokenService.RevokeAsync(userId);
        return NoContent();
    }
    [HttpGet("profile")]
    public async Task<ActionResult<GitHubUserProfileDto>> GetUserProfile([FromHeader] AcceptHeaderDto acceptHeaderDto)
    {
        string? userId = await userContext.GetUserIdAsync();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }
        string? accessToken = await gitHubAccessTokenService.GetAsync(userId);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            Console.WriteLine($"No Access token in db for user {userId}");
            return NotFound();
        }
        GitHubUserProfileDto? userProfile = await gitHubService.GetUserProfileAsync(accessToken);
        if (userProfile is null)
        {
            Console.WriteLine($"User profile not found for user {userId}");
            return NotFound();
        }
        Console.WriteLine($"Include Links: {acceptHeaderDto.IncludeLinks}");
        if (acceptHeaderDto.IncludeLinks)
        {
            userProfile.Links =
                [
                    linkService.Create(nameof(GetUserProfile), "self", HttpMethods.Get),
                linkService.Create(nameof(StoreAccessToken), "store-token", HttpMethods.Put),
                linkService.Create(nameof(RevokeAccessToken), "revoke-token", HttpMethods.Delete),
            ];
        }
        return Ok(userProfile);
    }
}

using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevHabit.API.DTOs.GitHub;
using Newtonsoft.Json;
using Refit;

namespace DevHabit.API.Services;

public sealed class RefitGitHubService(
    IGitHubApi gitHubApi,
    ILogger<GitHubService> logger)
{

    public async Task<GitHubUserProfileDto?> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
            ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        try
        {
            ApiResponse<GitHubUserProfileDto> response = await gitHubApi.GetUserProfile(accessToken, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "GitHub user profile request failed with status code {StatusCode}",
                    response.StatusCode);

                return null;
            }

            return response.Content;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Error while requesting GitHub user profile");

            return null;
        }
    }

    public async Task<IReadOnlyList<GitHubEventDto?>> GetUserEventsAsync(
        string username,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
        try
        {
            ApiResponse<List<GitHubEventDto>> response = await gitHubApi.GetUserEvents(username, accessToken, cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "GitHub user events request failed with status code {StatusCode}",
                    response.StatusCode);

                return [];
            }

            return response.Content ?? [];
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Error while requesting GitHub events for user {Username}",
                username);

            return [];
        }
    }
}

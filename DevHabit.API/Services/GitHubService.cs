using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using DevHabit.API.DTOs.GitHub;
using Newtonsoft.Json;

namespace DevHabit.API.Services;

public sealed class GitHubService(
    IHttpClientFactory httpClientFactory,
    ILogger<GitHubService> logger)
{

    public async Task<GitHubUserProfileDto?> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using HttpClient client = CreateGitHubClient(accessToken);

        try
        {
            HttpResponseMessage response = await client.GetAsync(
                "user",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "GitHub user profile request failed with status code {StatusCode}",
                    response.StatusCode);

                return null;
            }

            string content = await response.Content.ReadAsStringAsync(
                cancellationToken);

            return JsonConvert.DeserializeObject<GitHubUserProfileDto>(content);
        }
        catch (HttpRequestException exception)
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
        using HttpClient client = CreateGitHubClient(accessToken);

        try
        {
            HttpResponseMessage response = await client.GetAsync(
                $"users/{Uri.EscapeDataString(username)}/events?per_page=100",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "GitHub user events request failed with status code {StatusCode}",
                    response.StatusCode);

                return [];
            }

            string content = await response.Content.ReadAsStringAsync(
                cancellationToken);

            return JsonConvert.DeserializeObject<List<GitHubEventDto>>(content) ?? [];
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(
                exception,
                "Error while requesting GitHub events for user {Username}",
                username);

            return [];
        }
    }

    private HttpClient CreateGitHubClient(string accessToken)
    {
        var client = httpClientFactory.CreateClient("github");

        //client.BaseAddress = new Uri("https://api.github.com/");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        //client.DefaultRequestHeaders.Accept.Add(
        //    new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        //client.DefaultRequestHeaders.UserAgent.ParseAdd(
        //    "DevHabit.API");

        //client.DefaultRequestHeaders.Add(
        //    "X-GitHub-Api-Version",
        //    "2022-11-28");

        return client;
    }
}

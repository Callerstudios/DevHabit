namespace DevHabit.API.DTOs.GitHub;

public sealed record GitHubEventDto(string Id, string Type, DateTime CreatedAt, string? RepoName);

namespace DevHabit.API.DTOs.GitHub;

public sealed record GitHubUserProfileDto(
    long Id,
    string Login,
    string? Name,
    //string? AvatarUrl,
    string? Bio,
    //string? HtmlUrl,
    string? Company,
    string? Location,
    int PublicRepos,
    int Followers,
    int Following);

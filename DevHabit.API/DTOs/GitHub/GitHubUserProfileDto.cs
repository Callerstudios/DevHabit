using System.Collections.ObjectModel;
using DevHabit.API.DTOs.Common;

namespace DevHabit.API.DTOs.GitHub;

public sealed record GitHubUserProfileDto
{
    public long Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string? Name { get; init; }
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
    public string? Bio { get; init; }
    public string? Company { get; init; }
    public string? Location { get; init; }
    public int PublicRepos { get; init; }
    public int Followers { get; init; }
    public int Following { get; init; }
}

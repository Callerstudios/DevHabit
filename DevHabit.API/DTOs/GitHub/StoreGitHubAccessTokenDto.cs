namespace DevHabit.API.DTOs.GitHub
{
    public sealed record StoreGitHubAccessTokenDto(string Token, DateTime ExpiresAtUtc);
}

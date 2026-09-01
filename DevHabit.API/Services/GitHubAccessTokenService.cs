using DevHabit.API.Database;
using DevHabit.API.DTOs.GitHub;
using DevHabit.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.API.Services;

public class GitHubAccessTokenService(ApplicationDbContext dbContext, EncryptionService encryptionService)
{
    public async Task StoreAsync(
        string userId,
        StoreGitHubAccessTokenDto accessTokenDto,
        CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? existingToken = await GetAccessTokenAsync(userId, cancellationToken);

        string encryptedToken = encryptionService.Encrypt(accessTokenDto.Token);

        if (existingToken is not null)
        {
            existingToken.Token = encryptedToken;
            existingToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(accessTokenDto.ExpiresInDays);
            existingToken.CreatedAtUtc = DateTime.UtcNow;
        }
        else
        {
            var accessToken = new GitHubAccessToken
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Token = accessTokenDto.Token,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(accessTokenDto.ExpiresInDays),
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.GitHubAccessTokens.Add(accessToken);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> GetAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetAccessTokenAsync(userId, cancellationToken);

        if (accessToken is null)
        {
            Console.WriteLine($"No GitHub access token found for user {userId}");
            return null;
        }

        if (accessToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            Console.WriteLine($"GitHub access token for user {userId} has expired, revoking it");
            await RevokeAsync(userId, cancellationToken);
            return null;
        }
        string decryptedToken = encryptionService.Decrypt(accessToken.Token);
        return decryptedToken;
    }

    public async Task RevokeAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var accessToken = await GetAccessTokenAsync(userId, cancellationToken);

        if (accessToken is null)
        {
            return;
        }

        dbContext.GitHubAccessTokens.Remove(accessToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<GitHubAccessToken?> GetAccessTokenAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<GitHubAccessToken>()
            .FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken);
    }
}

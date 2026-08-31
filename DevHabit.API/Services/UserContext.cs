using DevHabit.API.Database;
using DevHabit.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DevHabit.API.Services;

public sealed class UserContext(
    IHttpContextAccessor httpContextAccessor,
    ApplicationDbContext dbContext,
    IMemoryCache memoryCache)
{
    private const string CacheKeyPrefix = "user-context:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<string?> GetUserIdAsync(
        CancellationToken cancellationToken = default)
    {
        string? identityid = httpContextAccessor.HttpContext?.User
            .GetIdentityId();

        if (identityid is null)
        {
            return null;
        }

        var cacheKey = $"{CacheKeyPrefix}{identityid}";

        string? userid = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SetSlidingExpiration(CacheDuration);

            string? userId = await dbContext.Users
                .Where(u => u.IdentityId == identityid)
                .Select(u => u.Id)
                .FirstOrDefaultAsync(cancellationToken);
            return userId;
        });

        return userid;
    }
}

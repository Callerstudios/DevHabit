using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace DevHabit.API.Services;

public sealed class InMemoryETagStore
{
    private static readonly ConcurrentDictionary<string, string> ETags = [];

    public string GetETag(Uri resourceUri)
    {
        return ETags.GetOrAdd(resourceUri.ToString(), _ => string.Empty);
    }

    public string GetETag(string resourceUri)
    {
        return ETags.GetOrAdd(resourceUri, _ => string.Empty);
    }

    public void SetETag(Uri resourceUri, string etag)
    {
        ETags.AddOrUpdate(resourceUri.ToString(), etag, (_, _) => etag);
    }

    public void SetETag(Uri resourceUri, object resource)
    {
        ETags.AddOrUpdate(resourceUri.ToString(), GenerateEtag(resource), (_, _) => GenerateEtag(resource));
    }

    public void SetETag(string resourceUri, string etag)
    {
        ETags.AddOrUpdate(resourceUri, etag, (_, _) => etag);
    }

    public void SetETag(string resourceUri, object resource)
    {
        ETags.AddOrUpdate(resourceUri, GenerateEtag(resource), (_, _) => GenerateEtag(resource));
    }

    public void RemoveETag(Uri resourceUri)
    {
        ETags.TryRemove(resourceUri.ToString(), out _);
    }

    public void RemoveETag(string resourceUri)
    {
        ETags.TryRemove(resourceUri, out _);
    }

    public static string GenerateEtag(object resource)
    {
        byte[] content = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(resource));
        byte[] hash = SHA256.HashData(content);

        return Convert.ToHexString(hash);
    }
}

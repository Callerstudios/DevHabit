using System.Buffers;
using System.Security.Cryptography;
using System.Text;
using DevHabit.API.Services;

namespace DevHabit.API.Middlewares;

public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseEtagCaching(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ETagMiddleware>();
    }
}

public sealed class ETagMiddleware(RequestDelegate next)
{
    private static readonly string[] ConcurrencyCheckMethods =
    [
        HttpMethods.Patch,
        HttpMethods.Put,
    ];

    public async Task InvokeAsync(
        HttpContext context,
        InMemoryETagStore etagStore)
    {
        if (CanSkipETag(context))
        {
            await next(context);
            return;
        }

        string resourcePath = context.Request.Path.Value ?? "/";
        Uri resourceUri = new(resourcePath, UriKind.Relative);

        string? clientETag = context.Request.Headers.IfNoneMatch.FirstOrDefault()?.Trim('"');
        string? ifMatch = context.Request.Headers.IfMatch.FirstOrDefault()?.Trim('"');

        if (ConcurrencyCheckMethods.Contains(context.Request.Method) && !string.IsNullOrWhiteSpace(ifMatch))
        {
            string currentETag = etagStore.GetETag(resourceUri);

            if (!string.IsNullOrWhiteSpace(currentETag) && ifMatch != currentETag)
            {
                context.Response.StatusCode = StatusCodes.Status412PreconditionFailed;
                context.Response.ContentLength = 0;
                return;
            }
        }

        Stream originalStream = context.Response.Body;
        using var memoryStream = new MemoryStream();

        try
        {
            context.Response.Body = memoryStream;

            await next(context);

            if (IsETaggableResponse(context))
            {
                memoryStream.Position = 0;

                byte[] responseBody = await GetResponseBodyAsync(memoryStream);
                string etag = GenerateEtag(responseBody);

                etagStore.SetETag(resourceUri, etag);
                context.Response.Headers.ETag = $"\"{etag}\"";

                if (context.Request.Method == HttpMethods.Get && clientETag is not null && clientETag == etag)
                {
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    context.Response.Body = originalStream;
                    context.Response.ContentLength = 0;
                    return;
                }
            }

            context.Response.Body = originalStream;
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalStream);
        }
        catch
        {
            context.Response.Body = originalStream;
            throw;
        }
    }

    private static bool CanSkipETag(HttpContext context)
    {
        return context.Request.Method == HttpMethods.Post ||
            context.Request.Method == HttpMethods.Delete;
    }

    private static bool IsETaggableResponse(HttpContext context)
    {
        return context.Response.StatusCode == StatusCodes.Status200OK &&
            (context.Response.Headers.ContentType.FirstOrDefault()?.Contains("json", StringComparison.OrdinalIgnoreCase) ?? false);
    }

    private static string GenerateEtag(byte[] content)
    {

        byte[] hash = SHA512.HashData(content);
        return Convert.ToHexString(hash);
    }
    private static async Task<byte[]> GetResponseBodyAsync(MemoryStream memoryStream)
    {
        using var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
        memoryStream.Position = 0;
        string content = await reader.ReadToEndAsync();
        return Encoding.UTF8.GetBytes(content);
    }
}

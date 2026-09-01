using System.Net.Http.Headers;
using DevHabit.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DevHabit.API.DTOs.Common;

public record AcceptHeaderDto
{
    [FromHeader(Name = "Accept")]
    public string? Accept { get; init; }

    public bool IncludeLinks =>
    MediaTypeHeaderValue.TryParse(Accept, out MediaTypeHeaderValue? mediaType) &&
    string.Equals(
        mediaType.MediaType,
        CustomMediaTypeNames.Application.HateoasJson,
        StringComparison.OrdinalIgnoreCase);
}

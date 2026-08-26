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
    mediaType.MediaType?
        .Split('/', 2)
        .ElementAtOrDefault(1)?
        .Contains(CustomMediaTypeNames.Application.HateoasSubType, StringComparison.OrdinalIgnoreCase) == true;
}

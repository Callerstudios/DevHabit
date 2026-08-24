using DevHabit.API.DTOs.Common;

namespace DevHabit.API.DTOs.Tags;

public sealed record TagsCollectionDto: ICollectionResponse<TagDto>
{
    public required IReadOnlyCollection<TagDto> Items { get; init; }
}

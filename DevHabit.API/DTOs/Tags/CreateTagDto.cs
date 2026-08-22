using System.ComponentModel.DataAnnotations;

namespace DevHabit.API.DTOs.Tags;

public sealed record CreateTagDto
{
    [Required]
    [MinLength(3)]
    public required string Name { get; init; }
    [MaxLength(50)]
    public string? Description { get; init; }
}

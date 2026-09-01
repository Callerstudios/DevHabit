using FluentValidation;

namespace DevHabit.API.DTOs.GitHub;

public sealed class StoreGitHubAccessTokenDtoValidator
: AbstractValidator<StoreGitHubAccessTokenDto>
{
    public StoreGitHubAccessTokenDtoValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();
        RuleFor(x => x.ExpiresInDays)
            .GreaterThan(0)
            .WithMessage("The expiration days must be a positive integer.");
    }
}

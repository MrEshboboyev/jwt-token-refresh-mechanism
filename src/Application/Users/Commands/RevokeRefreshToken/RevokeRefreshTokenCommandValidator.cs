using FluentValidation;

namespace Application.Users.Commands.RevokeRefreshToken;

internal class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator()
    {
        RuleFor(obj => obj.RefreshToken).NotEmpty();
    }
}
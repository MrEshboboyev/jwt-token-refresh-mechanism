using FluentValidation;

namespace Application.Users.Commands.RefreshToken;

internal class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(obj => obj.RefreshToken).NotEmpty();
    }
}
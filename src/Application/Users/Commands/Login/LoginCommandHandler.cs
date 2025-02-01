using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Users.Common;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Domain.Shared;
using Domain.ValueObjects;

namespace Application.Users.Commands.Login;

internal sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : ICommandHandler<LoginCommand, LoginResponse>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand request,
        CancellationToken cancellationToken)
    {
        var (email, password) = request;
        
        #region Checking user exists by this email and credentials valid

        // Validate and create the Email value object
        var createEmailResult = Email.Create(email);
        if (createEmailResult.IsFailure)
        {
            return Result.Failure<LoginResponse>(
                createEmailResult.Error);
        }
        
        // Retrieve the user by email
        var user = await userRepository.GetByEmailAsync(
            createEmailResult.Value,
            cancellationToken);
        
        // Verify if user exists and the password matches
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(
                DomainErrors.User.InvalidCredentials);
        }

        #endregion
        
        #region Generate token

        // Generate a JWT token for the authenticated user
        var accessToken = jwtProvider.Generate(user);

        #endregion
        
        #region Generate refresh token
        
        var refreshToken = tokenService.CreateRefreshToken(
            user.Id,
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken(refreshToken.Value);
        
        #endregion

        return Result.Success(new LoginResponse(accessToken, refreshToken.Value.Token));
    }
}
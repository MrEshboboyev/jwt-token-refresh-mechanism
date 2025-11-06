using Application.Abstractions.Authentication;
using Application.Abstractions.Logging;
using Application.Abstractions.Messaging;
using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.Users.Common;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Application.Users.Commands.Login;

internal sealed class LoginCommandHandler(
    IJwtProvider jwtProvider,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    IClientInfoService clientInfoService,
    IConcurrentLoginService concurrentLoginService,
    IOptions<TokenPolicyOptions> tokenPolicyOptions,
    ITokenLogger tokenLogger
) : ICommandHandler<LoginCommand, LoginResponse>
{
    private readonly TokenPolicyOptions _tokenPolicyOptions = tokenPolicyOptions.Value;

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var (email, password) = request;
        
        #region Checking user exists by this email and credentials valid

        // Validate and create the Email value object
        var createEmailResult = Email.Create(email);
        if (createEmailResult.IsFailure)
        {
            tokenLogger.LogSuspiciousActivity(Guid.Empty, "login", "Invalid email format", clientInfoService.GetIpAddress());
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
            tokenLogger.LogSuspiciousActivity(user?.Id ?? Guid.Empty, "login", "Invalid credentials", clientInfoService.GetIpAddress());
            return Result.Failure<LoginResponse>(
                DomainErrors.User.InvalidCredentials);
        }

        #endregion
        
        #region Check concurrent login limits
        
        if (_tokenPolicyOptions.EnableConcurrentLoginDetection)
        {
            var activeSessionCount = user.RefreshTokens.Count(rt => rt.IsActive);
            if (activeSessionCount >= _tokenPolicyOptions.MaxConcurrentSessionsPerUser)
            {
                tokenLogger.LogConcurrentSessionLimitExceeded(user.Id, activeSessionCount, _tokenPolicyOptions.MaxConcurrentSessionsPerUser);
            }
            
            // Terminate oldest sessions if we're at the limit
            var sessionsTerminated = concurrentLoginService.TerminateOldestSessions(
                user, 
                _tokenPolicyOptions.MaxConcurrentSessionsPerUser);
                
            // Revoke the terminated tokens
            if (sessionsTerminated > 0)
            {
                userRepository.Update(user);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        
        #endregion
        
        #region Generate token

        // Generate a JWT token for the authenticated user
        var accessToken = jwtProvider.Generate(user);

        #endregion
        
        #region Generate refresh token
        
        var ipAddress = clientInfoService.GetIpAddress();
        var userAgent = clientInfoService.GetUserAgent();
        var refreshTokenValue = Guid.NewGuid().ToString();
        
        Result<Domain.Entities.RefreshToken> refreshToken;
        
        try
        {
            if (_tokenPolicyOptions.EnableSlidingWindowExpiration)
            {
                refreshToken = tokenService.CreateRefreshTokenWithPolicy(
                    user.Id,
                    refreshTokenValue,
                    ipAddress,
                    userAgent);
            }
            else
            {
                refreshToken = tokenService.CreateRefreshToken(
                    user.Id,
                    refreshTokenValue,
                    DateTime.UtcNow.AddDays(_tokenPolicyOptions.RefreshTokenLifetimeDays),
                    ipAddress,
                    userAgent);
            }
            
            if (refreshToken.IsFailure)
            {
                return Result.Failure<LoginResponse>(refreshToken.Error);
            }
        }
        catch (Exception ex)
        {
            tokenLogger.LogTokenCreationError(user.Id, ex.Message, ipAddress);
            return Result.Failure<LoginResponse>(DomainErrors.RefreshToken.CreationFailed);
        }
        
        user.AddRefreshToken(refreshToken.Value);
        tokenLogger.LogTokenCreated(user.Id, refreshToken.Value.Id.ToString(), ipAddress);
        
        #endregion
        
        #region Update database
        
        try
        {
            userRepository.Update(user);
            await refreshTokenRepository.AddAsync(refreshToken.Value, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            tokenLogger.LogDatabaseError(user.Id, "Failed to save refresh token", ex.Message);
            return Result.Failure<LoginResponse>(DomainErrors.General.DatabaseError);
        }
        
        #endregion

        return Result.Success(new LoginResponse(accessToken, refreshTokenValue));
    }
}

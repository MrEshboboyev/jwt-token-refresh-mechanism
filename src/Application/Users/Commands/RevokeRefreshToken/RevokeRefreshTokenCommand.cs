using Application.Abstractions.Messaging;

namespace Application.Users.Commands.RevokeRefreshToken;

public sealed record RevokeRefreshTokenCommand(
    string RefreshToken) : ICommand;
using Application.Abstractions.Messaging;
using Application.Users.Common;

namespace Application.Users.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    string RefreshToken) : ICommand<RefreshTokenResponse>;
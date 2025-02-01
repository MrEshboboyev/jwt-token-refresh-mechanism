using Application.Abstractions.Messaging;
using Application.Users.Common;

namespace Application.Users.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password) : ICommand<LoginResponse>;
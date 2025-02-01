using Application.Abstractions.Security;
using Application.Users.Commands.Login;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Services;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _jwtProviderMock = new Mock<IJwtProvider>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new LoginCommandHandler(
            _jwtProviderMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var user = User.Create(
            Guid.NewGuid(),
            Email.Create(email).Value,
            "hashedPassword",
            FullName.Create("John Doe").Value);

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        _jwtProviderMock
            .Setup(x => x.Generate(It.IsAny<User>()))
            .Returns("accessToken");

        _tokenServiceMock
            .Setup(x => x.CreateRefreshToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Result.Success(RefreshToken.Create(user.Id, "refreshToken", DateTime.UtcNow.AddDays(7))));

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("accessToken");
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenCredentialsAreInvalid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "WrongPassword";

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<Email>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new LoginCommand(email, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.User.InvalidCredentials);
    }
}
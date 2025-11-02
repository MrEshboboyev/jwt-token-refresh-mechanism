using Application.Abstractions.Security;
using Application.Abstractions.Services;
using Application.Users.Commands.RefreshToken;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using Domain.Shared;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Users.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _tokenServiceMock = new Mock<ITokenService>();
        _jwtProviderMock = new Mock<IJwtProvider>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new RefreshTokenCommandHandler(
            _userRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _tokenServiceMock.Object,
            _jwtProviderMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTokenIsValid()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "refreshToken", DateTime.UtcNow.AddDays(7));
        var user = User.Create(
            refreshToken.UserId,
            Email.Create("test@example.com").Value,
            "hashedPassword",
            FullName.Create("John Doe").Value);

        _refreshTokenRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)refreshToken);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtProviderMock
            .Setup(x => x.Generate(It.IsAny<User>()))
            .Returns("newAccessToken");

        _tokenServiceMock
            .Setup(x => x.CreateRefreshToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns(Result.Success(RefreshToken.Create(user.Id, "newRefreshToken", DateTime.UtcNow.AddDays(7))));

        var command = new RefreshTokenCommand("refreshToken");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("newAccessToken");
        result.Value.RefreshToken.Should().Be("newRefreshToken");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTokenIsInvalid()
    {
        // Arrange
        _refreshTokenRepositoryMock
            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var command = new RefreshTokenCommand("invalidToken");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainErrors.RefreshToken.InvalidToken);
    }
}
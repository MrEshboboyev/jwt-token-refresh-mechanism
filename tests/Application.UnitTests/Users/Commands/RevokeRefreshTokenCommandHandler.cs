//using Application.Users.Commands.RevokeRefreshToken;
//using Domain.Entities;
//using Domain.Errors;
//using Domain.Repositories;
//using Domain.ValueObjects;
//using FluentAssertions;
//using Moq;

//namespace Application.UnitTests.Users.Commands;

//public class RevokeTokenCommandHandlerTests
//{
//    private readonly Mock<IUserRepository> _userRepositoryMock;
//    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
//    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
//    private readonly RevokeRefreshTokenCommandHandler _handler;

//    public RevokeTokenCommandHandlerTests()
//    {
//        _userRepositoryMock = new Mock<IUserRepository>();
//        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
//        _unitOfWorkMock = new Mock<IUnitOfWork>();
//        _handler = new RevokeRefreshTokenCommandHandler(
//            _userRepositoryMock.Object,
//            _refreshTokenRepositoryMock.Object,
//            _unitOfWorkMock.Object);
//    }

//    [Fact]
//    public async Task Handle_ShouldReturnSuccess_WhenTokenIsRevoked()
//    {
//        // Arrange
//        var refreshToken = RefreshToken.Create(Guid.NewGuid(), "refreshToken", DateTime.UtcNow.AddDays(7));
//        var user = User.Create(
//            refreshToken.UserId,
//            Email.Create("test@example.com").Value,
//            "hashedPassword",
//            FullName.Create("John Doe").Value);
        
//        user.AddRefreshToken(refreshToken);

//        _refreshTokenRepositoryMock
//            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync((RefreshToken?)refreshToken);

//        _userRepositoryMock
//            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync(user);

//        var command = new RevokeRefreshTokenCommand("refreshToken");

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        result.IsSuccess.Should().BeTrue();
//        _userRepositoryMock.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
//        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
//    }

//    [Fact]
//    public async Task Handle_ShouldReturnFailure_WhenTokenIsInvalid()
//    {
//        // Arrange
//        _refreshTokenRepositoryMock
//            .Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
//            .ReturnsAsync((RefreshToken?)null);

//        var command = new RevokeRefreshTokenCommand("invalidToken");

//        // Act
//        var result = await _handler.Handle(command, CancellationToken.None);

//        // Assert
//        result.IsFailure.Should().BeTrue();
//        result.Error.Should().Be(DomainErrors.RefreshToken.InvalidToken);
//    }
//}
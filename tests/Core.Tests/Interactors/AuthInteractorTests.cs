using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Interactors;
using Core.Ports;
using Moq;

namespace Core.Tests.Interactors;

public class AuthInteractorTests
{
    private readonly Mock<AccessTokenService> accessTokenServiceMock = new();
    private readonly AuthInteractor authInteractor;
    private readonly Mock<AuthSessionsRepository> authSessionsRepositoryMock = new();
    private readonly Mock<PasswordVerifier> passwordVerifierMock = new();
    private readonly Mock<RefreshTokensFactory> refreshTokensFactoryMock = new();
    private readonly Mock<RefreshTokensRepository> refreshTokensRepositoryMock = new();
    private readonly Mock<UsersRepository> usersRepositoryMock = new();

    public AuthInteractorTests()
    {
        authInteractor = new AuthInteractor(
            usersRepositoryMock.Object,
            passwordVerifierMock.Object,
            refreshTokensFactoryMock.Object,
            refreshTokensRepositoryMock.Object,
            authSessionsRepositoryMock.Object,
            accessTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task LogIn_WhenUserWithGivenEmailDoesNotExist_ShouldFailAndNotPersist()
    {
        var email = "test@test.com";

        usersRepositoryMock.Setup(repo => repo.FindByEmail(email)).ReturnsAsync(null as User);

        var result = await ExecuteLogIn(email);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<User>>(result.Exception);
        refreshTokensRepositoryMock.Verify(repo => repo.Flush(), Times.Never);
    }

    [Fact]
    public async Task LogIn_WhenUserExistsButPasswordIsInvalid_ShouldFailAndNotPersist()
    {
        var user = CreateTestUser();

        usersRepositoryMock.Setup(repo => repo.FindByEmail(It.IsAny<string>())).ReturnsAsync(user);
        passwordVerifierMock
            .Setup(verifier => verifier.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var result = await ExecuteLogIn();

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidCredentials>(result.Exception);
        refreshTokensRepositoryMock.Verify(repo => repo.Flush(), Times.Never);
    }

    [Fact]
    public async Task LogIn_WhenUserExistsAndPasswordIsValid_ShouldReturnTokenPairAndPersistIt()
    {
        var plainPassword = "password";
        var user = CreateTestUser();
        var accessToken = "accessToken";
        var refreshToken = CreateTestRefreshToken(user.Id);

        usersRepositoryMock.Setup(repo => repo.FindByEmail(user.Email)).ReturnsAsync(user);

        passwordVerifierMock
            .Setup(verifier => verifier.Verify(plainPassword, user.Password))
            .Returns(true);

        refreshTokensFactoryMock
            .Setup(factory =>
                factory.Create(It.Is<AuthSession>(session => session.UserId == user.Id))
            )
            .Returns(refreshToken);

        accessTokenServiceMock
            .Setup(service =>
                service.Create(It.Is<AuthSession>(session => session.UserId == user.Id))
            )
            .Returns(accessToken);

        RefreshToken? actualRefreshToken = null;

        refreshTokensRepositoryMock
            .Setup(repo => repo.Persist(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => actualRefreshToken = token);

        var result = await ExecuteLogIn(user.Email, plainPassword);

        Assert.True(result.IsSuccess);
        Assert.Equal(accessToken, result.Value.AccessToken);
        Assert.Equal(refreshToken.Token, result.Value.RefreshToken);
        Assert.Equivalent(refreshToken, actualRefreshToken);
        refreshTokensRepositoryMock.Verify(repo => repo.Flush(), Times.AtLeastOnce);
    }

    private Task<Result<TokenPairOutput>> ExecuteLogIn(
        string email = "defaultEmail",
        string password = "defaultPassword"
    )
    {
        return authInteractor.LogIn(email, password);
    }

    private User CreateTestUser(
        string email = "defaultEmail",
        string userName = "defaultUserName",
        string password = "defaultPassword"
    )
    {
        return new User
        {
            UserName = userName,
            Email = email,
            Password = password,
        };
    }

    private RefreshToken CreateTestRefreshToken(Guid userId)
    {
        var session = new AuthSession(userId);

        return new RefreshToken(session, DateTime.MinValue, "token");
    }
}

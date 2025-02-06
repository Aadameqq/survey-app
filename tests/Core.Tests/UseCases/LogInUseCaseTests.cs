using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class LogInUseCaseTests
{
    private readonly Mock<AccessTokenService> accessTokenServiceMock = new();
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<AuthSessionsRepository> authSessionsRepositoryMock = new();
    private readonly Mock<DateTimeProvider> dateTimeProviderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string existingPlainPassword = "plain-password";
    private readonly string generatedAccessToken = "access-token";
    private readonly string generatedRefreshToken = "refresh-token";

    private readonly Mock<PasswordVerifier> passwordVerifierMock = new();
    private readonly Mock<RefreshTokensFactory> refreshTokensFactoryMock = new();

    private readonly LogInUseCase useCase;

    public LogInUseCaseTests()
    {
        useCase = new LogInUseCase(
            refreshTokensFactoryMock.Object,
            accountsRepositoryMock.Object,
            accessTokenServiceMock.Object,
            passwordVerifierMock.Object,
            dateTimeProviderMock.Object,
            authSessionsRepositoryMock.Object
        );

        accountsRepositoryMock
            .Setup(x => x.FindByEmail(existingAccount.Email))
            .ReturnsAsync(existingAccount);

        passwordVerifierMock
            .Setup(x => x.Verify(existingPlainPassword, existingAccount.Password))
            .Returns(true);

        dateTimeProviderMock.Setup(x => x.Now()).Returns(DateTime.MinValue);

        refreshTokensFactoryMock.Setup(x => x.Generate()).Returns(generatedRefreshToken);

        accessTokenServiceMock
            .Setup(x => x.Create(It.Is((AuthSession s) => IsExpectedAuthSession(s))))
            .Returns(generatedAccessToken);
    }

    [Fact]
    public async Task WhenAccountDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute("invalid-email", existingAccount.Password);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoRepositoryChanges();
    }

    [Fact]
    public async Task WhenPasswordIsInvalid_ShouldFail()
    {
        var result = await useCase.Execute(existingAccount.Email, "invalid-password");

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidCredentials>(result.Exception);
        AssertNoRepositoryChanges();
    }

    [Fact]
    public async Task WhenAccountHasNotBeenActivated_ShouldFail()
    {
        var result = await useCase.Execute(existingAccount.Email, existingPlainPassword);

        Assert.True(result.IsFailure);
        Assert.IsType<AccountNotActivated>(result.Exception);
        AssertNoRepositoryChanges();
    }

    [Fact]
    public async Task WhenCredentialsAreValidAndAccountHasBeenActivated_ShouldSucceedAndPersistSession()
    {
        existingAccount.Activate();

        var result = await useCase.Execute(existingAccount.Email, existingPlainPassword);

        Assert.True(result.IsSuccess);
        Assert.Same(generatedAccessToken, result.Value.AccessToken);
        Assert.Same(generatedRefreshToken, result.Value.RefreshToken);

        authSessionsRepositoryMock.Verify(
            x => x.Create(It.Is((AuthSession s) => IsExpectedAuthSession(s))),
            Times.Once
        );
        authSessionsRepositoryMock.Verify(x => x.Flush(), Times.AtLeastOnce);
    }

    private void AssertNoRepositoryChanges()
    {
        accountsRepositoryMock.Verify(x => x.Flush(), Times.Never);
        authSessionsRepositoryMock.Verify(x => x.Flush(), Times.Never);
    }

    private bool IsExpectedAuthSession(AuthSession authSession)
    {
        return authSession.CurrentToken == generatedRefreshToken
            && authSession.UserId == existingAccount.Id;
    }
}

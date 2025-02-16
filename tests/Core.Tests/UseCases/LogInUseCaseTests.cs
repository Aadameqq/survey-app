using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class LogInUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<DateTimeProvider> dateTimeProviderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string existingPlainPassword = "plain-password";
    private readonly string generatedAccessToken = "access-token";
    private readonly string generatedRefreshToken = "refresh-token";

    private readonly Mock<PasswordVerifier> passwordVerifierMock = new();
    private readonly Mock<TokenService> tokenServiceMock = new();

    private readonly LogInUseCase useCase;

    public LogInUseCaseTests()
    {
        useCase = new LogInUseCase(
            accountsRepositoryMock.Object,
            tokenServiceMock.Object,
            passwordVerifierMock.Object,
            dateTimeProviderMock.Object
        );

        accountsRepositoryMock
            .Setup(x => x.FindByEmail(existingAccount.Email))
            .ReturnsAsync(existingAccount);

        passwordVerifierMock
            .Setup(x => x.Verify(existingPlainPassword, existingAccount.Password))
            .Returns(true);

        dateTimeProviderMock.Setup(x => x.Now()).Returns(DateTime.MinValue);

        tokenServiceMock
            .Setup(x => x.CreateRefreshToken(existingAccount))
            .Returns(generatedRefreshToken);

        tokenServiceMock
            .Setup(x => x.CreateAccessToken(existingAccount, It.IsAny<Guid>()))
            .Returns(generatedAccessToken);
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute("invalid-email", existingAccount.Password);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenPasswordIsInvalid_ShouldFail()
    {
        var result = await useCase.Execute(existingAccount.Email, "invalid-password");

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidCredentials>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenAccountHasNotBeenActivated_ShouldFail()
    {
        var result = await useCase.Execute(existingAccount.Email, existingPlainPassword);

        Assert.True(result.IsFailure);
        Assert.IsType<AccountNotActivated>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenCredentialsAreValidAndAccountHasBeenActivated_ShouldReturnCorrectTokens()
    {
        existingAccount.Activate();

        var result = await useCase.Execute(existingAccount.Email, existingPlainPassword);

        Assert.True(result.IsSuccess);
        Assert.Same(generatedAccessToken, result.Value.AccessToken);
        Assert.Same(generatedRefreshToken, result.Value.RefreshToken);
    }

    [Fact]
    public async Task WhenCredentialsAreValidAndAccountHasBeenActivated_UpdateAccountAndCreateSession()
    {
        existingAccount.Activate();

        Account actualAccount = null!;

        accountsRepositoryMock
            .Setup(r => r.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        var sessionId = Guid.Empty;

        tokenServiceMock
            .Setup(t => t.CreateAccessToken(It.IsAny<Account>(), It.IsAny<Guid>()))
            .Callback((Account _, Guid s) => sessionId = s);

        await useCase.Execute(existingAccount.Email, existingPlainPassword);

        Assert.NotNull(actualAccount);
        Assert.Equal(existingAccount.Id, actualAccount.Id);
        Assert.NotNull(actualAccount.GetSessionId(generatedRefreshToken));
        Assert.Equal(actualAccount.GetSessionId(generatedRefreshToken), sessionId);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(x => x.UpdateAndFlush(It.IsAny<Account>()), Times.Never);
    }
}

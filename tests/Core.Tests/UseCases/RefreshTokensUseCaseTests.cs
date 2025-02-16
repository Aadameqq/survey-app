using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class RefreshTokensUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<ArchivedTokensRepository> archivedTokensRepositoryMock = new();
    private readonly Mock<DateTimeProvider> dateTimeProviderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly ArchivedToken existingArchivedToken = new("archived-token", Guid.NewGuid());
    private readonly string generatedAccessToken = "access-token";
    private readonly string generatedRefreshToken = "refresh-token";

    private readonly Mock<TokenService> tokenServiceMock = new();

    private readonly RefreshTokensUseCase useCase;

    public RefreshTokensUseCaseTests()
    {
        useCase = new RefreshTokensUseCase(
            accountsRepositoryMock.Object,
            dateTimeProviderMock.Object,
            tokenServiceMock.Object,
            archivedTokensRepositoryMock.Object
        );
        existingAccount.Activate();

        accountsRepositoryMock
            .Setup(r => r.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);

        archivedTokensRepositoryMock
            .Setup(r => r.FindByToken(existingArchivedToken.Token))
            .ReturnsAsync(existingArchivedToken);

        tokenServiceMock
            .Setup(s => s.CreateRefreshToken(It.Is((Account a) => a.Id == existingAccount.Id)))
            .Returns(generatedRefreshToken);

        dateTimeProviderMock.Setup(p => p.Now()).Returns(DateTime.MinValue);
    }

    [Fact]
    public async Task ShouldFail_WhenAccountWithGivenIdDoesNotExist()
    {
        var result = await useCase.Execute(Guid.Empty, "token");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountExistsAndSessionWithGivenTokenDoesNotExistAndTokenIsNotArchived()
    {
        var result = await useCase.Execute(existingAccount.Id, "invalid-token");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountExistsAndTokenIsArchived()
    {
        var result = await useCase.Execute(existingAccount.Id, existingArchivedToken.Token);

        Assert.True(result.IsFailure);
        Assert.IsType<InvalidToken>(result.Exception);
    }

    [Fact]
    public async Task ShouldRemoveAllSessionsPersistAccount_WhenAccountExistsAndTokenIsArchived()
    {
        var testToken = "test-token";
        CreateTestSession(testToken);

        Account actualAccount = null!;

        accountsRepositoryMock
            .Setup(r => r.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(existingAccount.Id, existingArchivedToken.Token);

        Assert.NotNull(actualAccount);
        Assert.Equal(existingAccount.Id, actualAccount.Id);
        Assert.Null(actualAccount.GetSessionId(testToken));
    }

    [Fact]
    public async Task ShouldSucceedAndReturnCorrectTokensPair_WhenAccountExistsAndTokenIsNotArchived()
    {
        var testToken = "test-token";
        var sessionId = CreateTestSession(testToken);

        tokenServiceMock
            .Setup(s =>
                s.CreateAccessToken(It.Is((Account a) => a.Id == existingAccount.Id), sessionId)
            )
            .Returns(generatedAccessToken);

        var result = await useCase.Execute(existingAccount.Id, testToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(generatedAccessToken, result.Value.AccessToken);
        Assert.Equal(generatedRefreshToken, result.Value.RefreshToken);
    }

    [Fact]
    public async Task ShouldArchiveOldToken_WhenAccountExistsAndTokenIsNotArchived()
    {
        var testToken = "test-token";
        var sessionId = CreateTestSession(testToken);

        ArchivedToken actualArchivedToken = null!;

        archivedTokensRepositoryMock
            .Setup(r => r.CreateAndFlush(It.IsAny<ArchivedToken>()))
            .Callback((ArchivedToken a) => actualArchivedToken = a);

        await useCase.Execute(existingAccount.Id, testToken);

        Assert.NotNull(actualArchivedToken);
        Assert.Equal(testToken, actualArchivedToken.Token);
        Assert.Equal(sessionId, actualArchivedToken.SessionId);
    }

    [Fact]
    public async Task ShouldChangeTokenForSessionWithGivenIdAndPersistAccount_WhenAccountExistsAndTokenIsNotArchived()
    {
        var testToken = "test-token";
        var sessionId = CreateTestSession(testToken);

        Account actualAccount = null!;

        accountsRepositoryMock
            .Setup(r => r.UpdateAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(existingAccount.Id, testToken);

        Assert.NotNull(actualAccount);
        Assert.Null(actualAccount.GetSessionId(testToken));
        Assert.Equal(sessionId, actualAccount.GetSessionId(generatedRefreshToken));
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(r => r.UpdateAndFlush(It.IsAny<Account>()), Times.Never);
        archivedTokensRepositoryMock.Verify(
            r => r.CreateAndFlush(It.IsAny<ArchivedToken>()),
            Times.Never
        );
    }

    private Guid CreateTestSession(string token)
    {
        return existingAccount.CreateSession(DateTime.MinValue, token).Value;
    }
}

using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class RefreshTokensUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<DateTimeProvider> dateTimeProviderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly RefreshToken existingRefreshToken = new("archived-token", Guid.NewGuid());
    private readonly Guid existingSessionId;
    private readonly string testToken = "valid-token";
    private readonly TokenPairOutput testTokenPair = new("access-token", "refresh-token");

    private readonly Mock<TokenService> tokenServiceMock = new();

    private readonly RefreshTokensUseCase useCase;

    public RefreshTokensUseCaseTests()
    {
        useCase = new RefreshTokensUseCase(
            accountsRepositoryMock.Object,
            dateTimeProviderMock.Object,
            tokenServiceMock.Object
        );
        existingAccount.Activate();

        accountsRepositoryMock
            .Setup(r => r.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);

        dateTimeProviderMock.Setup(p => p.Now()).Returns(DateTime.MinValue);

        existingSessionId = CreateTestSession(testToken);

        tokenServiceMock
            .Setup(s => s.FetchRefreshTokenPayloadIfValid(testToken))
            .ReturnsAsync(new RefreshTokenPayload(existingAccount.Id, existingSessionId));
    }

    [Fact]
    public async Task ShouldFail_WhenAccountWithGivenIdDoesNotExist() // TODO:
    {
        var result = await useCase.Execute("token");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountExistsAndSessionWithGivenTokenDoesNotExistAndTokenIsNotArchived()
    {
        var result = await useCase.Execute("invalid-token");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task ShouldFail_WhenAccountExistsAndTokenIsArchived()
    {
        var result = await useCase.Execute(existingRefreshToken.Token);

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

        await useCase.Execute(existingRefreshToken.Token);

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
                s.CreateTokenPair(It.Is((Account a) => a.Id == existingAccount.Id), sessionId)
            )
            .Returns(testTokenPair);

        var result = await useCase.Execute(testToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(testTokenPair.AccessToken, result.Value.AccessToken);
        Assert.Equal(testTokenPair.RefreshToken, result.Value.RefreshToken);
    }

    [Fact]
    public async Task ShouldArchiveOldToken_WhenAccountExistsAndTokenIsNotArchived()
    {
        var testToken = "test-token";
        var sessionId = CreateTestSession(testToken);

        RefreshToken actualRefreshToken = null!;

        archivedTokensRepositoryMock
            .Setup(r => r.CreateAndFlush(It.IsAny<RefreshToken>()))
            .Callback((RefreshToken a) => actualRefreshToken = a);

        await useCase.Execute(existingAccount.Id, testToken);

        Assert.NotNull(actualRefreshToken);
        Assert.Equal(testToken, actualRefreshToken.Token);
        Assert.Equal(sessionId, actualRefreshToken.SessionId);
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
            r => r.CreateAndFlush(It.IsAny<RefreshToken>()),
            Times.Never
        );
    }

    private Guid CreateTestSession(string token)
    {
        return existingAccount.CreateSession(DateTime.MinValue, token).Value;
    }
}

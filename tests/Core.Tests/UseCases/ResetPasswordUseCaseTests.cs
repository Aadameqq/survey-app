using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class ResetPasswordUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<AuthSessionsRepository> authSessionsRepositoryMock = new();
    private readonly Mock<PasswordResetCodesRepository> codesRepositoryMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string existingCode = "code";

    private readonly Mock<PasswordHasher> passwordHasherMock = new();

    private readonly string testPassword = "new-password";
    private readonly string testPasswordHashed = "new-password-hash";

    private readonly ResetPasswordUseCase useCase;

    public ResetPasswordUseCaseTests()
    {
        useCase = new(
            passwordHasherMock.Object,
            accountsRepositoryMock.Object,
            codesRepositoryMock.Object,
            authSessionsRepositoryMock.Object
        );

        codesRepositoryMock
            .Setup(x => x.GetAccountIdAndRevokeCode(existingCode))
            .ReturnsAsync(existingAccount.Id);

        accountsRepositoryMock
            .Setup(x => x.FindById(existingAccount.Id))
            .ReturnsAsync(existingAccount);

        passwordHasherMock.Setup(x => x.HashPassword(testPassword)).Returns(testPasswordHashed);
    }

    [Fact]
    public async Task WhenGivenCodeIsInvalid_ShouldFail()
    {
        var result = await useCase.Execute("invalid-code", "some-password");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenGivenCodeIsValidButAccountAssignedToItDoesNotExist_ShouldFail()
    {
        accountsRepositoryMock
            .Setup(x => x.FindById(It.IsAny<Guid>()))
            .ReturnsAsync(null as Account);

        var result = await useCase.Execute(existingCode, "some-password");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenGivenCodeIsValid_ShouldSucceed()
    {
        var result = await useCase.Execute(existingCode, "new-password");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WhenGivenCodeIsValid_ShouldChangePasswordAndPersist()
    {
        Account? actualAccount = null;

        accountsRepositoryMock
            .Setup(x => x.Update(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(existingCode, testPassword);

        existingAccount.ChangePassword(testPasswordHashed);

        Assert.Equivalent(existingAccount, actualAccount);
        accountsRepositoryMock.Verify(x => x.Flush(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task WhenGivenCodeIsValid_ShouldDestroyAllSessions()
    {
        Account? actualAccount = null;

        authSessionsRepositoryMock
            .Setup(x => x.RemoveAllByAccountAndFlush(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(existingCode, testPassword);

        Assert.Equal(existingAccount.Id, actualAccount.Id);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(x => x.Flush(), Times.Never);
        authSessionsRepositoryMock.Verify(
            x => x.RemoveAllByAccountAndFlush(It.IsAny<Account>()),
            Times.Never
        );
        codesRepositoryMock.Verify(x => x.Create(It.IsAny<Account>()), Times.Never);
    }
}

using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class InitializePasswordResetUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<PasswordResetCodesRepository> codesRepositoryMock = new();
    private readonly Mock<PasswordResetEmailSender> emailSenderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string existingCode = "code";

    private readonly InitializePasswordResetUseCase useCase;

    public InitializePasswordResetUseCaseTests()
    {
        useCase = new InitializePasswordResetUseCase(
            accountsRepositoryMock.Object,
            codesRepositoryMock.Object,
            emailSenderMock.Object
        );

        accountsRepositoryMock
            .Setup(x => x.FindByEmail(existingAccount.Email))
            .ReturnsAsync(existingAccount);

        codesRepositoryMock.Setup(x => x.Create(It.IsAny<Account>())).ReturnsAsync(existingCode);
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailDoesNotExist_ShouldFail()
    {
        var result = await useCase.Execute("invalid-email");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailExists_ShouldSucceed()
    {
        var result = await useCase.Execute(existingAccount.Email);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailExists_ShouldCreateCode()
    {
        Account? actualAccount = null;
        codesRepositoryMock
            .Setup(x => x.Create(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(existingAccount.Email);

        Assert.NotNull(actualAccount);
        Assert.Equivalent(existingAccount, actualAccount);
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailExists_ShouldSendEmail()
    {
        Account? actualAccount = null;
        var actualCode = string.Empty;

        emailSenderMock
            .Setup(x => x.Send(It.IsAny<Account>(), It.IsAny<string>()))
            .Callback(
                (Account a, string c) =>
                {
                    actualAccount = a;
                    actualCode = c;
                }
            );

        await useCase.Execute(existingAccount.Email);

        Assert.Equivalent(existingAccount, actualAccount);
        Assert.Same(existingCode, actualCode);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(x => x.Flush(), Times.Never);
        codesRepositoryMock.Verify(x => x.Create(It.IsAny<Account>()), Times.Never);
        emailSenderMock.Verify(x => x.Send(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
    }
}

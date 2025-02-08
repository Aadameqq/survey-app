using Core.Domain;
using Core.Exceptions;
using Core.Ports;
using Core.UseCases;
using Moq;

namespace Core.Tests.UseCases;

public class CreateAccountUseCaseTests
{
    private readonly Mock<AccountsRepository> accountsRepositoryMock = new();
    private readonly Mock<ActivationCodesRepository> codesRepositoryMock = new();
    private readonly Mock<ActivationCodeEmailSender> emailSenderMock = new();

    private readonly Account existingAccount = new("userName", "email", "password");
    private readonly string expectedCode = "code";

    private readonly Mock<PasswordHasher> passwordHasherMock = new();

    private readonly TestAccount testAccount = new("new-userName", "new-email", "new-password");

    private readonly CreateAccountUseCase useCase;

    public CreateAccountUseCaseTests()
    {
        useCase = new CreateAccountUseCase(
            accountsRepositoryMock.Object,
            passwordHasherMock.Object,
            codesRepositoryMock.Object,
            emailSenderMock.Object
        );

        accountsRepositoryMock
            .Setup(x => x.FindByEmail(existingAccount.Email))
            .ReturnsAsync(existingAccount);

        passwordHasherMock
            .Setup(x => x.HashPassword(It.IsAny<string>()))
            .Returns((string password) => password + "hash");

        codesRepositoryMock.Setup(x => x.Create(It.IsAny<Account>())).ReturnsAsync(expectedCode);
    }

    [Fact]
    public async Task WhenAccountWithGivenEmailAlreadyExists_ShouldFail()
    {
        var result = await useCase.Execute("test-username", existingAccount.Email, "test-password");

        Assert.True(result.IsFailure);
        Assert.IsType<AlreadyExists<Account>>(result.Exception);
        AssertNoChanges();
    }

    [Fact]
    public async Task WhenGivenEmailIsNotOccupied_ShouldSucceed()
    {
        var result = await useCase.Execute("new-userName", "new-email", "new-password");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task WhenGivenEmailIsNotOccupied_ShouldPersistAccountWithCorrectData()
    {
        Account? actualAccount = null;

        accountsRepositoryMock
            .Setup(x => x.Create(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(testAccount.UserName, testAccount.Email, testAccount.Password);

        AssertExpectedAccount(actualAccount);
    }

    [Fact]
    public async Task WhenGivenEmailIsNotOccupied_ShouldCreateCodeForCorrectAccount()
    {
        Account? actualAccount = null;

        codesRepositoryMock
            .Setup(x => x.Create(It.IsAny<Account>()))
            .Callback((Account a) => actualAccount = a);

        await useCase.Execute(testAccount.UserName, testAccount.Email, testAccount.Password);

        AssertExpectedAccount(actualAccount);
    }

    [Fact]
    public async Task WhenGivenEmailIsNotOccupied_ShouldSendEmailWithValidData()
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

        await useCase.Execute(testAccount.UserName, testAccount.Email, testAccount.Password);

        AssertExpectedAccount(actualAccount);
        Assert.Same(expectedCode, actualCode);
    }

    private void AssertExpectedAccount(Account? account)
    {
        Assert.NotNull(account);
        Assert.Equal(testAccount.UserName, account.UserName);
        Assert.Equal(testAccount.Email, account.Email);
        Assert.Equal(testAccount.Password + "hash", account.Password);
    }

    private void AssertNoChanges()
    {
        accountsRepositoryMock.Verify(x => x.Flush(), Times.Never);
        codesRepositoryMock.Verify(x => x.Create(It.IsAny<Account>()), Times.Never);
        emailSenderMock.Verify(x => x.Send(It.IsAny<Account>(), It.IsAny<string>()), Times.Never);
    }

    private record TestAccount(string UserName, string Email, string Password);
}

using Core.Domain;
using Core.Exceptions;
using Core.Interactors;
using Core.Ports;
using Moq;

namespace Core.Tests.Interactors;

public class AccountInteractorTests
{
    private readonly AccountInteractor accountInteractor;
    private readonly Mock<ActivationCodesRepository> codeRepositoryMock = new();
    private readonly Mock<ActivationCodeEmailSender> emailBodyGeneratorMock = new();
    private readonly Mock<EmailSender> emailSenderMock = new();
    private readonly Mock<PasswordHasher> passwordHasherMock = new();
    private readonly Mock<AccountsRepository> usersRepositoryMock = new();

    public AccountInteractorTests()
    {
        accountInteractor = new AccountInteractor(
            usersRepositoryMock.Object,
            passwordHasherMock.Object,
            emailSenderMock.Object,
            emailBodyGeneratorMock.Object,
            codeRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Create_WhenUserWithGivenEmailAlreadyExists_ShouldFailAndNotPersist()
    {
        var testEmail = "mail";
        usersRepositoryMock
            .Setup(repo => repo.FindByEmail(testEmail))
            .ReturnsAsync(
                new Account
                {
                    Email = testEmail,
                    UserName = "userName",
                    Password = "password",
                }
            );

        var actual = await accountInteractor.Create("userName", testEmail, "password");

        Assert.True(actual.IsFailure);
        Assert.IsType<AlreadyExists<Account>>(actual.Exception);

        usersRepositoryMock.Verify(repo => repo.Create(It.IsAny<Account>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenUserDoesNotExistYet_ShouldSucceedAndPersistUser()
    {
        var userName = "test";
        var email = "mail";
        var password = "password";
        var passwordHash = "passwordhash";

        Account? capturedUser = null;

        usersRepositoryMock.Setup(repo => repo.FindByEmail(email)).ReturnsAsync(null as Account);

        usersRepositoryMock
            .Setup(repo => repo.Create(It.IsAny<Account>()))
            .Callback<Account>(user => capturedUser = user);

        passwordHasherMock.Setup(hasher => hasher.HashPassword(password)).Returns(passwordHash);

        var actual = await accountInteractor.Create(userName, email, password);

        var expected = new Account
        {
            UserName = userName,
            Email = email,
            Password = passwordHash,
        };

        Assert.True(actual.IsSuccess);
        Assert.Equivalent(expected, capturedUser);

        usersRepositoryMock.Verify(repo => repo.Flush(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Get_WhenUserWithGivenIdDoesNotExist_ShouldFail()
    {
        var id = Guid.NewGuid();

        usersRepositoryMock.Setup(repo => repo.FindById(id)).ReturnsAsync(null as Account);

        var actual = await accountInteractor.Get(id);

        Assert.True(actual.IsFailure);
        Assert.IsType<NoSuch<Account>>(actual.Exception);
    }

    [Fact]
    public async Task Get_WhenUserWithGivenIdExists_ShouldSucceedAndReturnFoundUser()
    {
        var id = Guid.NewGuid();
        var user = new Account
        {
            Id = id,
            UserName = "userName",
            Email = "email",
            Password = "password",
        };
        usersRepositoryMock.Setup(repo => repo.FindById(id)).ReturnsAsync(user);

        var actual = await accountInteractor.Get(id);

        Assert.True(actual.IsSuccess);
        Assert.Equivalent(user, actual.Value);
    }
}

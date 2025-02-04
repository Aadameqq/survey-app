using Core.Domain;
using Core.Exceptions;
using Core.Interactors;
using Core.Ports;
using Moq;

namespace Core.Tests.Interactors;

public class UserInteractorTests
{
    private readonly Mock<ActivationCodeRepository> codeRepositoryMock = new();
    private readonly Mock<ActivationEmailBodyGenerator> emailBodyGeneratorMock = new();
    private readonly Mock<EmailSender> emailSenderMock = new();
    private readonly Mock<PasswordHasher> passwordHasherMock = new();
    private readonly UserInteractor userInteractor;
    private readonly Mock<UsersRepository> usersRepositoryMock = new();

    public UserInteractorTests()
    {
        userInteractor = new UserInteractor(
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
                new User
                {
                    Email = testEmail,
                    UserName = "userName",
                    Password = "password",
                }
            );

        var actual = await userInteractor.Create("userName", testEmail, "password");

        Assert.True(actual.IsFailure);
        Assert.IsType<AlreadyExists<User>>(actual.Exception);

        usersRepositoryMock.Verify(repo => repo.Create(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenUserDoesNotExistYet_ShouldSucceedAndPersistUser()
    {
        var userName = "test";
        var email = "mail";
        var password = "password";
        var passwordHash = "passwordhash";

        User? capturedUser = null;

        usersRepositoryMock.Setup(repo => repo.FindByEmail(email)).ReturnsAsync(null as User);

        usersRepositoryMock
            .Setup(repo => repo.Create(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);

        passwordHasherMock.Setup(hasher => hasher.HashPassword(password)).Returns(passwordHash);

        var actual = await userInteractor.Create(userName, email, password);

        var expected = new User
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

        usersRepositoryMock.Setup(repo => repo.FindById(id)).ReturnsAsync(null as User);

        var actual = await userInteractor.Get(id);

        Assert.True(actual.IsFailure);
        Assert.IsType<NoSuch<User>>(actual.Exception);
    }

    [Fact]
    public async Task Get_WhenUserWithGivenIdExists_ShouldSucceedAndReturnFoundUser()
    {
        var id = Guid.NewGuid();
        var user = new User
        {
            Id = id,
            UserName = "userName",
            Email = "email",
            Password = "password",
        };
        usersRepositoryMock.Setup(repo => repo.FindById(id)).ReturnsAsync(user);

        var actual = await userInteractor.Get(id);

        Assert.True(actual.IsSuccess);
        Assert.Equivalent(user, actual.Value);
    }
}

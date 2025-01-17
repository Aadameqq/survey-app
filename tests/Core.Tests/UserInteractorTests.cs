using Core.Domain;
using Core.Exceptions;
using Core.Interactors;
using Core.Ports;
using Moq;

namespace Core.Tests;

public class UserInteractorTests
{
    private readonly Mock<UsersRepository> _usersRepositoryMock;
    private readonly Mock<PasswordHasher> _passwordHasherMock;
    private readonly UserInteractor _userInteractor;

    public UserInteractorTests()
    {
        _usersRepositoryMock = new Mock<UsersRepository>();
        _passwordHasherMock = new Mock<PasswordHasher>();

        _userInteractor = new UserInteractor(_usersRepositoryMock.Object, _passwordHasherMock.Object);
    }

    [Fact]
    public async Task Create_WhenUserWithGivenEmailAlreadyExists_ShouldFailAndNotPersist()
    {
        var testEmail = "mail";
        _usersRepositoryMock.Setup(repo => repo.FindByEmail(testEmail))
            .ReturnsAsync(new User
            {
                Email = testEmail,
                UserName = "",
                Password = ""
            });

        var actual = await _userInteractor.Create("", testEmail, "");

        Assert.True(actual.IsFailure);
        Assert.IsType<AlreadyExists<User>>(actual.Exception);

        _usersRepositoryMock.Verify(repo => repo.Create(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Create_WhenUserDoesNotExistYet_ShouldSucceedAndPersistUser()
    {
        var userName = "test";
        var email = "mail";
        var password = "password";
        var passwordHash = "passwordhash";

        User capturedUser = null;

        _usersRepositoryMock.Setup(repo => repo.FindByEmail(email))
            .ReturnsAsync(null as User);

        _usersRepositoryMock
            .Setup(repo => repo.Create(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);

        _passwordHasherMock.Setup(hasher => hasher.HashPassword(password)).Returns(passwordHash);

        var actual = await _userInteractor.Create(userName, email, password);

        var expected = new User
        {
            UserName = userName,
            Email = email,
            Password = passwordHash
        };

        Assert.True(actual.IsSuccess);
        Assert.Equivalent(expected, capturedUser);

        _usersRepositoryMock.Verify(repo => repo.Flush(), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Get_WhenUserWithGivenIdDoesNotExist_ShouldFail()
    {
        var id = Guid.NewGuid();

        _usersRepositoryMock.Setup(repo => repo.FindById(id)).ReturnsAsync(null as User);

        var actual = await _userInteractor.Get(id);

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
            Password = "password"
        };
        _usersRepositoryMock.Setup(repo => repo.FindById(id)).ReturnsAsync(user);

        var actual = await _userInteractor.Get(id);

        Assert.True(actual.IsSuccess);
        Assert.Equivalent(user, actual.Value);
    }
}

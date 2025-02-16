using Core.Domain;
using Core.Exceptions;

namespace Core.Tests.Domain;

public class AccountTests
{
    private readonly Account testAccount = new("username", "email", "password");
    private TimeSpan sessionLifeSpan = TimeSpan.FromMinutes(30);

    [Fact]
    public void HasBeenActivated_ShouldReturnTrue_WhenAccountIsNew()
    {
        Assert.False(testAccount.HasBeenActivated());
    }

    [Fact]
    public void HasBeenActivated_ShouldReturnTrue_WhenAccountIsActivated()
    {
        testAccount.Activate();

        Assert.True(testAccount.HasBeenActivated());
    }

    [Fact]
    public void ChangePassword_ShouldUpdatePassword()
    {
        var newPassword = "new-password";

        testAccount.ChangePassword(newPassword);

        Assert.Equal(newPassword, testAccount.Password);
    }

    [Fact]
    public void AssignRole_ShouldFail_WhenAccountAlreadyHasRole()
    {
        testAccount.AssignRole(Role.Admin, Guid.Empty);

        var result = testAccount.AssignRole(Role.ProblemsCreator, Guid.Empty);

        Assert.True(result.IsFailure);
        Assert.IsType<RoleAlreadyAssigned>(result.Exception);
    }

    [Fact]
    public void AssignRole_ShouldAssignRole_WhenAccountHasNoRoleAssignedAndIsNotIssuer()
    {
        var result = testAccount.AssignRole(Role.Admin, Guid.Empty);

        Assert.True(result.IsSuccess);
        Assert.Equal(Role.Admin, testAccount.Role);
    }

    [Fact]
    public void AssignRole_ShouldRemoveAllSessions_WhenAccountHasNoRoleAssignedAndIsNotIssuer()
    {
        var firstToken = "first-token";
        CreateSessionAndGetId(firstToken);
        var secondToken = "second-token";
        CreateSessionAndGetId(secondToken);

        testAccount.AssignRole(Role.Admin, Guid.Empty);

        Assert.Null(testAccount.GetSessionId(firstToken));
        Assert.Null(testAccount.GetSessionId(secondToken));
    }

    [Fact]
    public void AssignRole_Fail_WhenAccountIsIssuer()
    {
        var result = testAccount.AssignRole(Role.Admin, testAccount.Id);

        Assert.True(result.IsFailure);
        Assert.IsType<CannotManageOwn<Role>>(result.Exception);
    }

    [Fact]
    public void RemoveRole_ShouldSucceedAndRemoveRole_WhenAccountIsNotIssuer()
    {
        testAccount.AssignRole(Role.Admin, Guid.Empty);

        var result = testAccount.RemoveRole(Guid.Empty);

        Assert.True(result.IsSuccess);
        Assert.Equal(Role.None, testAccount.Role);
    }

    [Fact]
    public void RemoveRole_ShouldFail_WhenAccountIsIssuer()
    {
        var result = testAccount.RemoveRole(testAccount.Id);

        Assert.True(result.IsFailure);
        Assert.IsType<CannotManageOwn<Role>>(result.Exception);
    }

    [Fact]
    public void CreateSession_ShouldFail_WhenAccountHasNotBeenActivatedYet()
    {
        var result = testAccount.CreateSession(DateTime.MinValue, "token");

        Assert.True(result.IsFailure);
        Assert.IsType<AccountNotActivated>(result.Exception);
    }

    [Fact]
    public void CreateSession_ShouldSucceed_WhenAccountHasBeenActivated()
    {
        testAccount.Activate();

        var result = testAccount.CreateSession(DateTime.MinValue, "token");

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void GetSessionId_ShouldReturnCorrectId_WhenSessionHasBeenCreated()
    {
        var testToken = "token";

        testAccount.Activate();

        var result = testAccount.CreateSession(DateTime.MinValue, testToken);
        var expectedId = result.Value;

        var actualId = testAccount.GetSessionId(testToken);

        Assert.Equal(expectedId, actualId);
    }

    [Fact]
    public void DestroySession_ShouldRemoveCorrect()
    {
        testAccount.Activate();

        var firstToken = "token1";
        var firstId = CreateSessionAndGetId(firstToken);

        var secondToken = "token2";
        var secondId = CreateSessionAndGetId(secondToken);

        testAccount.DestroySession(firstId);

        Assert.Null(testAccount.GetSessionId(firstToken));
        Assert.NotNull(testAccount.GetSessionId(secondToken));
    }

    [Fact]
    public void DestroyAllSessions_ShouldRemoveAllSessions()
    {
        testAccount.Activate();

        var firstToken = "token1";
        CreateSessionAndGetId(firstToken);

        var secondToken = "token2";
        CreateSessionAndGetId(secondToken);

        testAccount.DestroyAllSessions();

        Assert.Null(testAccount.GetSessionId(firstToken));
        Assert.Null(testAccount.GetSessionId(secondToken));
    }

    [Fact]
    public void RefreshToken_ShouldFail_WhenSessionWithGivenTokenDoesNotExist()
    {
        var result = testAccount.RefreshSession("invalid-token", DateTime.MinValue, "token");

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
    }

    [Fact]
    public void RefreshToken_ShouldFailAndRemoveSession_WhenSessionExistsButIsNotActive()
    {
        testAccount.Activate();

        var now = DateTime.MinValue;
        var token = "token";
        testAccount.CreateSession(now, token);

        var newToken = "new-token";
        var result = testAccount.RefreshSession(token, now + sessionLifeSpan, newToken);

        Assert.True(result.IsFailure);
        Assert.IsType<NoSuch<AuthSession>>(result.Exception);
        Assert.Null(testAccount.GetSessionId(token));
        Assert.Null(testAccount.GetSessionId(newToken));
    }

    [Fact]
    public void RefreshToken_ShouldSucceedAndReturnCorrectArchivedToken_WhenSessionExistsAndIsActive()
    {
        testAccount.Activate();

        var now = DateTime.MinValue;
        var token = "token";
        var sessionId = testAccount.CreateSession(now, token).Value;

        var result = testAccount.RefreshSession(token, now, "new-token");

        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.Token, token);
        Assert.Equal(result.Value.SessionId, sessionId);
    }

    [Fact]
    public void RefreshToken_ShouldChangeSessionToken_WhenSessionExistsAndIsActive()
    {
        testAccount.Activate();

        var now = DateTime.MinValue;
        var token = "token";
        var newToken = "new-token";
        var sessionId = testAccount.CreateSession(now, token).Value;
        testAccount.RefreshSession(token, now, newToken);

        Assert.Null(testAccount.GetSessionId(token));
        Assert.Equal(sessionId, testAccount.GetSessionId(newToken));
    }

    [Fact]
    public void ResetPassword_ShouldChangePassword()
    {
        var newPasswordHash = "new-password-hash";
        testAccount.ResetPassword(newPasswordHash);

        Assert.Equal(newPasswordHash, testAccount.Password);
    }

    [Fact]
    public void ResetPassword_ShouldRemoveAllSessions()
    {
        var firstToken = "token1";
        CreateSessionAndGetId(firstToken);

        var secondToken = "token2";
        CreateSessionAndGetId(secondToken);

        testAccount.ResetPassword("new-password-hash");

        Assert.Null(testAccount.GetSessionId(firstToken));
        Assert.Null(testAccount.GetSessionId(secondToken));
    }

    private Guid CreateSessionAndGetId(string token)
    {
        testAccount.Activate();
        return testAccount.CreateSession(DateTime.MinValue, token).Value;
    }
}

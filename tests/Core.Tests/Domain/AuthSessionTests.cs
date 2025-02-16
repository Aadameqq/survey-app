using Core.Domain;
using Core.Exceptions;

namespace Core.Tests.Domain;

public class AuthSessionTests
{
    private readonly DateTime testNow = DateTime.MinValue;
    private readonly AuthSession testSession;
    private readonly string testToken = "token";
    private TimeSpan sessionLifeSpan = TimeSpan.FromMinutes(30);

    public AuthSessionTests()
    {
        testSession = new AuthSession(Guid.Empty, testNow, testToken);
    }

    [Fact]
    public void IsActive_ShouldBeTrue_WhenCurrentDateIsLowerThanExpirationDate()
    {
        Assert.True(testSession.IsActive(testNow));
        Assert.True(
            testSession.IsActive(testNow + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1)))
        );
    }

    [Fact]
    public void IsActive_ShouldBeFalse_WhenCurrentDateIsGreaterThanOrEqualExpirationDate()
    {
        Assert.False(testSession.IsActive(testNow + sessionLifeSpan));
        Assert.False(testSession.IsActive(testNow + sessionLifeSpan.Add(TimeSpan.FromMinutes(70))));
    }

    [Fact]
    public void Refresh_ShouldFail_WhenSessionIsNotActive()
    {
        var result = testSession.Refresh("new-token", testNow + sessionLifeSpan);

        Assert.True(result.IsFailure);
        Assert.IsType<SessionInactive>(result.Exception);
    }

    [Fact]
    public void Refresh_ShouldSucceedAndReturnCorrectArchivedToken_WhenSessionIsActive()
    {
        var result = testSession.Refresh(
            "new-token",
            testNow + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1))
        );

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(result.Value.Token, testToken);
        Assert.Equal(result.Value.SessionId, testSession.Id);
    }

    [Fact]
    public void Refresh_ShouldChangeCurrentTokenAndExpirationDate_WhenSessionIsActive()
    {
        var newToken = "new-token";

        var refreshTime = testNow + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1));

        testSession.Refresh(newToken, refreshTime);

        Assert.Equal(newToken, testSession.CurrentToken);
        Assert.True(
            testSession.IsActive(refreshTime + sessionLifeSpan.Subtract(TimeSpan.FromMinutes(1)))
        );
        Assert.False(testSession.IsActive(refreshTime + sessionLifeSpan));
    }
}

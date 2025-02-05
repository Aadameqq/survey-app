namespace Core.Domain;

public class AuthSession
{
    private DateTime expiresAt;
    private TimeSpan lifeSpan = TimeSpan.FromMinutes(30);

    public AuthSession(Guid userId, DateTime now, string refreshToken)
    {
        UserId = userId;
        expiresAt = now + lifeSpan;
        CurrentToken = refreshToken;
    }

    private AuthSession() { }

    public Guid Id { get; init; } = Guid.NewGuid();
    public string CurrentToken { get; private set; }
    public Guid UserId { get; }

    public ArchivedToken GetTokenForArchiving()
    {
        return new ArchivedToken(CurrentToken, Id);
    }

    public void Refresh(string newToken, DateTime now)
    {
        CurrentToken = newToken;
        expiresAt = now + lifeSpan;
    }

    public bool IsActive(DateTime now)
    {
        return now < expiresAt;
    }
}

using Core.Exceptions;

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

    public Result<ArchivedToken> Refresh(string newToken, DateTime now)
    {
        if (!IsActive(now))
        {
            return new SessionInactive();
        }

        var archived = new ArchivedToken(CurrentToken, Id);

        CurrentToken = newToken;
        expiresAt = now + lifeSpan;

        return archived;
    }

    public bool IsActive(DateTime now)
    {
        return now < expiresAt;
    }
}

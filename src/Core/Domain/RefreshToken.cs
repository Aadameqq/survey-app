namespace Core.Domain;

public class RefreshToken
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Token { get; private init; }
    public AuthSession Session { get; private init; }
    private bool _revoked;
    private DateTime _expiresAt;

    private RefreshToken()
    {
    }

    public RefreshToken(AuthSession authSession, DateTime expiresAt, string token)
    {
        Session = authSession;
        _expiresAt = expiresAt;
        Token = token;
    }

    public void Revoke()
    {
        _revoked = true;
    }

    public bool IsRevoked()
    {
        return _revoked;
    }

    public bool HasExpired()
    {
        return DateTime.UtcNow > _expiresAt;
    }

    public bool IsInvalid()
    {
        return IsRevoked() || HasExpired();
    }
}

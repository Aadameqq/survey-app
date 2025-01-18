namespace Core.Domain;

public class RefreshToken
{
    private DateTime expiresAt;
    private bool revoked;

    public RefreshToken(AuthSession authSession, DateTime expiresAt, string token)
    {
        Session = authSession;
        this.expiresAt = expiresAt;
        Token = token;
    }

    private RefreshToken() { }

    public Guid Id { get; } = Guid.NewGuid();

    public string Token { get; private init; }

    public AuthSession Session { get; private init; }

    public void Revoke()
    {
        revoked = true;
    }

    public bool IsRevoked()
    {
        return revoked;
    }

    public bool HasExpired()
    {
        return DateTime.UtcNow > expiresAt;
    }

    public bool IsInvalid()
    {
        return IsRevoked() || HasExpired();
    }
}

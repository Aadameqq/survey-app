namespace Core.Domain;

public class RefreshToken
{
    public Guid SessionId { get; private set; }
    public required string Token { get; init; }
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; private init; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; private init; }
    public bool Revoked { get; private set; } = false;

    public required TimeSpan LifeSpan
    {
        init => ExpiresAt = CreatedAt + TimeSpan.FromMinutes(value.Minutes);
    }

    public required AuthSession Session
    {
        init => SessionId = value.Id;
    }

    public void Revoke()
    {
        Revoked = true;
    }

    public bool IsRevoked()
    {
        return Revoked;
    }

    public bool HasExpired()
    {
        return DateTime.UtcNow > ExpiresAt;
    }
}

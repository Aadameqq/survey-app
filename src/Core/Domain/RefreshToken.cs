namespace Core.Domain;

public record RefreshToken
{
    public readonly Guid Id = Guid.NewGuid();

    public RefreshToken(Guid sessionId)
    {
        SessionId = sessionId;
    }

    public RefreshToken(Guid id, Guid sessionId)
    {
        Id = id;
        SessionId = sessionId;
    }

    public Guid SessionId { get; }
}

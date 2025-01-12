namespace Core.Domain;

public class AuthSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; }

    public AuthSession(Guid userId)
    {
        UserId = userId;
    }
}

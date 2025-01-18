namespace Core.Domain;

public class AuthSession
{
    public AuthSession(Guid userId)
    {
        UserId = userId;
    }

    public Guid Id { get; init; } = Guid.NewGuid();

    public Guid UserId { get; }
}

namespace Core.Domain;

public class AuthSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid UserId { get; init; }
}

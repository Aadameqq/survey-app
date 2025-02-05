namespace Core.Domain;

public class Account
{
    private bool activated;
    public Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }

    public bool HasBeenActivated()
    {
        return activated;
    }

    public void Activate()
    {
        activated = true;
    }
}

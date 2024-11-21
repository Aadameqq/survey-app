namespace Api.Models;

public class User
{
    public Guid Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}

using Core.Exceptions;

namespace Core.Domain;

public class Account
{
    private bool activated;

    public Account(string userName, string email, string password)
    {
        UserName = userName;
        Email = email;
        Password = password;
    }

    private Account() { }

    public Role Role { get; private set; } = Role.None;
    public Guid Id { get; } = Guid.NewGuid();
    public string UserName { get; private set; }
    public string Email { get; private set; }
    public string Password { get; private set; }

    public bool HasBeenActivated()
    {
        return activated;
    }

    public void Activate()
    {
        activated = true;
    }

    public void ChangePassword(string newPasswordHash)
    {
        Password = newPasswordHash;
    }

    public Result AssignRole(Role role)
    {
        if (Role != Role.None)
        {
            return new RoleHasBeenAlreadyAssigned();
        }

        Role = role;
        return Result.Success();
    }

    public void RemoveRole()
    {
        Role = Role.None;
    }
}

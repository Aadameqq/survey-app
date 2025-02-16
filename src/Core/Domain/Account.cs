using Core.Exceptions;

namespace Core.Domain;

public class Account
{
    private bool activated;
    private List<AuthSession> sessions = [];

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

    public Result AssignRole(Role role, Guid issuerId)
    {
        if (issuerId == Id)
        {
            return new CannotManageOwn<Role>();
        }

        if (Role != Role.None)
        {
            return new RoleAlreadyAssigned();
        }

        Role = role;
        DestroyAllSessions();
        return Result.Success();
    }

    public Result RemoveRole(Guid issuerId)
    {
        if (issuerId == Id)
        {
            return new CannotManageOwn<Role>();
        }

        Role = Role.None;

        return Result.Success();
    }

    public Result<Guid> CreateSession(DateTime now, string refreshToken)
    {
        if (!HasBeenActivated())
        {
            return new AccountNotActivated();
        }

        var created = new AuthSession(Id, now, refreshToken);

        sessions.Add(created);

        return created.Id;
    }

    public void DestroySession(Guid sessionId)
    {
        sessions.RemoveAll(session => session.Id == sessionId);
    }

    public void DestroyAllSessions()
    {
        sessions = [];
    }

    public Result<ArchivedToken> RefreshSession(string oldToken, DateTime now, string newToken)
    {
        var session = sessions.Find(session => session.CurrentToken == oldToken);

        if (session is null)
        {
            return new NoSuch<AuthSession>();
        }

        var result = session.Refresh(newToken, now);

        if (result is { IsFailure: true, Exception: SessionInactive })
        {
            DestroySession(session.Id);
            return new NoSuch<AuthSession>();
        }

        return result.Value;
    }

    public void ResetPassword(string newPasswordHash)
    {
        ChangePassword(newPasswordHash);
        DestroyAllSessions();
    }

    public Guid? GetSessionId(string token)
    {
        return sessions.Find(s => s.CurrentToken == token)?.Id;
    }
}

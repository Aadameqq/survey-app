using Core.Exceptions;

namespace Core.Domain;

public record Role
{
    public static readonly Role Admin;
    public static readonly Role ProblemsCreator;
    public static readonly Role None;

    static Role()
    {
        Admin = Register(nameof(Admin));
        ProblemsCreator = Register(nameof(ProblemsCreator));
        None = Register(nameof(None));
    }

    private Role(string name)
    {
        Name = name;
    }

    public static List<Role> Roles { get; } = [];

    public string Name { get; }

    public static Result<Role> FromName(string name)
    {
        if (!Roles.Exists(r => r.Name == name))
        {
            return new NoSuch<Role>();
        }

        return new Role(name);
    }

    private static Role Register(string name)
    {
        var role = new Role(name);
        Roles.Add(role);
        return role;
    }
}

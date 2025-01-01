using Api.Models;
using Api.Models.Interfaces;

namespace Api.Infrastructure;

public class EfUsersRepository(DatabaseContext ctx) : UsersRepository
{
    public User? FindByEmail(string email)
    {
        return ctx.Users.FirstOrDefault(u => u.Email == email);
    }

    public User? FindById(Guid id)
    {
        return ctx.Users.FirstOrDefault(u => u.Id == id);
    }


    public void Create(User user)
    {
        ctx.Users.Add(user);
        ctx.SaveChanges();
    }
}

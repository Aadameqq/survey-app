using Core.Domain;
using Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class EfUsersRepository(DatabaseContext ctx) : UsersRepository
{
    public Task<User?> FindByEmail(string email)
    {
        return ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> FindById(Guid id)
    {
        return ctx.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task Create(User user)
    {
        await ctx.Users.AddAsync(user);
    }

    public Task Update(User user)
    {
        ctx.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task Flush()
    {
        await ctx.SaveChangesAsync();
    }
}

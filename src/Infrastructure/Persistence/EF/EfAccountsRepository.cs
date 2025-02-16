using Core.Domain;
using Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.EF;

public class EfAccountsRepository(DatabaseContext ctx) : AccountsRepository
{
    public Task<Account?> FindByEmail(string email)
    {
        return ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<Account?> FindById(Guid id)
    {
        return ctx.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task Create(Account account)
    {
        await ctx.Users.AddAsync(account);
    }

    public Task Update(Account account)
    {
        ctx.Users.Update(account);
        return Task.CompletedTask;
    }

    public async Task UpdateAndFlush(Account account)
    {
        await Update(account);
        await Flush();
    }

    public async Task Flush()
    {
        await ctx.SaveChangesAsync();
    }
}

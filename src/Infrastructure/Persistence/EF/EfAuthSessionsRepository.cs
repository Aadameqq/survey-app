using Core.Domain;
using Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.EF;

public class EfAuthSessionsRepository(DatabaseContext ctx) : AuthSessionsRepository
{
    public Task Remove(AuthSession session)
    {
        ctx.AuthSessions.Remove(session);
        return Task.CompletedTask;
    }

    public async Task Create(AuthSession session)
    {
        await ctx.AuthSessions.AddAsync(session);
    }

    public Task Update(AuthSession session)
    {
        ctx.AuthSessions.Update(session);
        return Task.CompletedTask;
    }

    public Task<AuthSession?> FindById(Guid sessionId)
    {
        return ctx.AuthSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task RemoveAllByAccountAndFlush(Account account)
    {
        await ctx.AuthSessions.Where(session => session.UserId == account.Id).ExecuteDeleteAsync();
    }

    public Task<AuthSession?> FindByToken(string token)
    {
        return ctx.AuthSessions.FirstOrDefaultAsync(s => s.CurrentToken == token);
    }

    public async Task<AuthSession?> FindByArchivedToken(string token)
    {
        return await ctx
            .ArchivedTokens.Where(t => t.Token == token)
            .Select(t => t.Session)
            .FirstOrDefaultAsync();
    }

    public async Task ArchiveToken(ArchivedToken token)
    {
        await ctx.ArchivedTokens.AddAsync(token);
    }

    public async Task Flush()
    {
        await ctx.SaveChangesAsync();
    }
}

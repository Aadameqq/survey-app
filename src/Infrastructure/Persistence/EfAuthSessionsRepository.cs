using Core.Domain;
using Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class EfAuthSessionsRepository(DatabaseContext ctx) : AuthSessionsRepository
{
    public Task Remove(AuthSession session)
    {
        ctx.AuthSessions.Remove(session);
        return Task.CompletedTask;
    }

    public Task<AuthSession?> FindById(Guid sessionId)
    {
        return ctx.AuthSessions.FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task Flush()
    {
        await ctx.SaveChangesAsync();
    }
}

using Core.Domain;
using Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class EfRefreshTokensRepository(DatabaseContext ctx)
    : RefreshTokensRepository
{
    public async Task Persist(RefreshToken token)
    {
        await ctx.RefreshTokens.AddAsync(token);
    }

    public async Task RemoveAllInSession(AuthSession session)
    {
        await ctx.RefreshTokens.Where(t => t.SessionId == session.Id).ExecuteDeleteAsync();
    }

    public Task<RefreshToken?> FindByToken(string token)
    {
        return ctx.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task Update(RefreshToken token)
    {
        ctx.RefreshTokens.Update(token);
    }

    public async Task Flush()
    {
        await ctx.SaveChangesAsync();
    }
}

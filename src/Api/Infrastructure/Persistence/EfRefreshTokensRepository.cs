using Api.Models;
using Api.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure;

public class EfRefreshTokensRepository(DatabaseContext ctx)
    : RefreshTokensRepository
{
    public void Persist(RefreshToken token)
    {
        ctx.RefreshTokens.Add(token);
    }

    public void RemoveAllInSession(AuthSession session)
    {
        ctx.RefreshTokens.Where(t => t.SessionId == session.Id).ExecuteDelete();
    }

    public RefreshToken? FindByToken(string token)
    {
        return ctx.RefreshTokens.FirstOrDefault(t => t.Token == token);
    }

    public void Update(RefreshToken token)
    {
        ctx.RefreshTokens.Update(token);
    }

    public void Flush()
    {
        ctx.SaveChanges();
    }
}

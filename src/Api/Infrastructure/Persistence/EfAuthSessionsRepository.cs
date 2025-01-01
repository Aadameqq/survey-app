using Api.Models;
using Api.Models.Interfaces;

namespace Api.Infrastructure;

public class EfAuthSessionsRepository(DatabaseContext ctx) : AuthSessionsRepository
{
    public void Persist(AuthSession session)
    {
        ctx.AuthSessions.Add(session);
    }

    public void Remove(AuthSession session)
    {
        ctx.AuthSessions.Remove(session);
    }

    public AuthSession? FindById(Guid sessionId)
    {
        return ctx.AuthSessions.FirstOrDefault(s => s.Id == sessionId);
    }

    public AuthSession? FindByRefreshToken(RefreshToken refreshToken)
    {
        return ctx.AuthSessions.FirstOrDefault(s => s.Id == refreshToken.SessionId);
    }

    public void Flush()
    {
        ctx.SaveChanges();
    }
}

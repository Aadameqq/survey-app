using Api.Models;
using Api.Models.Interfaces;

namespace Api.Infrastructure;

public class EfTokensRepository(DatabaseContext ctx) : TokensRepository
{
    public void Create(RefreshTokenPayload tokenPayload)
    {
        ctx.RefreshTokens.Add(tokenPayload);
        ctx.SaveChanges();
    }

    public RefreshTokenPayload? FindByUser(Guid userId)
    {
        return ctx.RefreshTokens.FirstOrDefault(t => t.UserId == userId);
    }

    public RefreshTokenPayload? FindById(Guid id)
    {
        return ctx.RefreshTokens.FirstOrDefault(t => t.Id == id);
    }
}

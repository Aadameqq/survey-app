namespace Api.Models.Interfaces;

public interface RefreshTokensRepository
{
    void Persist(RefreshToken token);
    void RemoveAllInSession(AuthSession session);
    RefreshToken? FindByToken(string token);
    void Update(RefreshToken token);
    void Flush();
}

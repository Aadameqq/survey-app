namespace Api.Models.Interfaces;

public interface AuthSessionsRepository
{
    void Persist(AuthSession session);
    void Remove(AuthSession session);
    AuthSession? FindById(Guid sessionId);
    AuthSession? FindByRefreshToken(RefreshToken refreshToken);
    void Flush();
}

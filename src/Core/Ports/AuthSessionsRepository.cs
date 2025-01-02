using Core.Domain;

namespace Core.Ports;

public interface AuthSessionsRepository
{
    Task Persist(AuthSession session);
    Task Remove(AuthSession session);
    Task<AuthSession?> FindById(Guid sessionId);
    Task<AuthSession?> FindByRefreshToken(RefreshToken refreshToken);
    Task Flush();
}

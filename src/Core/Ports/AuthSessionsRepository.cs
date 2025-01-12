using Core.Domain;

namespace Core.Ports;

public interface AuthSessionsRepository
{
    Task Remove(AuthSession session);
    Task<AuthSession?> FindById(Guid sessionId);
    Task Flush();
}

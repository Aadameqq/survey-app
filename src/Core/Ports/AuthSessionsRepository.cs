using Core.Domain;

namespace Core.Ports;

public interface AuthSessionsRepository
{
    Task Remove(AuthSession session);
    Task Create(AuthSession session);
    Task Update(AuthSession session);

    Task<AuthSession?> FindById(Guid sessionId);
    Task<AuthSession?> FindByToken(string token);
    Task<AuthSession?> FindByArchivedToken(string token);
    Task ArchiveToken(ArchivedToken token);

    Task Flush();
}

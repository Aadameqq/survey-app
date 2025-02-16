using Core.Domain;

namespace Core.Ports;

public interface ArchivedTokensRepository
{
    Task<ArchivedToken?> FindByToken(string token);

    Task CreateAndFlush(ArchivedToken archivedToken);
}

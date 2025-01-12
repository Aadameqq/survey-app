using Core.Domain;

namespace Core.Ports;

public interface RefreshTokensRepository
{
    Task Persist(RefreshToken token);
    Task<RefreshToken?> FindByToken(string token);
    Task Update(RefreshToken token);
    Task Flush();
}

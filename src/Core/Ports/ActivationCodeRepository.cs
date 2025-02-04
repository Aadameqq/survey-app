using Core.Domain;

namespace Core.Ports;

public interface ActivationCodeRepository
{
    public Task<string> Create(User user);
    public Task<Guid?> GetUserIdAndRevokeCode(string code);
}

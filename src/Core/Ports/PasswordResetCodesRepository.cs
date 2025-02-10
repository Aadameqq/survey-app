using Core.Domain;

namespace Core.Ports;

public interface PasswordResetCodesRepository
{
    public Task<string> Create(Account account);
    public Task<Guid?> GetAccountIdAndRevokeCode(string code);
}

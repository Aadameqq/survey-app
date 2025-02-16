using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class LogOutUseCase(AccountsRepository accountsRepository)
{
    public async Task<Result> Execute(Guid accountId, Guid sessionId)
    {
        var account = await accountsRepository.FindById(accountId);

        if (account is null)
        {
            return new NoSuch<Account>();
        }

        account.DestroySession(sessionId);

        await accountsRepository.UpdateAndFlush(account);

        return Result.Success();
    }
}

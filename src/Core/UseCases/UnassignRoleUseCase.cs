using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class UnassignRoleUseCase(AccountsRepository accountsRepository)
{
    public async Task<Result> Execute(Guid issuerId, Guid accountId)
    {
        if (issuerId == accountId)
        {
            return new CannotManageOwn<Role>();
        }

        var account = await accountsRepository.FindById(accountId);

        if (account is null)
        {
            return new NoSuch<Account>();
        }

        account.RemoveRole();

        await accountsRepository.Update(account);
        await accountsRepository.Flush();

        return Result.Success();
    }
}

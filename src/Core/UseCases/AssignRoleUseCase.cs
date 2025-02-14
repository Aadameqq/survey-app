using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class AssignRoleUseCase(AccountsRepository accountsRepository)
{
    public async Task<Result> Execute(Guid issuerId, Guid accountId, Role role)
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

        var assignResult = account.AssignRole(role);

        if (assignResult.IsFailure)
        {
            return assignResult.Exception;
        }

        await accountsRepository.Update(account);
        await accountsRepository.Flush();

        return Result.Success();
    }
}

using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class AssignRoleUseCase(AccountsRepository accountsRepository)
{
    public async Task<Result> Execute(Guid issuerId, Guid accountId, string roleName)
    {
        if (issuerId == accountId)
        {
            return new CannotManageOwn<Role>();
        }

        var roleResult = Role.FromName(roleName);

        if (roleResult.IsFailure)
        {
            return roleResult.Exception;
        }

        var account = await accountsRepository.FindById(accountId);

        if (account is null)
        {
            return new NoSuch<Account>();
        }

        var assignResult = account.AssignRole(roleResult.Value);

        if (assignResult.IsFailure)
        {
            return assignResult.Exception;
        }

        await accountsRepository.Update(account);
        await accountsRepository.Flush();

        return Result.Success();
    }
}

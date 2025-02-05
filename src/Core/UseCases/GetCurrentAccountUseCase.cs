using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class GetCurrentAccountUseCase(AccountsRepository accountsRepository)
{
    public async Task<Result<Account>> Execute(Guid id)
    {
        var found = await accountsRepository.FindById(id);
        if (found is null)
        {
            return new NoSuch<Account>();
        }

        return found;
    }
}

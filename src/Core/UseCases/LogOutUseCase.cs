using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class LogOutUseCase(AuthSessionsRepository authSessionsRepository)
{
    public async Task<Result> Execute(Guid sessionId)
    {
        var session = await authSessionsRepository.FindById(sessionId);

        if (session is null)
        {
            return new NoSuch<AuthSession>();
        }

        await authSessionsRepository.Remove(session);
        await authSessionsRepository.Flush();

        return Result.Success();
    }
}

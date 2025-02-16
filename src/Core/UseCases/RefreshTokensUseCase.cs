using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class RefreshTokensUseCase(
    AccountsRepository accountsRepository,
    DateTimeProvider dateTimeProvider,
    TokenService tokenService,
    ArchivedTokensRepository archivedTokensRepository
)
{
    public async Task<Result<TokenPairOutput>> Execute(Guid accountId, string token)
    {
        var account = await accountsRepository.FindById(accountId);

        if (account is null)
        {
            return new NoSuch<Account>();
        }

        var archived = await archivedTokensRepository.FindByToken(token);

        if (archived is not null)
        {
            account.DestroyAllSessions();
            await accountsRepository.UpdateAndFlush(account);

            return new InvalidToken();
        }

        var refreshToken = tokenService.CreateRefreshToken(account);

        var result = account.RefreshSession(token, dateTimeProvider.Now(), refreshToken);

        if (result is { IsFailure: true, Exception: NoSuch<AuthSession> })
        {
            return result.Exception;
        }

        var accessToken = tokenService.CreateAccessToken(
            account,
            account.GetSessionId(refreshToken)!.Value
        );

        await archivedTokensRepository.CreateAndFlush(result.Value);
        await accountsRepository.UpdateAndFlush(account);

        return new TokenPairOutput(accessToken, refreshToken);
    }
}

using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class RefreshTokensUseCase(
    AuthSessionsRepository authSessionsRepository,
    DateTimeProvider dateTimeProvider,
    RefreshTokensFactory refreshTokensFactory,
    AccessTokenService accessTokenService
)
{
    public async Task<Result<TokenPairOutput>> Execute(string token)
    {
        var session = await authSessionsRepository.FindByToken(token);

        if (session is null)
        {
            var authSession = await authSessionsRepository.FindByArchivedToken(token);

            if (authSession is null)
            {
                return new NoSuch<AuthSession>();
            }

            await authSessionsRepository.Remove(authSession);
            await authSessionsRepository.Flush();
            return new InvalidToken();
        }

        if (!session.IsActive(dateTimeProvider.Now()))
        {
            await authSessionsRepository.Remove(session);
            await authSessionsRepository.Flush();
            return new NoSuch<AuthSession>();
        }

        var refreshToken = refreshTokensFactory.Generate();
        var accessToken = accessTokenService.Create(session);

        await authSessionsRepository.ArchiveToken(session.GetTokenForArchiving());

        session.Refresh(refreshToken, dateTimeProvider.Now());

        await authSessionsRepository.Update(session);
        await authSessionsRepository.Flush();

        return new TokenPairOutput(accessToken, refreshToken);
    }
}

using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.Interactors;

public class AuthInteractor(
    AccountsRepository accountsRepository,
    PasswordVerifier passwordVerifier,
    RefreshTokensFactory refreshTokensFactory,
    AuthSessionsRepository authSessionsRepository,
    AccessTokenService accessTokenService,
    DateTimeProvider dateTimeProvider
)
{
    public async Task<Result<TokenPairOutput>> LogIn(string email, string password)
    {
        var user = await accountsRepository.FindByEmail(email);
        if (user is null)
        {
            return new NoSuch<Account>();
        }

        if (!passwordVerifier.Verify(password, user.Password))
        {
            return new InvalidCredentials();
        }

        if (!user.HasBeenActivated())
        {
            return new AccountNotActivated();
        }

        var refreshToken = refreshTokensFactory.Generate();
        var session = new AuthSession(user.Id, dateTimeProvider.Now(), refreshToken);
        var accessToken = accessTokenService.Create(session);
        await authSessionsRepository.Create(session);
        await authSessionsRepository.Flush();

        return new TokenPairOutput(accessToken, refreshToken);
    }

    public async Task<Result> LogOut(Guid sessionId)
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

    public async Task<Result<TokenPairOutput>> RefreshTokens(string token)
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

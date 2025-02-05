using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class LogInUseCase(
    RefreshTokensFactory refreshTokensFactory,
    AccountsRepository accountsRepository,
    AccessTokenService accessTokenService,
    PasswordVerifier passwordVerifier,
    DateTimeProvider dateTimeProvider,
    AuthSessionsRepository authSessionsRepository
)
{
    public async Task<Result<TokenPairOutput>> Execute(string email, string password)
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
}

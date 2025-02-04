using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.Interactors;

public class AuthInteractor(
    UsersRepository usersRepository,
    PasswordVerifier passwordVerifier,
    RefreshTokensFactory refreshTokensFactory,
    RefreshTokensRepository refreshTokensRepository,
    AuthSessionsRepository authSessionsRepository,
    AccessTokenService accessTokenService
)
{
    public async Task<Result<TokenPairOutput>> LogIn(string email, string password)
    {
        var user = await usersRepository.FindByEmail(email);
        if (user is null)
        {
            return new NoSuch<User>();
        }

        if (!passwordVerifier.Verify(password, user.Password))
        {
            return new InvalidCredentials();
        }

        if (!user.HasBeenActivated())
        {
            return new AccountNotActivated();
        }

        var session = new AuthSession(user.Id);
        var tokenPair = await CreateTokenPair(session);
        await refreshTokensRepository.Flush();
        return tokenPair;
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
        var refreshToken = await refreshTokensRepository.FindByToken(token);

        if (refreshToken is null)
        {
            return new NoSuch<RefreshToken>();
        }

        var session = refreshToken.Session;

        if (refreshToken.IsInvalid())
        {
            await authSessionsRepository.Remove(session);
            await authSessionsRepository.Flush();
            return new InvalidToken();
        }

        refreshToken.Revoke();
        await refreshTokensRepository.Update(refreshToken);

        var tokenPair = await CreateTokenPair(session);

        await refreshTokensRepository.Flush();
        return tokenPair;
    }

    private async Task<TokenPairOutput> CreateTokenPair(AuthSession session)
    {
        var refreshToken = refreshTokensFactory.Create(session);
        await refreshTokensRepository.Persist(refreshToken);

        var accessToken = accessTokenService.Create(session);

        return new TokenPairOutput(accessToken, refreshToken.Token);
    }
}

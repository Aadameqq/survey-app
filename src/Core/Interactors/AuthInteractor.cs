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
    TokenService tokenService)
{
    public async Task<Result<TokenPairOutput>> LogIn(string email, string password)
    {
        var user = await usersRepository.FindByEmail(email);

        if (user is null)
        {
            return new NoSuchException<User>();
        }

        if (!passwordVerifier.Verify(password, user.Password))
        {
            return new InvalidCredentialsException<User>();
        }

        var session = new AuthSession()
        {
            UserId = user.Id
        };
        await authSessionsRepository.Persist(session);

        var tokenPair = await CreateTokenPair(session);

        await authSessionsRepository.Flush();
        await refreshTokensRepository.Flush();

        return tokenPair;
    }

    public async Task<Result> LogOut(Guid sessionId)
    {
        var session = await authSessionsRepository.FindById(sessionId);

        if (session is null)
        {
            return new NoSuchException<AuthSession>();
        }

        await DeleteSession(session);

        return Result.Success();
    }

    public async Task<Result<TokenPairOutput>> RefreshTokens(string token)
    {
        var refreshToken = await refreshTokensRepository.FindByToken(token);

        if (refreshToken is null)
        {
            return new NoSuchException<RefreshToken>();
        }

        var session = await authSessionsRepository.FindByRefreshToken(refreshToken);

        if (session is null)
        {
            return new NoSuchException<AuthSession>();
        }

        if (refreshToken.IsRevoked() || refreshToken.HasExpired())
        {
            await DeleteSession(session);
            return new InvalidCredentialsException<RefreshToken>();
        }

        refreshToken.Revoke();
        await refreshTokensRepository.Update(refreshToken);

        var tokenPair = await CreateTokenPair(session);

        await authSessionsRepository.Flush();
        await refreshTokensRepository.Flush();

        return tokenPair;
    }

    private async Task DeleteSession(AuthSession session)
    {
        await refreshTokensRepository.RemoveAllInSession(session);
        await authSessionsRepository.Remove(session);
        await authSessionsRepository.Flush();
        await refreshTokensRepository.Flush();
    }

    private async Task<TokenPairOutput> CreateTokenPair(AuthSession session)
    {
        var refreshToken = refreshTokensFactory.Create(session);
        await refreshTokensRepository.Persist(refreshToken);

        var accessToken = tokenService.CreateAccessToken(session);

        return new TokenPairOutput(accessToken, refreshToken.Token);
    }
}

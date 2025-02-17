using Core.Domain;
using Core.Dtos;

namespace Core.Ports;

public interface TokenService
{
    TokenPairOutput CreateTokenPair(Account account, Guid sessionId);
    string CreateAccessToken(Account account, Guid sessionId);
    string CreateRefreshToken(Account account, Guid sessionId);
    Task<AccessTokenPayload?> FetchPayloadIfValid(string accessToken);

    Task<RefreshTokenPayload?> FetchRefreshTokenPayloadIfValid(string refreshToken);
}

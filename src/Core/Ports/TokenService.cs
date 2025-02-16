using Core.Domain;
using Core.Dtos;

namespace Core.Ports;

public interface TokenService
{
    TokenPairOutput CreatePair(Account account, Guid sessionId);
    string CreateAccessToken(Account account, Guid sessionId);
    string CreateRefreshToken(Account account);

    Task<AccessTokenPayload?> FetchPayloadIfValid(string accessToken);
}

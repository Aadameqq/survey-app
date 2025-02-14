using Core.Domain;
using Core.Dtos;

namespace Core.Ports;

public interface AccessTokenService
{
    string Create(AuthSession session, Account account);

    Task<AccessTokenPayload?> FetchPayloadIfValid(string accessToken);
}

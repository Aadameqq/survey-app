using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.Interactors;

public class AccessTokenInteractor(AccessTokenService accessTokenService)
{
    public async Task<Result<AccessTokenPayload>> GetAccessTokenPayload(string accessToken)
    {
        var payload = await accessTokenService.FetchPayloadIfValid(accessToken);

        if (payload is null)
        {
            return new InvalidToken();
        }

        return payload;
    }
}

using Core.Domain;
using Core.Dtos;
using Core.Exceptions;
using Core.Ports;

namespace Core.UseCases;

public class GetAccountFromTokenUseCase(AccessTokenService accessTokenService)
{
    public async Task<Result<AccessTokenPayload>> Execute(string accessToken)
    {
        var payload = await accessTokenService.FetchPayloadIfValid(accessToken);

        if (payload is null)
        {
            return new InvalidToken();
        }

        return payload;
    }
}

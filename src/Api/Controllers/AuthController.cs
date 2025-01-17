using Api.Auth;
using Api.Dtos;
using Core.Domain;
using Core.Exceptions;
using Core.Interactors;
using Core.Ports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    AuthInteractor authInteractor
)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TokenPairResponse>> LogIn([FromBody] LogInBody body)
    {
        var result = await authInteractor.LogIn(body.Email, body.Password);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<User> _ => Unauthorized(),
                InvalidCredentials _ => Unauthorized(),
                _ => throw result.Exception
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }

    [HttpDelete]
    [RequireAuth]
    public async Task<IActionResult> LogOut(AuthorizedUser authUser)
    {
        var result = await authInteractor.LogOut(authUser.SessionId);

        if (result is { IsFailure: true, Exception: NoSuch<AuthSession> })
        {
            return Unauthorized();
        }

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult<TokenPairResponse>> RefreshTokens([FromBody] RefreshTokensBody body)
    {
        var result = await authInteractor.RefreshTokens(body.RefreshToken);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<RefreshToken> _ => Unauthorized(),
                InvalidToken _ => Unauthorized(),
                _ => throw result.Exception
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }
}

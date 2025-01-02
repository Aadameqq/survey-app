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
    AuthInteractor authInteractor,
    TokenService tokenService //TODO:
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
                NoSuchException<User> _ => Unauthorized(),
                InvalidCredentialsException<User> _ => Unauthorized(),
                _ => throw result.Exception
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> LogOut()
    {
        var sessionId = User.Claims.First(c => c.Type == tokenService.GetSessionIdClaimType()).Value;

        var result = await authInteractor.LogOut(Guid.Parse(sessionId));

        if (result is { IsFailure: true, Exception: NoSuchException<AuthSession> })
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
                NoSuchException<RefreshToken> _ => Unauthorized(),
                NoSuchException<AuthSession> _ => throw new InvalidOperationException(
                    "Session is null, indicating a logic error or database inconsistency."),
                InvalidCredentialsException<RefreshToken> _ => Unauthorized(),
                _ => throw result.Exception
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }
}

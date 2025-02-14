using Api.Auth;
using Api.Controllers.Dtos;
using Api.Dtos;
using Core.Domain;
using Core.Exceptions;
using Core.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    LogInUseCase logInUseCase,
    LogOutUseCase logOutUseCase,
    RefreshTokensUseCase refreshTokensUseCase
) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TokenPairResponse>> LogIn([FromBody] LogInBody body)
    {
        var result = await logInUseCase.Execute(body.Email, body.Password);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => Unauthorized(),
                InvalidCredentials _ => Unauthorized(),
                AccountNotActivated _ => Unauthorized(
                    new { message = "Account has not been activated yet" }
                ),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }

    [HttpDelete]
    [RequireAuth]
    public async Task<IActionResult> LogOut(AuthorizedUser authUser)
    {
        var result = await logOutUseCase.Execute(authUser.SessionId);

        if (result is { IsFailure: true, Exception: NoSuch<AuthSession> })
        {
            return Unauthorized();
        }

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult<TokenPairResponse>> RefreshTokens(
        [FromBody] RefreshTokensBody body
    )
    {
        var result = await refreshTokensUseCase.Execute(body.RefreshToken);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<AuthSession> _ => Unauthorized(),
                InvalidToken _ => Unauthorized(),
                _ => throw result.Exception,
            };
        }

        return new TokenPairResponse(result.Value.AccessToken, result.Value.RefreshToken);
    }
}

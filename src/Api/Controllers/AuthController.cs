using Api.Dtos;
using Api.Models;
using Api.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    PasswordVerifier passwordVerifier,
    UsersRepository usersRepository,
    TokenService tokenService,
    RefreshTokensRepository refreshTokensRepository,
    AuthSessionsRepository authSessionsRepository,
    RefreshTokensFactory refreshTokensFactory
)
    : ControllerBase
{
    [HttpPost]
    public ActionResult<TokenPairResponse> LogIn([FromBody] LogInBody body)
    {
        var user = usersRepository.FindByEmail(body.Email);

        if (user is null) return Unauthorized();

        if (!passwordVerifier.Verify(body.Password, user.Password)) return Unauthorized();

        var session = new AuthSession
        {
            UserId = user.Id
        };

        var refreshToken = refreshTokensFactory.Create(session);

        authSessionsRepository.Persist(session);
        refreshTokensRepository.Persist(refreshToken);

        var accessToken = tokenService.CreateAccessToken(session);

        return new TokenPairResponse(accessToken, refreshToken.Token);
    }

    [HttpDelete]
    [Authorize]
    public IActionResult LogOut()
    {
        var sessionId = Guid.Parse(User.Claims.First(c => c.Type == tokenService.GetSessionIdClaimType()).Value);

        var session = authSessionsRepository.FindById(sessionId);

        if (session is null)
        {
            return Unauthorized();
        }

        authSessionsRepository.Remove(session);
        refreshTokensRepository.RemoveAllInSession(session);

        return Ok();
    }

    [HttpPut]
    public ActionResult<TokenPairResponse> RefreshTokens([FromBody] RefreshTokensBody body)
    {
        var refreshToken = refreshTokensRepository.FindByToken(body.RefreshToken);

        if (refreshToken is null)
        {
            return Unauthorized();
        }

        var session = authSessionsRepository.FindByRefreshToken(refreshToken);

        if (session is null)
        {
            throw new InvalidOperationException("Session is null, indicating a logic error or database inconsistency.");
        }

        if (refreshToken.IsRevoked() || refreshToken.HasExpired())
        {
            refreshTokensRepository.RemoveAllInSession(session);
            authSessionsRepository.Remove(session);
            return Unauthorized();
        }

        refreshToken.Revoke();
        refreshTokensRepository.Update(refreshToken);

        var newRefreshToken = refreshTokensFactory.Create(session);

        refreshTokensRepository.Persist(newRefreshToken);

        var accessToken = tokenService.CreateAccessToken(session);

        return new TokenPairResponse(accessToken, newRefreshToken.Token);
    }
}

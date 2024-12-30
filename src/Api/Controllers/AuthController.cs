using Api.Dtos;
using Api.Models;
using Api.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    PasswordVerifier passwordVerifier,
    UsersRepository usersRepository,
    TokenService tokenService,
    TokensRepository tokensRepository)
    : ControllerBase
{
    [HttpPost]
    public ActionResult<LogInResponse> LogIn([FromBody] LogInBody body)
    {
        var user = usersRepository.FindByEmail(body.Email);

        if (user is null) return Unauthorized();

        if (!passwordVerifier.Verify(body.Password, user.Password)) return Unauthorized();

        var refreshTokenPayload = new RefreshTokenPayload
        {
            UserId = user.Id
        };
        tokensRepository.Create(refreshTokenPayload);

        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken(refreshTokenPayload, user);

        return new LogInResponse(accessToken, refreshToken);
    }

    [HttpDelete]
    public IActionResult LogOut()
    {
        return Ok();
    }

    [HttpPut]
    public IActionResult RefreshTokenPair()
    {
        throw new NotImplementedException();
    }
}

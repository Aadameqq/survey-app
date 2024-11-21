using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(DatabaseContext ctx) : ControllerBase
{
    [HttpPost]
    public ActionResult<LogInResponse> LogIn([FromBody] LogInBody body)
    {
        var user = ctx.Users.FirstOrDefault(u => u.Email == body.Email);

        if (user == null) return Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(body.Password, user.Password)) return Unauthorized();

        // Token creation

        return Ok();
    }

    [HttpDelete]
    public IActionResult LogOut()
    {
        return Ok();
    }
}

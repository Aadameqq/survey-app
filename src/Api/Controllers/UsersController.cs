using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(DatabaseContext ctx) : ControllerBase
{
    [HttpPost("")]
    public IActionResult Create([FromBody] CreateUserBody body)
    {
        var found = ctx.Users.FirstOrDefault(u => u.Email == body.Email);
        if (found != null) return Conflict("Email already exists");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(body.Password);

        var user = new User
        {
            Email = body.Email,
            UserName = body.UserName,
            Password = hashedPassword
        };
        ctx.Users.Add(user);
        ctx.SaveChanges();

        return Created(nameof(GetAuthenticated), new { });
    }

    [HttpPut("@me")]
    public ActionResult<GetAuthenticatedUserResponse> GetAuthenticated()
    {
        return NotFound();
    }
}

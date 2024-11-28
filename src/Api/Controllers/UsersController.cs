using Api.Dtos;
using Api.Models;
using Api.Models.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(UsersRepository usersRepository, PasswordHasher passwordHasher) : ControllerBase
{
    [HttpPost("")]
    public IActionResult Create([FromBody] CreateUserBody body)
    {
        var found = usersRepository.FindByEmail(body.Email);
        if (found != null) return Conflict("Email already exists");

        var hashedPassword = passwordHasher.HashPassword(body.Password);

        var user = new User
        {
            Email = body.Email,
            UserName = body.UserName,
            Password = hashedPassword
        };
        usersRepository.Create(user);

        return CreatedAtAction(nameof(GetAuthenticated), new { });
    }

    [HttpPut("@me")]
    public ActionResult<GetAuthenticatedUserResponse> GetAuthenticated()
    {
        return NotFound();
    }
}

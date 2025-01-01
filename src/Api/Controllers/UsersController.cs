using System.Security.Claims;
using Api.Dtos;
using Api.Models;
using Api.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet("@me")]
    [Authorize]
    public ActionResult<GetAuthenticatedUserResponse> GetAuthenticated()
    {
        var id = User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

        var user = usersRepository.FindById(Guid.Parse(id));
        if (user is null)
        {
            throw new InvalidOperationException(
                "The operation could not proceed because the user is logged in but does not exist in the database. This might indicate a corrupted session or data inconsistency.");
        }

        return Ok(new { user.Email });
    }
}

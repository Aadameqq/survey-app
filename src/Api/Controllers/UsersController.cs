using System.Security.Claims;
using Api.Auth;
using Api.Dtos;
using Core.Domain;
using Core.Exceptions;
using Core.Interactors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(UserInteractor userInteractor) : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateUserBody body)
    {
        var result = await userInteractor.Create(body.UserName, body.Email, body.Password);

        if (result.IsFailure)
        {
            if (result.Exception is AlreadyExists<User>)
            {
                return Conflict();
            }
        }

        return CreatedAtAction(nameof(GetAuthenticated), new { });
    }

    [HttpGet("@me")]
    [RequireAuth]
    public async Task<ActionResult<GetAuthenticatedUserResponse>> GetAuthenticated(AuthorizedUser user)
    {
        var result = await userInteractor.Get(user.UserId);

        if (result.IsFailure)
        {
            if (result.Exception is NoSuch<User>)
            {
                throw new InvalidOperationException(
                    "The operation could not proceed because the user is logged in but does not exist in the database. This might indicate a corrupted session or data inconsistency.");
            }
        }

        return Ok(new { result.Value.Email });
    }
}

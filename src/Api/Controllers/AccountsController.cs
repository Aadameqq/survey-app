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
public class AccountsController(
    CreateAccountUseCase createAccountUseCase,
    GetCurrentAccountUseCase getCurrentAccountUseCase
) : ControllerBase
{
    [HttpPost("")]
    public async Task<IActionResult> Create([FromBody] CreateAccountBody body)
    {
        var result = await createAccountUseCase.Execute(body.UserName, body.Email, body.Password);

        if (result.IsFailure)
        {
            if (result.Exception is AlreadyExists<Account>)
            {
                return Conflict();
            }
        }

        return CreatedAtAction(nameof(GetAuthenticated), new { });
    }

    [HttpGet("@me")]
    [RequireAuth]
    public async Task<ActionResult<GetAuthenticatedUserResponse>> GetAuthenticated(
        AuthorizedUser user
    )
    {
        var result = await getCurrentAccountUseCase.Execute(user.UserId);

        if (result.IsFailure)
        {
            if (result.Exception is NoSuch<Account>)
            {
                throw new InvalidOperationException(
                    "The operation could not proceed because the user is logged in but does not exist in the database. This might indicate a corrupted session or data inconsistency."
                );
            }
        }

        return Ok(new { result.Value.Email });
    }

    [HttpDelete("password")]
    public async Task<IActionResult> ResetPassword()
    {
        throw new NotImplementedException();
    }
}

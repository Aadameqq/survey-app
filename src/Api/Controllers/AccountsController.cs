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
    GetCurrentAccountUseCase getCurrentAccountUseCase,
    InitializePasswordResetUseCase initializePasswordResetUseCase,
    ActivateAccountUseCase activateAccountUseCase,
    ResetPasswordUseCase resetPasswordUseCase
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
    public async Task<IActionResult> InitializeResetPassword(
        [FromBody] InitializeResetPasswordBody body
    )
    {
        var result = await initializePasswordResetUseCase.Execute(body.Email);

        if (result is { IsFailure: true, Exception: NoSuch<Account> })
        {
            return NotFound();
        }

        return Ok(new { message = "Password reset email sent." });
    }

    [HttpGet("activation/{code}")]
    public async Task<IActionResult> Activate([FromRoute] string code)
    {
        var result = await activateAccountUseCase.Execute(code);

        if (result.IsFailure)
        {
            return result.Exception switch
            {
                NoSuch<Account> _ => NotFound(),
                NoSuch _ => NotFound(),
                _ => throw result.Exception,
            };
        }

        return Ok("Account Activated");
    }

    [HttpDelete("password/{code}")]
    public async Task<IActionResult> ResetPassword(
        [FromRoute] string code,
        [FromBody] ResetPasswordBody body
    )
    {
        var result = await resetPasswordUseCase.Execute(code, body.NewPassword);

        if (result is { IsFailure: true, Exception: NoSuch })
        {
            return NotFound();
        }

        return Ok();
    }
}

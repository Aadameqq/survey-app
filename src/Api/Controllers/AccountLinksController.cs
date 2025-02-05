using Core.Domain;
using Core.Exceptions;
using Core.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("account")]
[Controller]
public class AccountLinksController(ActivateAccountUseCase activateAccountUseCase) : ControllerBase
{
    [HttpGet("account-activation/{code}")]
    public async Task<IActionResult> VerifyEmail([FromRoute] string code)
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
}

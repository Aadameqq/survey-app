using Core.Domain;
using Core.Exceptions;
using Core.Interactors;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[Route("account")]
[Controller]
public class AccountLinksController(AccountInteractor accountInteractor) : ControllerBase
{
    [HttpGet("account-activation/{code}")]
    public async Task<IActionResult> VerifyEmail([FromRoute] string code)
    {
        var result = await accountInteractor.Activate(code);

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

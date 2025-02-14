using Microsoft.AspNetCore.Mvc;

namespace Api;

public static class ApiResponse
{
    public static ActionResult Forbid(string customMessage = "Forbidden")
    {
        return GenerateResponse(403, customMessage);
    }

    private static ActionResult GenerateResponse(int statusCode, string message)
    {
        return new JsonResult(new { message }) { StatusCode = statusCode };
    }
}

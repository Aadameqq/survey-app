using System.ComponentModel.DataAnnotations;

namespace Api.Controllers.Dtos;

public record CreateUserBody(
    [Required] string UserName,
    [Required] string Email,
    [Required][MinLength(12)] string Password
);

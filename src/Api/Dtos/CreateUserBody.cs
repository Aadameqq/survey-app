using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record CreateUserBody(
    [Required] string UserName,
    [Required] string Email,
    [Required] [MinLength(12)] string Password
);

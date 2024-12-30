using System.ComponentModel.DataAnnotations;

namespace Api;

public class JwtSettings
{
    [Required] public required string AccessTokenSecret { get; init; }
    [Required] public required int AccessTokenLifetimeInMinutes { get; init; }
    [Required] public required string RefreshTokenSecret { get; init; }
    [Required] public required int RefreshTokenLifetimeInMinutes { get; init; }
    [Required] public required string Issuer { get; init; }
}

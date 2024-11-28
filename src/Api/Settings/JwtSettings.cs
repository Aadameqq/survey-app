using System.ComponentModel.DataAnnotations;

namespace Api;

public class JwtSettings
{
    [Required] public required string Secret { get; init; }
}

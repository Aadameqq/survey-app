using System.ComponentModel.DataAnnotations;

namespace Api;

public class JwtConfig
{
    [Required] public required string Secret { get; init; }
}

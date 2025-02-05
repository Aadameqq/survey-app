using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class AccountOptions
{
    [Required]
    public required string ActivationUrl { get; init; }

    [Required]
    public required string PasswordResetUrl { get; init; }
}

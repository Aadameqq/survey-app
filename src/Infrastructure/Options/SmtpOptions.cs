using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class SmtpOptions
{
    [Required]
    public required string Host { get; init; }

    [Required]
    public required int Port { get; init; }

    [Required]
    public required string Email { get; init; }
}

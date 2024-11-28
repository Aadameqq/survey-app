using System.ComponentModel.DataAnnotations;

namespace Api;

public class DatabaseSettings
{
    [Required] public required string ConnectionString { get; init; }
}

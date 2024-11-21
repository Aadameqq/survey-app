using System.ComponentModel.DataAnnotations;

namespace Api;

public class DatabaseConfig
{
    [Required] public required string ConnectionString { get; init; }
}

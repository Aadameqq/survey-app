using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class DatabaseOptions
{
    [Required] public required string ConnectionString { get; init; }
}

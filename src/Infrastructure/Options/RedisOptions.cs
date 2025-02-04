using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Options;

public class RedisOptions
{
    [Required]
    public required string ConnectionString { get; init; }
}

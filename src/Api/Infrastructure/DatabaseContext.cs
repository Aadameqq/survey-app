using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Models;

public class DatabaseContext(IOptions<DatabaseSettings> databaseConfig) : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshTokenPayload> RefreshTokens { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(databaseConfig.Value.ConnectionString);
    }
}

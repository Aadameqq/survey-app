using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Api.Models;

public class DatabaseContext(IOptions<DatabaseConfig> databaseConfig) : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(databaseConfig.Value.ConnectionString);
    }
}

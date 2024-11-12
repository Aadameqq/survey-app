using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure;

public class PostgresContext : DbContext
{
    public DbSet<Greeting> Greetings { set; get; }

    private string ConnectionString = "Host=localhost;Database=survey-app;Username=testUser;Password=testPassword";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(ConnectionString);
}

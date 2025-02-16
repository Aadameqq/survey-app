using Core.Domain;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence.EF;

public class DatabaseContext(IOptions<DatabaseOptions> databaseConfig) : DbContext
{
    public DbSet<Account> Users { get; set; }
    public DbSet<ArchivedToken> ArchivedTokens { get; set; }
    public DbSet<AuthSession> AuthSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(databaseConfig.Value.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArchivedToken>(b =>
        {
            b.Property<Guid>("id").ValueGeneratedOnAdd();
            b.Property<string>("Token").HasColumnName("Token");

            modelBuilder
                .Entity<ArchivedToken>()
                .HasOne(a => a.Session)
                .WithMany()
                .HasForeignKey(a => a.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Account>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("activated").HasColumnName("Activated");

            b.Property(u => u.Role)
                .HasConversion(role => role.Name, name => Role.ParseOrFail(name))
                .HasColumnType("varchar(50)");

            b.OwnsMany<AuthSession>(
                "sessions",
                b2 =>
                {
                    b2.Property<Guid>("Id").ValueGeneratedOnAdd();
                    b2.Property<DateTime>("expiresAt").HasColumnName("ExpiresAt");
                    b2.Property<Guid>("UserId").HasColumnName("UserId");
                    b2.Property<string>("CurrentToken").HasColumnName("CurrentToken");
                    b2.WithOwner().HasForeignKey("UserId");
                }
            );
        });
    }
}

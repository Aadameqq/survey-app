using Core.Domain;
using Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Persistence;

public class DatabaseContext(IOptions<DatabaseOptions> databaseConfig) : DbContext
{
    public DbSet<Account> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuthSession> AuthSessions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(databaseConfig.Value.ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("revoked").HasColumnName("Revoked");
            b.Property<DateTime>("expiredAt").HasColumnName("ExpiredAt");

            b.HasOne(rt => rt.Session)
                .WithMany()
                .HasForeignKey("SessionId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AuthSession>(b =>
        {
            b.Property<Guid>("Id").ValueGeneratedOnAdd();
            b.Property<Guid>("UserId");
        });

        modelBuilder.Entity<Account>(b =>
        {
            b.Property<bool>("activated").HasColumnName("Activated");
        });
    }
}

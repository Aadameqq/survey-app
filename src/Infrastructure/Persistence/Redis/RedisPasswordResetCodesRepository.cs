using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis;

public class RedisPasswordResetCodesRepository
    : RedisConfirmationCodesRepository,
        PasswordResetCodesRepository
{
    private const string Prefix = "PasswordResetCodes";

    public RedisPasswordResetCodesRepository(
        IConnectionMultiplexer redis,
        IOptions<AccountOptions> accountOptions
    )
        : base(
            redis,
            Prefix,
            TimeSpan.FromMinutes(accountOptions.Value.PasswordResetCodeLifeSpanInMinutes)
        ) { }
}

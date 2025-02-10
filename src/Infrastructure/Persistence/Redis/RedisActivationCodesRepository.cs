using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis;

public class RedisActivationCodesRepository
    : RedisConfirmationCodesRepository,
        ActivationCodesRepository
{
    private const string Prefix = "ActivationCodes";

    public RedisActivationCodesRepository(
        IConnectionMultiplexer redis,
        IOptions<AccountOptions> accountOptions
    )
        : base(
            redis,
            Prefix,
            TimeSpan.FromMinutes(accountOptions.Value.ActivationCodeLifeSpanInMinutes)
        ) { }
}

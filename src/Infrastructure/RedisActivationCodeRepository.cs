using System.Security.Cryptography;
using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure;

public class RedisActivationCodeRepository(
    IConnectionMultiplexer redis,
    IOptions<AuthOptions> authOptions
) : ActivationCodeRepository
{
    public async Task<string> Create(User user)
    {
        var code = GenerateCode();

        var db = redis.GetDatabase();
        await db.StringSetAsync(
            code,
            user.Id.ToString(),
            TimeSpan.FromMinutes(authOptions.Value.ActivationCodeLifeSpanInMinutes)
        );

        return code;
    }

    public async Task<Guid?> GetUserIdAndRevokeCode(string code)
    {
        var db = redis.GetDatabase();
        var userId = await db.StringGetAsync(code);

        if (userId.IsNullOrEmpty)
        {
            return null;
        }

        await db.KeyDeleteAsync(code);

        return Guid.Parse(userId.ToString());
    }

    private string GenerateCode()
    {
        var rng = RandomNumberGenerator.Create();
        var randomNumber = new byte[4];

        rng.GetBytes(randomNumber);

        var generatedNumber = BitConverter.ToInt32(randomNumber, 0) & 0x7FFFFFFF;

        return (generatedNumber % 1000000).ToString("D6");
    }
}

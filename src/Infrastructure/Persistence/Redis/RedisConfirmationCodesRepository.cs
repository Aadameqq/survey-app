using System.Security.Cryptography;
using Core.Domain;
using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis;

public class RedisConfirmationCodesRepository(
    IConnectionMultiplexer redis,
    string prefix,
    TimeSpan codeLifeTime
)
{
    public async Task<string> Create(Account target)
    {
        var code = GenerateCode();

        var db = redis.GetDatabase();
        await db.StringSetAsync($"{prefix}{code}", target.Id.ToString(), codeLifeTime);

        return code;
    }

    public async Task<Guid?> GetAccountIdAndRevokeCode(string code)
    {
        var db = redis.GetDatabase();
        var accountId = await db.StringGetAsync($"{prefix}{code}");

        if (accountId.IsNullOrEmpty)
        {
            return null;
        }

        await db.KeyDeleteAsync($"{prefix}{code}");

        return Guid.Parse(accountId.ToString());
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

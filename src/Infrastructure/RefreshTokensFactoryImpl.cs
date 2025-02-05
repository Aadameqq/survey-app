using System.Security.Cryptography;
using Core.Ports;

namespace Infrastructure;

public class RefreshTokensFactoryImpl : RefreshTokensFactory
{
    public string Generate()
    {
        return GenerateRandomToken();
    }

    private string GenerateRandomToken()
    {
        var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

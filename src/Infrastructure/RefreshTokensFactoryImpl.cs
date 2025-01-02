using System.Security.Cryptography;
using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public class RefreshTokensFactoryImpl(IOptions<AuthOptions> jwtOptions) : RefreshTokensFactory
{
    public RefreshToken Create(AuthSession session)
    {
        return new RefreshToken
        {
            Session = session,
            Token = GenerateRandomToken(),
            LifeSpan = TimeSpan.FromMinutes(jwtOptions.Value.RefreshTokenLifetimeInMinutes)
        };
    }

    private string GenerateRandomToken()
    {
        var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[32];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

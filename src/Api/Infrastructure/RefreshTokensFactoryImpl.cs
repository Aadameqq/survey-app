using System.Security.Cryptography;
using Api.Models;
using Api.Models.Interfaces;
using Microsoft.Extensions.Options;

namespace Api.Infrastructure;

public class RefreshTokensFactoryImpl(IOptions<AuthSettings> jwtOptions) : RefreshTokensFactory
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

using System.Security.Cryptography;
using Core.Domain;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public class RefreshTokensFactoryImpl(IOptions<AuthOptions> authOptions) : RefreshTokensFactory
{
    public RefreshToken Create(AuthSession session)
    {
        var lifeSpan = authOptions.Value.RefreshTokenLifetimeInMinutes;
        var expires = DateTime.UtcNow.AddMinutes(lifeSpan);
        return new RefreshToken(session, expires, GenerateRandomToken());
    }

    private string GenerateRandomToken()
    {
        var rng = RandomNumberGenerator.Create();
        var randomBytes = new byte[64];
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

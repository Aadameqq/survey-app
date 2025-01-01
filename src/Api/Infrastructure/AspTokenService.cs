using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Api.Models;
using Api.Models.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Api.Infrastructure;

public class AspTokenService(IOptions<JwtSettings> jwtConfig) : TokenService
{
    public string CreateAccessToken(AuthSession session)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new(GetSessionIdClaimType(), session.Id.ToString())
        };

        return GenerateToken(
            claims,
            jwtConfig.Value.Issuer,
            jwtConfig.Value.AccessTokenSecret,
            jwtConfig.Value.AccessTokenLifetimeInMinutes
        );
    }

    public string GetSessionIdClaimType()
    {
        return "sessionId";
    }

    private static string GenerateToken(List<Claim> claims, string issuer, string secret, int lifetimeInMinutes)
    {
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            claims: claims,
            issuer: issuer,
            audience: "*",
            notBefore: now,
            expires: now.Add(TimeSpan.FromMinutes(lifetimeInMinutes)),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

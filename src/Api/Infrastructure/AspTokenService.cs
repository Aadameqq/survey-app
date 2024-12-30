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
    private const string RefreshTokenIdClaimType = "refresh";

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        return GenerateToken(
            claims,
            jwtConfig.Value.Issuer,
            jwtConfig.Value.AccessTokenSecret,
            jwtConfig.Value.AccessTokenLifetimeInMinutes
        );
    }


    public RefreshTokenPayload FetchRefreshTokenPayloadOrFail(string refreshToken)
    {
        var payload = new JwtSecurityTokenHandler().ValidateToken(refreshToken,
            new TokenValidationParameters
            {
                ValidateLifetime = true, ValidateIssuerSigningKey = true, ValidateIssuer = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Value.AccessTokenSecret)),
                ValidIssuer = jwtConfig.Value.Issuer
            },
            out _
        );

        var userId = payload.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        var tokenId = payload.Claims.First(c => c.Type == RefreshTokenIdClaimType).Value;

        return new RefreshTokenPayload
        {
            UserId = Guid.Parse(userId),
            Id = Guid.Parse(tokenId)
        };
    }

    public string GenerateRefreshToken(RefreshTokenPayload tokenPayload, User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(RefreshTokenIdClaimType, tokenPayload.Id.ToString())
        };

        return GenerateToken(
            claims,
            jwtConfig.Value.Issuer,
            jwtConfig.Value.RefreshTokenSecret,
            jwtConfig.Value.RefreshTokenLifetimeInMinutes
        );
    }

    private static string GenerateToken(List<Claim> claims, string issuer, string secret, int lifetimeInMinutes)
    {
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            claims: claims,
            issuer: issuer,
            notBefore: now,
            expires: now.Add(TimeSpan.FromMinutes(lifetimeInMinutes)),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                SecurityAlgorithms.HmacSha256Signature)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

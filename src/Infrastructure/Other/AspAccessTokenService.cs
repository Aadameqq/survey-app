using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Core.Domain;
using Core.Dtos;
using Core.Ports;
using Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure;

public class AspAccessTokenService(IOptions<AuthOptions> authOptions) : AccessTokenService
{
    private const string SessionIdClaimType = "sessionId";

    private readonly SymmetricSecurityKey signingKey = new(
        Encoding.UTF8.GetBytes(authOptions.Value.AccessTokenSecret)
    );

    public string Create(AuthSession session, Account account)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new(ClaimTypes.Role, account.Role.Name),
            new(SessionIdClaimType, session.Id.ToString()),
        };

        return GenerateToken(
            claims,
            authOptions.Value.Issuer,
            authOptions.Value.AccessTokenLifetimeInMinutes
        );
    }

    public async Task<AccessTokenPayload?> FetchPayloadIfValid(string accessToken)
    {
        var validationParameters = new TokenValidationParameters
        {
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = authOptions.Value.Issuer,
            ValidateAudience = false,
            IssuerSigningKey = signingKey,
        };
        try
        {
            var principal = await Task.Run(
                () =>
                    new JwtSecurityTokenHandler().ValidateToken(
                        accessToken,
                        validationParameters,
                        out var token
                    )
            );

            if (principal is null)
            {
                return null;
            }

            var userId = Guid.Parse(GetClaim(principal, ClaimTypes.NameIdentifier));
            var sessionId = Guid.Parse(GetClaim(principal, SessionIdClaimType));
            var role = GetClaim(principal, ClaimTypes.Role);

            return new AccessTokenPayload(userId, sessionId, Role.FromName(role).Value);
        }
        catch
        {
            return null;
        }
    }

    private static string GetClaim(ClaimsPrincipal principal, string claimType)
    {
        var claim = principal.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        if (claim is null)
        {
            throw new InvalidOperationException("No claim found for claim type");
        }

        return claim;
    }

    private string GenerateToken(List<Claim> claims, string issuer, int lifetimeInMinutes)
    {
        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            claims: claims,
            issuer: issuer,
            audience: "*",
            notBefore: now,
            expires: now.Add(TimeSpan.FromMinutes(lifetimeInMinutes)),
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256Signature
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

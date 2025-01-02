using Core.Domain;

namespace Core.Ports;

public interface TokenService
{
    string CreateAccessToken(AuthSession session);

    string GetSessionIdClaimType();
}

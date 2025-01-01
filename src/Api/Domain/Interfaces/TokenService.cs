namespace Api.Models.Interfaces;

public interface TokenService
{
    public string CreateAccessToken(AuthSession session);

    public string GetSessionIdClaimType();
}

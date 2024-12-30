namespace Api.Models.Interfaces;

public interface TokenService
{
    public string GenerateAccessToken(User user);

    public string GenerateRefreshToken(RefreshTokenPayload tokenPayload, User user);

    public RefreshTokenPayload FetchRefreshTokenPayloadOrFail(string refreshToken);
}

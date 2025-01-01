namespace Api.Models.Interfaces;

public interface RefreshTokensFactory
{
    public RefreshToken Create(AuthSession session);
}

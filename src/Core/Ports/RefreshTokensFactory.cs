using Core.Domain;

namespace Core.Ports;

public interface RefreshTokensFactory
{
    RefreshToken Create(AuthSession session);
}

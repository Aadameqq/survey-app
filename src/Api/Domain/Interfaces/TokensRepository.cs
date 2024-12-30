namespace Api.Models.Interfaces;

public interface TokensRepository
{
    public void Create(RefreshTokenPayload tokenPayload);
    public RefreshTokenPayload? FindByUser(Guid userId);
    public RefreshTokenPayload? FindById(Guid id);
}

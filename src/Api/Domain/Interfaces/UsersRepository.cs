namespace Api.Models.Interfaces;

public interface UsersRepository
{
    User? FindByEmail(string email);
    User? FindById(Guid id);
    void Create(User user);
}

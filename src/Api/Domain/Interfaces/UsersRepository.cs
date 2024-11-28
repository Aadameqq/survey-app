namespace Api.Models.Interfaces;

public interface UsersRepository
{
    User? FindByEmail(string email);
    void Create(User user);
}

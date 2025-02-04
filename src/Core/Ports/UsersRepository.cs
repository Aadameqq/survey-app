using Core.Domain;

namespace Core.Ports;

public interface UsersRepository
{
    Task<User?> FindByEmail(string email);
    Task<User?> FindById(Guid id);
    Task Create(User user);
    Task Update(User user);
    Task Flush();
}

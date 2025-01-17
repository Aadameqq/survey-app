using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Interactors;

public class UserInteractor(UsersRepository _usersRepository, PasswordHasher _passwordHasher)
{
    public async Task<Result> Create(string userName, string email, string plainPassword)
    {
        var found = await _usersRepository.FindByEmail(email);

        if (found != null) return new AlreadyExists<User>();

        var hashedPassword = _passwordHasher.HashPassword(plainPassword);

        var user = new User
        {
            Email = email,
            UserName = userName,
            Password = hashedPassword
        };

        await _usersRepository.Create(user);
        await _usersRepository.Flush();

        return Result.Success();
    }

    public async Task<Result<User>> Get(Guid id)
    {
        var found = await _usersRepository.FindById(id);
        if (found is null)
        {
            return new NoSuch<User>();
        }

        return found;
    }
}

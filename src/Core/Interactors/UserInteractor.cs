using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Interactors;

public class UserInteractor(UsersRepository usersRepository, PasswordHasher passwordHasher)
{
    public async Task<Result> Create(string userName, string email, string plainPassword)
    {
        var found = await usersRepository.FindByEmail(email);

        if (found != null) return new AlreadyExists<User>();

        var hashedPassword = passwordHasher.HashPassword(plainPassword);

        var user = new User
        {
            Email = email,
            UserName = userName,
            Password = hashedPassword
        };

        await usersRepository.Create(user);
        await usersRepository.Flush();

        return Result.Success();
    }

    public async Task<Result<User>> Get(Guid id)
    {
        var found = await usersRepository.FindById(id);
        if (found is null)
        {
            return new NoSuch<User>();
        }

        return found;
    }
}

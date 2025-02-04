using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Interactors;

public class UserInteractor(
    UsersRepository usersRepository,
    PasswordHasher passwordHasher,
    EmailSender emailSender,
    ActivationEmailBodyGenerator emailBodyGenerator,
    ActivationCodeRepository activationCodeRepository
)
{
    public async Task<Result> Create(string userName, string email, string plainPassword)
    {
        var found = await usersRepository.FindByEmail(email);

        if (found != null)
        {
            return new AlreadyExists<User>();
        }

        var hashedPassword = passwordHasher.HashPassword(plainPassword);

        var user = new User
        {
            Email = email,
            UserName = userName,
            Password = hashedPassword,
        };

        await usersRepository.Create(user);
        await usersRepository.Flush();

        var code = await activationCodeRepository.Create(user);

        var content = emailBodyGenerator.Generate(user, code);

        emailSender.Send(user.Email, "Account Activation", content);

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

    public async Task<Result> Activate(string code)
    {
        var userId = await activationCodeRepository.GetUserIdAndRevokeCode(code);

        if (userId is null)
        {
            return new NoSuch();
        }

        var user = await usersRepository.FindById(userId.Value);

        if (user is null)
        {
            return new NoSuch<User>();
        }

        user.Activate();

        await usersRepository.Update(user);
        await usersRepository.Flush();

        return Result.Success();
    }
}

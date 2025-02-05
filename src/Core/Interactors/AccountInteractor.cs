using Core.Domain;
using Core.Exceptions;
using Core.Ports;

namespace Core.Interactors;

public class AccountInteractor(
    AccountsRepository accountsRepository,
    PasswordHasher passwordHasher,
    EmailSender emailSender,
    ActivationEmailBodyGenerator emailBodyGenerator,
    ActivationCodesRepository activationCodesRepository
)
{
    public async Task<Result> Create(string userName, string email, string plainPassword)
    {
        var found = await accountsRepository.FindByEmail(email);

        if (found != null)
        {
            return new AlreadyExists<Account>();
        }

        var hashedPassword = passwordHasher.HashPassword(plainPassword);

        var user = new Account
        {
            Email = email,
            UserName = userName,
            Password = hashedPassword,
        };

        await accountsRepository.Create(user);
        await accountsRepository.Flush();

        var code = await activationCodesRepository.Create(user);

        var content = emailBodyGenerator.Generate(user, code);

        emailSender.Send(user.Email, "Account Activation", content);

        return Result.Success();
    }

    public async Task<Result<Account>> Get(Guid id)
    {
        var found = await accountsRepository.FindById(id);
        if (found is null)
        {
            return new NoSuch<Account>();
        }

        return found;
    }

    public async Task<Result> Activate(string code)
    {
        var userId = await activationCodesRepository.GetUserIdAndRevokeCode(code);

        if (userId is null)
        {
            return new NoSuch();
        }

        var user = await accountsRepository.FindById(userId.Value);

        if (user is null)
        {
            return new NoSuch<Account>();
        }

        user.Activate();

        await accountsRepository.Update(user);
        await accountsRepository.Flush();

        return Result.Success();
    }
}

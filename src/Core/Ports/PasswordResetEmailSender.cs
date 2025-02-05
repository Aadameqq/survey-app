using Core.Domain;

namespace Core.Ports;

public interface PasswordResetEmailSender
{
    public Task Send(Account account, string resetCode);
}

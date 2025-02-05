using Core.Domain;

namespace Core.Ports;

public interface ActivationCodeEmailSender
{
    public Task Send(Account account, string activationCode);
}

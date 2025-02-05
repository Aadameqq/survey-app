using Core.Domain;

namespace Core.Ports;

public interface ActivationEmailBodyGenerator
{
    public string Generate(Account account, string code);
}

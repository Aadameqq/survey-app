using Core.Domain;

namespace Core.Ports;

public interface ActivationEmailBodyGenerator
{
    public string Generate(User user, string code);
}

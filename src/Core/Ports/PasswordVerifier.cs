namespace Core.Ports;

public interface PasswordVerifier
{
    bool Verify(string plainPassword, string hashedPassword);
}

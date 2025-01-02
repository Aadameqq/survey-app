namespace Core.Ports;

public interface PasswordHasher
{
    string HashPassword(string plainPassword);
}

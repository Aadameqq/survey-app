using Core.Ports;

namespace Infrastructure;

public class BCryptPasswordService : PasswordHasher, PasswordVerifier
{
    public string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }

    public bool Verify(string plainPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}

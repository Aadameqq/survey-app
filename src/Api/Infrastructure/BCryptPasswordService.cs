using Api.Models.Interfaces;

namespace Api.Infrastructure;

public class BCryptPasswordService : PasswordHasher
{
    public string HashPassword(string plainPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(plainPassword);
    }
}

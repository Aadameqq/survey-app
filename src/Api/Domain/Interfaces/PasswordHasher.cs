namespace Api.Models.Interfaces;

public interface PasswordHasher
{
    public string HashPassword(string plainPassword);
}

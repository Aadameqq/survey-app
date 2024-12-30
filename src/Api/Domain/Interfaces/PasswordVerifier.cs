namespace Api.Models.Interfaces;

public interface PasswordVerifier
{
    public bool Verify(string plainPassword, string hashedPassword);
}

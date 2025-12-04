using Socialnetwork.Entityframework;
using System.Security.Cryptography;
using System.Text;

namespace SocialNetwork.Services;

public class AuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public (bool Success, string Message, string ErrorMessage) Login(string username, string password)
    {
        // 1: Hitta användaren
        var user = _context.Users.FirstOrDefault(u => u.Username == username);
        if (user == null)
        {
            return (false, "", "User not found");
        }

        // 2: Verifiera lösenord
        var hashed = HashPassword(password);
        if (hashed != user.PasswordHash)
        {
            return (false, "", "Incorrect password");
        }

        // 3: Success
        return (true, "Login successful", "");
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
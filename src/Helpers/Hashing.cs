namespace src;

public static class Hashing
{
    public static string HashPass(string password) =>
        BCrypt.Net.BCrypt.EnhancedHashPassword(password, 12);

    public static bool VerifyPassword(string password, string hashed) =>
        BCrypt.Net.BCrypt.EnhancedVerify(password, hashed);
    
}
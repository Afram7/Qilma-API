using System.Text.RegularExpressions;

namespace Qilma_API.Validators;

public class PasswordValidator
{
    private const int MIN_PASSWORD_LENGTH = 8;

    // Checks if the provided password matches the hashed password stored in the database
    public Task<bool> VerifyPasswordAsync(string password, string hashedPassword)
    {
        bool isValidPassword = BC.EnhancedVerify(password, hashedPassword);

        return Task.FromResult(isValidPassword);
    }

    // Validates the password based on certain criteria such as length, character types, and confirmation match
    public (bool IsValid, string? ErrorMessage) ValidatePassword(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Password is required");
        }
        if (password.Length < MIN_PASSWORD_LENGTH)
        {
            return (false, "Password too short");
        }
        if (password != confirmPassword)
        {
            return (false, "Passwords do not match");
        }
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return (false, "Need uppercase");
        }
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return (false, "Need lowercase");
        }
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            return (false, "Need digit");
        }
        if (!Regex.IsMatch(password, @"[\W_]"))
        {
            return (false, "Need special character");
        }
        
        return (true, null);
    }
}

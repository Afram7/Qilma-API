using Qilma_API.DTOs;

namespace Qilma_API.Services.Interfaces;

public interface ITokenService
{
    // Generates a sign-in JWT token for the given email and password
    Task<TokenResponseResult> GenerarteSignInTokenAsync(string email, string Password);

    // Generates a JWT token for the given email
    Task<string> GeneratePasswordResetTokenAsync(string email);

    // Validates the reset password JWT token
    Task<bool> ValidatePasswordResetTokenAsync(string token);

    // Extracts the email from the JWT token
    Task<string?> GetEmailFromTokenAsync(string token);
}

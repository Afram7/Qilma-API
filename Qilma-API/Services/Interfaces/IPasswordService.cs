using Qilma_API.DTOs;

namespace Qilma_API.Services.Interfaces;

public interface IPasswordService
{
    // Request password reset
    Task<ResetPasswordRequestResult> ResetPasswordRequestAsync(ResetPasswordRequestDTO request);

    // Confirm password reset
    Task<ResetPasswordResult> ResetPasswordConfirmAsync(ResetPasswordDTO resetPasswordDTO);

    // Update the user's password in the database by their email
    Task UpdatePasswordByEmailAsync(string email, string newPassword);

    // Hashes the password using BCrypt
    Task<string> HashPasswordAsync(string password);
}

using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface IPasswordService
{
    // Request password reset
    Task<ResetPasswordRequestResult> ResetPasswordRequestAsync(ResetPasswordRequestDTO request);

    // Confirm password reset
    Task<ResetPasswordResult> ResetPasswordConfirmAsync(ResetPasswordDTO resetPasswordDTO);
}

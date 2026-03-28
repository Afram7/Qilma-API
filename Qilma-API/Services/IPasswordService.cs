using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface IPasswordService
{
    // Request password reset
    Task<ResetPasswordRequestResult> ResetPasswordRequestAsync(string email);
}

using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface ITokenService
{
    // Generate a new token/sign in
    public Task<TokenResponseResult> GenerateTokenAsync(string email, string Password);
}

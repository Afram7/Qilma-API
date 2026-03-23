using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface IUserService
{
    // Create a new user
    Task<CreateUserResult> CreateUserAsync(CreateUserDTO user);

    // Get user's information
    Task<GetUserByIdResult> GetUserByIdAsync(int id);

    // Delete user's information
    public Task<DeleteUserByIdResult> DeleteUserByIdAsync(int id);
}
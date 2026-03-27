using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface IUserService
{
    // Create a new user
    Task<CreateUserResult> CreateUserAsync(CreateUserDTO user);

    // Get user's information
    Task<GetUserByIdResult> GetUserByIdAsync(int id);

    // Update user's information
    Task<UpdateUserByIdResult> UpdateUserByIdAsync(int id, UpdateUserDTO updatedUser);

    // Delete user's information
    Task<DeleteUserByIdResult> DeleteUserByIdAsync(int id);

    // Update user's password
    Task<UpdateUserPasswordResult> UpdateUserPasswordAsync(int id, UpdateUserPasswordDTO updatedPassword);

    // Get user's statistics
    Task<GetStatistcsResult> GetUserStatistcsAsync(int id);
}
using Qilma_API.DTOs;
using Qilma_API.Models;

namespace Qilma_API.Services.Interfaces;

public interface IUserService
{
    // Create a new user
    Task<CreateUserResult> CreateUserAsync(CreateUserDTO user);

    // Get user's information
    Task<GetUserByIdResult> GetUserByIdAsync(int userId);

    // Update user's information
    Task<UpdateUserByIdResult> UpdateUserByIdAsync(int userId, UpdateUserDTO updatedUser);

    // Delete user's information
    Task<DeleteUserByIdResult> DeleteUserByIdAsync(int userId);

    // Update user's password
    Task<UpdateUserPasswordResult> UpdateUserPasswordAsync(int userId, UpdateUserPasswordDTO updatedPassword);

    // Get user's statistics
    Task<GetStatisticsResult> GetUserStatistcsAsync(int userId);

    // Checks if the email exists in the database
    Task<bool> CheckEmailExistsAsync(string email);

    // Get user's name by email
    Task<string?> GetUserNameByEmailAsync(string email);

    // Fetches a user from the database by their email address
    Task<UserModel?> FetchUserByEmailAsync(string email);

    // Fetches a user from the database by their ID
    Task<UserModel?> FetchUserByIdAsync(int userId);
}
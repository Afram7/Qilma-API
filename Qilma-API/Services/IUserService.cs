using Qilma_API.DTOs;

namespace Qilma_API.Services;

public interface IUserService
{
    // Create a new user
    Task<UserDTO> CreateUserAsync(CreateUserDTO user);
}
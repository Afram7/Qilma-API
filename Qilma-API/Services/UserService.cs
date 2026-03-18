using Qilma_API.DTOs;
using Qilma_API.Models;

namespace Qilma_API.Services;

public class UserService : IUserService
{

    // Hardcoded data
    static readonly List<UserModel> _users = new List<UserModel>()
    {
        new() { UserId = 1, Name = "John Doe", Age = 30, Email = "john.doe@example.com", Password = "password123" },
        new() { UserId = 2, Name = "Jane Smith", Age = 25, Email = "jane.smith@example.com", Password = "password456" }
    };

    public async Task<UserDTO> CreateUserAsync(CreateUserDTO newUser)
    {
        var userModel = new UserModel
        {
            UserId = _users.Count + 1,
            Name = newUser.Name,
            Age = newUser.Age,
            Email = newUser.Email,
            Password = newUser.Password
        };
        _users.Add(userModel);
        return new UserDTO
        {
            UserId = userModel.UserId,
            Name = userModel.Name,
            Age = userModel.Age,
            Email = userModel.Email
        };
    }
}
using Qilma_API.DTOs;
using Qilma_API.Models;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class UserService : IUserService
{

    // Hardcoded data
    static readonly List<UserModel> _users = new List<UserModel>()
    {
        new() { UserId = 1, Name = "John Doe", Age = 30, Email = "john.doe@example.com", Password = "password123" },
        new() { UserId = 2, Name = "Jane Smith", Age = 25, Email = "jane.smith@example.com", Password = "password456" }
    };

    private readonly UserValidator _userValidator;

    public UserService(UserValidator userValidator){
        _userValidator = userValidator;
    }
    private bool IsEmailTaken(string email)
    {
        return _users.Any(user => user.Email == email);
    }

    public async Task<CreateUserResult> CreateUserAsync(CreateUserDTO newUser)
    {
        
        var validation = _userValidator.ValidateNewUser(newUser);
        if (!validation.isValid)
        {
            return new CreateUserResult
            {
                IsValid = false,
                ErrorMessage = validation.errorMessage
            };
        }
        if (IsEmailTaken(newUser.Email))
        {
            return new CreateUserResult
            {
                Conflict = true,
                ErrorMessage = "Email already exists"
            };
        }

        try
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
            return new CreateUserResult
            {
                IsValid = true,
                User = new UserDTO
                {
                    UserId = userModel.UserId,
                    Name = userModel.Name,
                    Age = userModel.Age,
                    Email = userModel.Email
                }
            };
        }
        catch (Exception)
        {
            return new CreateUserResult
            {
                Failed = true,
                ErrorMessage = "Internal server error"
            };
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class UserService : IUserService
{

    private const string INTERNAL_ERROR_MESSAGE = "Internal server error";
    private readonly UserValidator _userValidator;
    private readonly AppDbContext _db;

    public UserService(UserValidator userValidator, AppDbContext db){
        _userValidator = userValidator;
        _db = db;
    }

    // Validates if the email already exists in the database
    private async Task<(bool EmailExists, string? ErrorMessage)> ValidateEmailExistsAsync(string email)
    {
        bool emailExists = await _db.Users.AnyAsync(user => user.Email == email);
        return emailExists ? (true, "Email already exists") : (false, null);
    }

    // Hashes the password using BCrypt with a work factor of 13
    private async Task<string> HashPassword(string password)
    {
        string hashedPassword = await Task.Run(() => BC.EnhancedHashPassword(password, 13));
        return hashedPassword;
    }

    // Validates if a user with the given ID exists in the database
    private async Task<(UserModel? User, bool UserExists, string? ErrorMessage)> ValidateUserExistsAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        return user == null ? (null, false, "User not found") : (user, true, null);
    }
    public async Task<CreateUserResult> CreateUserAsync(CreateUserDTO newUser)
    {
        var validation = _userValidator.ValidateNewUser(newUser);
        if (!validation.IsValid)
        {
            return new CreateUserResult
            {
                IsValid = false,
                ErrorMessage = validation.ErrorMessage
            };
        }

        var emailResult = await ValidateEmailExistsAsync(newUser.Email);
        if (emailResult.EmailExists)
        {
            return new CreateUserResult
            {
                Conflict = true,
                ErrorMessage = emailResult.ErrorMessage
            };
        }

        try
        {
            string hashedPassword = await HashPassword(newUser.Password);
            var userModel = new UserModel
            {
                Name = newUser.Name,
                Age = newUser.Age,
                Email = newUser.Email,
                Password = hashedPassword
            };
            _db.Users.Add(userModel);
            await _db.SaveChangesAsync();
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new CreateUserResult
            {
                Failed = true,
                ErrorMessage = INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<GetUserByIdResult> GetUserByIdAsync(int id)
    {
        try
        {
            var userResult = await ValidateUserExistsAsync(id);
            if (!userResult.UserExists)
            {
                return new GetUserByIdResult
                {
                    NotFound = true,
                    ErrorMessage = userResult.ErrorMessage
                };
            }

            return new GetUserByIdResult
            {
                User = new UserDTO
                {
                    UserId = userResult.User!.UserId,
                    Name = userResult.User.Name,
                    Age = userResult.User.Age,
                    Email = userResult.User.Email
                }
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new GetUserByIdResult
            {
                Failed = true,
                ErrorMessage = INTERNAL_ERROR_MESSAGE
            };
        }
    }
}
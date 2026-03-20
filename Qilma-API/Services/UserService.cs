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
    private readonly AppDbContext _context;

    public UserService(UserValidator userValidator, AppDbContext context){
        _userValidator = userValidator;
        _context = context;
    }

    // Validates if the email already exists in the database
    private async Task<(bool EmailExists, string? ErrorMessage)> ValidateEmailAsync(string email)
    {
        bool emailExists = await _context.Users.AnyAsync(u => u.Email == email);
        return emailExists ? (true, "Email already exists") : (false, null);
    }

    // Hashes the password using BCrypt with a work factor of 13
    private async Task<string> HashPassword(string password)
    {
        string hashedPassword = await Task.Run(() => BC.EnhancedHashPassword(password, 13));
        return hashedPassword;
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

        var emailResult = await ValidateEmailAsync(newUser.Email);
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
            _context.Users.Add(userModel);
            await _context.SaveChangesAsync();
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
}
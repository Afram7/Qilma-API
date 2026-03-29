using Microsoft.EntityFrameworkCore;
using MimeKit;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;
using Qilma_API.Services.Interfaces;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class UserService : IUserService
{
    private readonly UserValidator _userValidator;
    private readonly PasswordValidator _passwordValidator;
    private readonly IPasswordService _passwordService;
    private readonly IStatisticService _statisticService;
    private readonly IEmailService _emailService;
    private readonly IGameService _gameService;
    private readonly AppDbContext _db;
    private readonly ILogger<UserService> _logger;

    public UserService(UserValidator userValidator, PasswordValidator passwordValidator, IPasswordService passwordService, IStatisticService statisticService, IEmailService emailService, IGameService gameService, AppDbContext db, ILogger<UserService> logger){
        _userValidator = userValidator;
        _passwordValidator = passwordValidator;
        _passwordService = passwordService;
        _statisticService = statisticService;
        _emailService = emailService;
        _gameService = gameService;
        _db = db;
        _logger = logger;
    }

    public async Task<bool> CheckEmailExistsAsync(string email)
    {
        bool emailExists = await _db.Users.AnyAsync(u => u.Email == email);

        return emailExists;
    }

    public async Task<string?> GetUserNameByEmailAsync(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        
        return user?.Name;
    }

    public async Task<UserModel?> FetchUserByEmailAsync(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);

        return user;
    }
    
    public async Task<UserModel?> FetchUserByIdAsync(int userId)
    {
        var user = await _db.Users.FindAsync(userId);

        return user;
    }
    
    public async Task<CreateUserResult> CreateUserAsync(CreateUserDTO newUser)
    {
        try
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

            var emailExists = await CheckEmailExistsAsync(newUser.Email);
            if (emailExists)
            {
                return new CreateUserResult
                {
                    Conflict = true,
                    ErrorMessage = ErrorMessages.EMAIL_ALREADY_EXISTS
                };
            }

            string hashedPassword = await _passwordService.HashPasswordAsync(newUser.Password);
            var userModel = new UserModel
            {
                Name = newUser.Name,
                Age = newUser.Age,
                Email = newUser.Email,
                Password = hashedPassword
            };

            using var transaction = await _db.Database.BeginTransactionAsync();
            _db.Users.Add(userModel);
            await _db.SaveChangesAsync();
            await _statisticService.CreateStatisticForNewUserAsync(userModel.UserId);
            await transaction.CommitAsync();

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
            _logger.LogError(ex, "Error creating user with email: {Email}", newUser.Email);
            return new CreateUserResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<GetUserByIdResult> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await FetchUserByIdAsync(userId);
            if (user == null)
            {
                return new GetUserByIdResult
                {
                    NotFound = true,
                    ErrorMessage = ErrorMessages.USER_NOT_FOUND
                };
            }

            return new GetUserByIdResult
            {
                User = new UserDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Age = user.Age,
                    Email = user.Email
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user with userId: {UserId}", userId);
            return new GetUserByIdResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<UpdateUserByIdResult> UpdateUserByIdAsync(int userId, UpdateUserDTO updatedUser)
    {
        try
        {
            var user = await FetchUserByIdAsync(userId);
            if (user == null)
            {
                return new UpdateUserByIdResult
                {
                    NotFound = true,
                    ErrorMessage = ErrorMessages.USER_NOT_FOUND
                };
            }

            var validation = _userValidator.ValidateUpdatedUser(updatedUser);
            if (!validation.IsValid)
            {
                return new UpdateUserByIdResult
                {
                    IsValid = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            user.Name = updatedUser.Name!;
            user.Age = updatedUser.Age!.Value;
            user.Email = updatedUser.Email!;

            await _db.SaveChangesAsync();
            
            return new UpdateUserByIdResult
            {
                IsValid = true,
                User = new UserDTO
                {
                    UserId = user.UserId,
                    Name = user.Name,
                    Age = user.Age,
                    Email = user.Email
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user with userId: {UserId}", userId);
            return new UpdateUserByIdResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<DeleteUserByIdResult> DeleteUserByIdAsync(int userId)
    {
        try
        {
            var user = await FetchUserByIdAsync(userId);
            if (user == null)
            {
                return new DeleteUserByIdResult
                {
                    NotFound = true,
                    ErrorMessage = ErrorMessages.USER_NOT_FOUND
                };
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            await _statisticService.DeleteStatisticForUserAsync(user.UserId);
            await _gameService.DeleteGamesByUserIdAsync(user.UserId);
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            var email = user.Email;
            var name = user.Name;
            string subject = "Qilma Account Deletion Confirmation";
            var builder = new BodyBuilder
            {
                HtmlBody = File.ReadAllText("Templates/AccountDeleted.html")
                    .Replace("{{NAME}}", name)
                    .Replace("{{YEAR}}", DateTime.Now.Year.ToString())
            };

            await _emailService.SendEmailAsync(email, name, subject, builder);

            return new DeleteUserByIdResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user with userId: {UserId}", userId);
            return new DeleteUserByIdResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<UpdateUserPasswordResult> UpdateUserPasswordAsync(int userId, UpdateUserPasswordDTO updatedPassword)
    {
        try
        {
            var user = await FetchUserByIdAsync(userId);
            if (user == null)
            {
                return new UpdateUserPasswordResult
                {
                    NotFound = true,
                    ErrorMessage = ErrorMessages.USER_NOT_FOUND
                };
            }
            
            var isValidPassword = await _passwordValidator.VerifyPasswordAsync(updatedPassword.CurrentPassword, user.Password);
            if (!isValidPassword)
            {
                return new UpdateUserPasswordResult
                {
                    IsValid = false,
                    ErrorMessage = ErrorMessages.INVALID_CURRENT_PASSWORD
                };
            }

            var validation = _userValidator.ValidateUpdatedPassword(updatedPassword.NewPassword, updatedPassword.ConfirmPassword);
            if (!validation.IsValid)
            {
                return new UpdateUserPasswordResult
                {
                    IsValid = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            string hashedPassword = await _passwordService.HashPasswordAsync(updatedPassword.NewPassword);
            user.Password = hashedPassword;
            await _db.SaveChangesAsync();

            var email = user.Email;
            var name = user.Name;
            string subject = "Qilma Password Update Successful";
            var builder = new BodyBuilder
            {
                HtmlBody = File.ReadAllText("Templates/PasswordUpdated.html")
                    .Replace("{{NAME}}", name)
                    .Replace("{{YEAR}}", DateTime.Now.Year.ToString())
            };

            await _emailService.SendEmailAsync(email, name, subject, builder);
            
            return new UpdateUserPasswordResult
            {
                IsValid = true,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for userId: {UserId}", userId);
            return new UpdateUserPasswordResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<GetStatisticsResult> GetUserStatistcsAsync(int userId)
    {
        try
        {
            var statistic = await _statisticService.FetchUserStatisticsByIdAsync(userId);
            if (statistic == null)
            {
                return new GetStatisticsResult
                {
                    NotFound = true,
                    ErrorMessage = ErrorMessages.STATISTIC_NOT_FOUND
                };
            }
            
            return new GetStatisticsResult
            {
                Statistic = new StatisticDTO
                {
                    StatisticId = statistic.StatisticId,
                    OwnerId = statistic.OwnerId,
                    OwnerType = statistic.OwnerType,
                    GamesPlayed = statistic.GamesPlayed,
                    GamesWon = statistic.GamesWon
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching statistics for userId: {UserId}", userId);
            return new GetStatisticsResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}
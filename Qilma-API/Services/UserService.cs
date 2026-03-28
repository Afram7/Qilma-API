using Microsoft.EntityFrameworkCore;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class UserService : IUserService
{

    private readonly UserValidator _userValidator;
    private readonly AppDbContext _db;

    public UserService(UserValidator userValidator, AppDbContext db){
        _userValidator = userValidator;
        _db = db;
    }

    // Checks if the email already exists in the database
    private async Task<bool> CheckEmailExistsAsync(string email)
    {
        bool emailExists = await _db.Users.AnyAsync(user => user.Email == email);
        return emailExists;
    }

    // Hashes the password using BCrypt with a work factor of 13
    public static async Task<string> HashPasswordAsync(string password)
    {
        string hashedPassword = await Task.Run(() => BC.EnhancedHashPassword(password, 13));
        return hashedPassword;
    }

    // Returns the user with the specified ID, or null if not found
    private async Task<UserModel?> FetchUserByIdAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        return user;
    }

    // Returns the user's statistics with the specified ID, or null if not found
    private async Task<StatisticModel?> FetchUserStatisticsByIdAsync(int id)
    {
        var statistic = await _db.Statistics.FirstOrDefaultAsync(statistics => statistics.OwnerId == id && statistics.OwnerType == "user");
        return statistic;
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

        try
        {
            var emailExists = await CheckEmailExistsAsync(newUser.Email);
            if (emailExists)
            {
                return new CreateUserResult
                {
                    Conflict = true,
                    ErrorMessage = HttpErrorMessages.EMAIL_ALREADY_EXISTS
                };
            }

            string hashedPassword = await HashPasswordAsync(newUser.Password);
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

            var statisticModel = new StatisticModel
            {
                OwnerId = userModel.UserId,
                OwnerType = "user",
                GamesPlayed = 0,
                GamesWon = 0
            };

            _db.Statistics.Add(statisticModel);
            await _db.SaveChangesAsync();
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
            Console.WriteLine(ex.Message);
            return new CreateUserResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<GetUserByIdResult> GetUserByIdAsync(int id)
    {
        try
        {
            var user = await FetchUserByIdAsync(id);
            if (user == null)
            {
                return new GetUserByIdResult
                {
                    NotFound = true,
                    ErrorMessage = HttpErrorMessages.USER_NOT_FOUND
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
            Console.WriteLine(ex.Message);
            return new GetUserByIdResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<UpdateUserByIdResult> UpdateUserByIdAsync(int id, UpdateUserDTO updatedUser)
    {
        try
        {
            var user = await FetchUserByIdAsync(id);
            if (user == null)
            {
                return new UpdateUserByIdResult
                {
                    NotFound = true,
                    ErrorMessage = HttpErrorMessages.USER_NOT_FOUND
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

            if (updatedUser.Name != null)
            {
                user.Name = updatedUser.Name;
            }
            if (updatedUser.Age.HasValue)
            {
                user.Age = updatedUser.Age.Value;
            }
            if (updatedUser.Email != null)
            {
                user.Email = updatedUser.Email;
            }

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
            Console.WriteLine(ex.Message);
            return new UpdateUserByIdResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<DeleteUserByIdResult> DeleteUserByIdAsync(int id)
    {
        try
        {
            var user = await FetchUserByIdAsync(id);
            if (user == null)
            {
                return new DeleteUserByIdResult
                {
                    NotFound = true,
                    ErrorMessage = HttpErrorMessages.USER_NOT_FOUND
                };
            }

            using var transaction = await _db.Database.BeginTransactionAsync();
            var statistic = await _db.Statistics.FirstOrDefaultAsync(statistics => statistics.OwnerId == user.UserId && statistics.OwnerType == "user");
            if (statistic != null)
            {
                _db.Statistics.Remove(statistic);
            }

            var games = await _db.Games.Where(game => game.OwnerId == user.UserId && game.OwnerType == "user").ToListAsync();
            _db.Games.RemoveRange(games);

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new DeleteUserByIdResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new DeleteUserByIdResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<UpdateUserPasswordResult> UpdateUserPasswordAsync(int id, UpdateUserPasswordDTO updatedPassword)
    {
        try
        {
            var user = await FetchUserByIdAsync(id);
            if (user == null)
            {
                return new UpdateUserPasswordResult
                {
                    NotFound = true,
                    ErrorMessage = HttpErrorMessages.USER_NOT_FOUND
                };
            }
            
            var isValidPassword = await PasswordValidator.VerifyPasswordAsync(updatedPassword.CurrentPassword, user.Password);
            if (!isValidPassword)
            {
                return new UpdateUserPasswordResult
                {
                    IsValid = false,
                    ErrorMessage = HttpErrorMessages.INVALID_CURRENT_PASSWORD
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

            string hashedPassword = await HashPasswordAsync(updatedPassword.NewPassword);
            user.Password = hashedPassword;
            await _db.SaveChangesAsync();
            
            return new UpdateUserPasswordResult
            {
                IsValid = true,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new UpdateUserPasswordResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<GetStatistcsResult> GetUserStatistcsAsync(int id)
    {
        try
        {
            var statistic = await FetchUserStatisticsByIdAsync(id);
            if (statistic == null)
            {
                return new GetStatistcsResult
                {
                    NotFound = true,
                    ErrorMessage = HttpErrorMessages.STATISTIC_NOT_FOUND
                };
            }
            
            return new GetStatistcsResult
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
            Console.WriteLine(ex.Message);
            return new GetStatistcsResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}
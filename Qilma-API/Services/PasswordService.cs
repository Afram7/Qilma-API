using Microsoft.EntityFrameworkCore;
using MimeKit;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Services.Interfaces;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class PasswordService : IPasswordService
{
    private const int TOKEN_EXPIRATION_TIME = 15;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly EmailValidator _emailValidator;
    private readonly PasswordValidator _passwordValidator;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(AppDbContext db, IConfiguration config, IEmailService emailService, ITokenService tokenService, EmailValidator emailValidator, PasswordValidator passwordValidator, ILogger<PasswordService> logger)
    {
        _db = db;
        _config = config;
        _emailService = emailService;
        _tokenService = tokenService;
        _emailValidator = emailValidator;
        _passwordValidator = passwordValidator;
        _logger = logger;
    }

    public async Task UpdatePasswordByEmailAsync(string email, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.Password = await HashPasswordAsync(newPassword);
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<string> HashPasswordAsync(string password)
    {
        string hashedPassword = BC.EnhancedHashPassword(password, 13);
        return hashedPassword;
    }

    public async Task<ResetPasswordRequestResult> ResetPasswordRequestAsync(ResetPasswordRequestDTO request)
    {
        try
        {
            var validation = _emailValidator.ValidateEmail(request.Email);
            if (!validation.IsValid)
            {
                return new ResetPasswordRequestResult
                {
                    IsValid = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user != null)
            {
                var token = await _tokenService.GeneratePasswordResetTokenAsync(request.Email);
                var email = user.Email;
                var name = user.Name;
                var frontendUrl = _config["FrontendUrl:Url"];
                var resetLink = $"{frontendUrl}/rp?token={token}";
                string subject = "Qilma Reset Password";
                var builder = new BodyBuilder
                {
                    HtmlBody = File.ReadAllText("Templates/ResetPassword.html")
                        .Replace("{{NAME}}", name)
                        .Replace("{{RESET_LINK}}", resetLink)
                        .Replace("{{TOKEN_TIME}}", TOKEN_EXPIRATION_TIME.ToString())
                        .Replace("{{YEAR}}", DateTime.Now.Year.ToString())
                };

                await _emailService.SendEmailAsync(email, name, subject, builder);

                return new ResetPasswordRequestResult
                {
                    IsValid = true,
                    SuccessMessage = SuccessMessages.PASSWORD_RESET_LINK_SENT
                };
            }
            else
            {
                return new ResetPasswordRequestResult
                {
                    IsValid = true,
                    SuccessMessage = SuccessMessages.PASSWORD_RESET_LINK_SENT
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing password reset request for email: {Email}", request.Email);
            return new ResetPasswordRequestResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<ResetPasswordResult> ResetPasswordConfirmAsync(ResetPasswordDTO resetPasswordDTO)
    {
        try
        {
            var isValidToken = await _tokenService.ValidatePasswordResetTokenAsync(resetPasswordDTO.Token);
            if (!isValidToken)
            {
                return new ResetPasswordResult
                {
                    Forbidden = true,
                    ErrorMessage = ErrorMessages.INVALID_TOKEN
                };
            }

            var validation = _passwordValidator.ValidatePassword(resetPasswordDTO.NewPassword, resetPasswordDTO.ConfirmPassword);
            if (!validation.IsValid)
            {
                return new ResetPasswordResult
                {
                    IsValid = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }
            
            var email = await _tokenService.GetEmailFromTokenAsync(resetPasswordDTO.Token);
            if (email == null)
            {
                return new ResetPasswordResult
                {
                    Forbidden = true,
                    ErrorMessage = ErrorMessages.INVALID_TOKEN
                };
            }

            await UpdatePasswordByEmailAsync(email, resetPasswordDTO.NewPassword);
            
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            var name = user!.Name;
            string subject = "Qilma Password Reset Successful";
            var builder = new BodyBuilder
            {
                HtmlBody = File.ReadAllText("Templates/PasswordUpdated.html")
                    .Replace("{{NAME}}", name)
                    .Replace("{{YEAR}}", DateTime.Now.Year.ToString())
            };

            await _emailService.SendEmailAsync(email, name!, subject, builder);

            return new ResetPasswordResult
            {
                IsValid = true,
                SuccessMessage = SuccessMessages.PASSWORD_RESET_SUCCESS
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password with token: {Token}", resetPasswordDTO.Token);
            return new ResetPasswordResult
            {
                Failed = true,
                ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}

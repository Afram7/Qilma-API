using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class PasswordService : IPasswordService
{
    private const int TOKEN_EXPIRATION_TIME = 15;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public PasswordService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // Checks if the email exists in the database
    private async Task<bool> CheckEmailExistsAsync(string email)
    {
        bool emailExists = await _db.Users.AnyAsync(user => user.Email == email);
        return emailExists;
    }

    // Get user's name by email
    private async Task<string?> GetUserNameByEmailAsync(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(user => user.Email == email);
        return user?.Name;
    }

    // Generates a JWT token for the given email
    private string GenerateToken(string email)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim("Email", email),
            ],
            expires: DateTime.UtcNow.AddMinutes(TOKEN_EXPIRATION_TIME),
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        );
        
        var tokenString = tokenHandler.WriteToken(token);
        return tokenString;
    }

    // Validates the JWT token and extracts the email claim
    private ClaimsPrincipal? GetPrincipalFromToken(string token)
    {
        var jwtSettings = _config.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);
        var tokenHandler = new JwtSecurityTokenHandler();

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"],
            ValidateLifetime = true,
        };

        try
        {
            return tokenHandler.ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }

    // Extracts the email from the JWT token
    private string? GetEmailFromToken(string token)
    {
        var principal = GetPrincipalFromToken(token);
        return principal?.Claims.FirstOrDefault(claim => claim.Type == "Email")?.Value;
    }

    // Sends an email with the given subject and body to the specified email address
    private async Task SendEmailAsync(string email, string name, string subject, BodyBuilder builder)
    {
        var message = new MimeMessage();
        message.From.Add (new MailboxAddress("Qilma", "qilma.game@gmail.com"));
        message.To.Add (new MailboxAddress (name, email));
        message.Subject = subject;

        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("qilma.game@gmail.com", "zcuj zpls jhdx xmkv");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    // update the user's password in the database
    private async Task UpdatePasswordAsync(string email, string newPassword)
    {
        var user = await _db.Users.FirstOrDefaultAsync(user => user.Email == email);
        if (user != null)
        {
            user.Password = await UserService.HashPasswordAsync(newPassword);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<ResetPasswordRequestResult> ResetPasswordRequestAsync(ResetPasswordRequestDTO request)
    {
        try
        {
            var validation = EmailValidator.ValidateEmail(request.Email);
            if (!validation.IsValid)
            {
                return new ResetPasswordRequestResult
                {
                    IsValid = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            var emailExists = await CheckEmailExistsAsync(request.Email);
            if (emailExists)
            {
                var token = GenerateToken(request.Email);
                var name = await GetUserNameByEmailAsync(request.Email);
                var frontendUrl = _config["FrontendUrl:Url"];
                var resetLink = $"{frontendUrl}/rp?token={token}";
                string subject = "Qilma Reset Password";
                var builder = new BodyBuilder();
                builder.HtmlBody = File.ReadAllText("Templates/ResetPassword.html")
                    .Replace("{{NAME}}", name)
                    .Replace("{{RESET_LINK}}", resetLink)
                    .Replace("{{TOKEN_TIME}}", TOKEN_EXPIRATION_TIME.ToString())
                    .Replace("{{YEAR}}", DateTime.Now.Year.ToString());
                    
                await SendEmailAsync(request.Email, name!, subject, builder);

                return new ResetPasswordRequestResult
                {
                    IsValid = true,
                    SuccessMessage = HttpSuccessMessages.PASSWORD_RESET_LINK_SENT
                };
            }
            else
            {
                return new ResetPasswordRequestResult
                {
                    IsValid = true,
                    SuccessMessage = HttpSuccessMessages.PASSWORD_RESET_LINK_SENT
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new ResetPasswordRequestResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }

    public async Task<ResetPasswordResult> ResetPasswordConfirmAsync(ResetPasswordDTO resetPasswordDTO)
    {
        try
        {
            var principal = GetPrincipalFromToken(resetPasswordDTO.Token);
            if (principal == null)
            {
                return new ResetPasswordResult
                {
                    Forbidden = true,
                    ErrorMessage = HttpErrorMessages.INVALID_TOKEN
                };
            }

            var validation = PasswordValidator.ValidatePassword(resetPasswordDTO.NewPassword, resetPasswordDTO.ConfirmPassword);
            if (!validation.IsValid)
            {
                return new ResetPasswordResult
                {
                    IsValid = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }
            
            var email = GetEmailFromToken(resetPasswordDTO.Token);
            if (email == null)
            {
                return new ResetPasswordResult
                {
                    Forbidden = true,
                    ErrorMessage = HttpErrorMessages.INVALID_TOKEN
                };
            }
            await UpdatePasswordAsync(email, resetPasswordDTO.NewPassword);
            
            var name = await GetUserNameByEmailAsync(email);
            string subject = "Qilma Password Reset Successful";
            var builder = new BodyBuilder();
            builder.HtmlBody = File.ReadAllText("Templates/PasswordUpdated.html")
                .Replace("{{NAME}}", name)
                .Replace("{{YEAR}}", DateTime.Now.Year.ToString());

            await SendEmailAsync(email, name!, subject, builder);

            return new ResetPasswordResult
            {
                IsValid = true,
                SuccessMessage = HttpSuccessMessages.PASSWORD_RESET_SUCCESS
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new ResetPasswordResult
            {
                Failed = true,
                ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
            };
        }
    }
}

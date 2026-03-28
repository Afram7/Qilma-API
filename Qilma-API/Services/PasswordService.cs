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
                expires: DateTime.UtcNow.AddMinutes(15),
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );
            
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
    }

    // Sends an email with the reset password link to the user
    private async Task SendMessage(string email, string name, string link)
    {
        var message = new MimeMessage();
        message.From.Add (new MailboxAddress("Qilma", "qilma.game@gmail.com"));
        message.To.Add (new MailboxAddress (name, email));
        message.Subject = "Qilma Reset Password";

        var builder = new BodyBuilder();
        builder.HtmlBody = File.ReadAllText("Templates/ResetPassword.html")
            .Replace("{{NAME}}", name)
            .Replace("{{RESET_LINK}}", link)
            .Replace("{{YEAR}}", DateTime.Now.Year.ToString());
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync("qilma.game@gmail.com", "zcuj zpls jhdx xmkv");
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task<ResetPasswordRequestResult> ResetPasswordRequestAsync(string email)
    {
        try
        {
            var validation = EmailValidator.ValidateEmail(email);
            if (!validation.IsValid)
            {
                return new ResetPasswordRequestResult
                {
                    IsValid = false,
                    ErrorMessage = validation.ErrorMessage
                };
            }

            var emailExists = await CheckEmailExistsAsync(email);
            if (emailExists)
            {
                var token = GenerateToken(email);
                var name = await GetUserNameByEmailAsync(email);
                var frontendUrl = _config.GetSection("FrontendUrl");
                var resetLink = $"{frontendUrl["Url"]}/rp?token={token}";

                await SendMessage(email, name!, resetLink);

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
}

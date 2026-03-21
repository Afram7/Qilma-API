using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Models;

namespace Qilma_API.Services;

public class TokenService : ITokenService
{

    private const string INTERNAL_ERROR_MESSAGE = "Internal server error";
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public TokenService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // Fetches a user from the database by their email address
    private async Task<UserModel?> FetchUserByEmailAsync(string email)
    {
        var user = await _db.Users.SingleOrDefaultAsync(user => user.Email == email);
        return user;
    }

    // Checks if the provided password matches the hashed password stored in the database
    private async Task<bool> ValidatePasswordAsync(string password, string hashedPassword)
    {
        bool isCorrectPassword = await Task.Run(() => BC.EnhancedVerify(password, hashedPassword));
        return isCorrectPassword;
    }

    public async Task<TokenResponseResult> GenerateTokenAsync(string email, string password)
    {
        try
        {
            var user = await FetchUserByEmailAsync(email);
            if (user == null)
            {
                return new TokenResponseResult
                {
                    IsValid = false,
                    ErrorMessage = HttpErrorMessages.INVALID_EMAIL_OR_PASSWORD
                };
            }

            var isPasswordValid = await ValidatePasswordAsync(password, user.Password);
            if (!isPasswordValid)
            {
                return new TokenResponseResult
                {
                    IsValid = false,
                    ErrorMessage = HttpErrorMessages.INVALID_EMAIL_OR_PASSWORD
                };
            }

            var jwtSettings = _config.GetSection("Jwt");
            var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = new JwtSecurityToken(
                claims:
                [
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("Name", user.Name),
                    new Claim("Email", user.Email),
                    new Claim("Age", user.Age.ToString())
                ],
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            );
            
            var tokenString = tokenHandler.WriteToken(token);
            return new TokenResponseResult
                {
                    IsValid = true,
                    TokenResponse = new TokenResponseDTO
                    {
                        Token = tokenString
                    }
                };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new TokenResponseResult
                {
                    Failed = true,
                    ErrorMessage = HttpErrorMessages.INTERNAL_ERROR_MESSAGE
                };
        }
    }
}

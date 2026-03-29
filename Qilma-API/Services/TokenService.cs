using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Qilma_API.Constants;
using Qilma_API.Data;
using Qilma_API.DTOs;
using Qilma_API.Services.Interfaces;
using Qilma_API.Validators;

namespace Qilma_API.Services;

public class TokenService : ITokenService
{
    private const int TOKEN_EXPIRATION_TIME = 15;
    private readonly AppDbContext _db;
    private readonly TokenValidator _tokenValidator;
    private readonly PasswordValidator _passwordValidator;
    private readonly IConfiguration _config;
    private readonly ILogger<TokenService> _logger;

    public TokenService(AppDbContext db, IConfiguration config, ILogger<TokenService> logger, TokenValidator tokenValidator, PasswordValidator passwordValidator)
    {
        _db = db;
        _config = config;
        _logger = logger;
        _tokenValidator = tokenValidator;
        _passwordValidator = passwordValidator;
    }

    public async Task<bool> ValidatePasswordResetTokenAsync(string token)
    {
        var principal = _tokenValidator.GetPrincipalFromToken(token);
        if (principal == null)
        {
            return false;
        }

        return true;
    }

    public async Task<string?> GetEmailFromTokenAsync(string token)
    {
        var principal = _tokenValidator.GetPrincipalFromToken(token);
        if (principal == null)
        {
            return null;
        }

        var email = principal.Claims.FirstOrDefault(c => c.Type == "Email")?.Value;

        return email;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
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

    public async Task<TokenResponseResult> GenerarteSignInTokenAsync(string email, string password)
    {
        try
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return new TokenResponseResult
                {
                    IsValid = false,
                    ErrorMessage = ErrorMessages.INVALID_EMAIL_OR_PASSWORD
                };
            }

            var isValidPassword = await _passwordValidator.VerifyPasswordAsync(password, user.Password);
            if (!isValidPassword)
            {
                return new TokenResponseResult
                {
                    IsValid = false,
                    ErrorMessage = ErrorMessages.INVALID_EMAIL_OR_PASSWORD
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
            _logger.LogError(ex, "Error generating token for email: {Email}", email);
            return new TokenResponseResult
                {
                    Failed = true,
                    ErrorMessage = ErrorMessages.INTERNAL_ERROR_MESSAGE
                };
        }
    }
}

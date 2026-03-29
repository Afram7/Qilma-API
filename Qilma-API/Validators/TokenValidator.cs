using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Qilma_API.Validators;

public class TokenValidator
{
    private readonly IConfiguration _config;
    public TokenValidator(IConfiguration config)
    {
        _config = config;
    }

    // Validates the JWT token. If valid, it extracts and returns the claims principal. If invalid, it returns null.
    public ClaimsPrincipal? GetPrincipalFromToken(string token)
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
}
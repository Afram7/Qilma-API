namespace Qilma_API.DTOs;

// DTO for Token request, includes email and password for authentication
public class TokenRequestDTO
{
    required public string Email { get; set; }
    required public string Password { get; set; }
}

// DTO for Token response, includes the generated token string
public class TokenResponseDTO
{
    required public string Token { get; set; }
}

// DTO for returning the result of a token generation attempt, including success status and any error messages
public class TokenResponseResult
{
    public bool IsValid { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public TokenResponseDTO? TokenResponse { get; set; }
}
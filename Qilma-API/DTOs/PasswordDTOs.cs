namespace Qilma_API.DTOs;

// DTO for requesting a password reset, includes email field for identifying the user
public class ResetPasswordRequestDTO
{
    required public string Email { get; set; }
}

// DTO for returning the result of a password reset request
public class ResetPasswordRequestResult
{
    public bool IsValid { get; set; }
    public bool Failed { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

// DTO for confirming a password reset, includes new password, confirmation, and token for verification
public class ResetPasswordDTO
{
    required public string NewPassword { get; set; }
    required public string ConfirmPassword { get; set; }
    required public string Token { get; set; }
}

// DTO for returning the result of a password reset confirmation
public class ResetPasswordResult
{
    public bool IsValid { get; set; }
    public bool Forbidden { get; set; }
    public bool Failed { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
namespace Qilma_API.DTOs;

// DTO for requesting a password reset, includes email field for identifying the user
public class ResetPasswordRequestDto
{
    required public string Eamil { get; set; }
}

// DTO for returning the result of a password reset request
public class ResetPasswordRequestResult
{
    public bool IsValid { get; set; }
    public bool Failed { get; set; }
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}

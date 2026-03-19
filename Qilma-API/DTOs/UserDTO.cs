namespace Qilma_API.DTOs;

public class UserDTO
{
    required public int UserId { get; set; }
    required public string Name { get; set; }
    required public int Age { get; set; }
    required public string Email { get; set; }
}

public class CreateUserDTO
{
    required public string Name { get; set; }
    required public int Age { get; set; }
    required public string Email { get; set; }
    required public string Password { get; set; }
    required public string ConfirmPassword { get; set; }
}

public class CreateUserResult
{ 
    public bool IsValid { get; set; }
    public bool Conflict { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public UserDTO? User { get; set; }
}
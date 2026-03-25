namespace Qilma_API.DTOs;

// DTO for returning user information without sensitive data like password
public class UserDTO
{
    required public int UserId { get; set; }
    required public string Name { get; set; }
    required public int Age { get; set; }
    required public string Email { get; set; }
}

// DTO for creating a new user, includes password fields for validation
public class CreateUserDTO
{
    required public string Name { get; set; }
    required public int Age { get; set; }
    required public string Email { get; set; }
    required public string Password { get; set; }
    required public string ConfirmPassword { get; set; }
}

// DTO for updating user information, includes name, age, and email fields
public class UpdateUserDTO
{
    public string? Name { get; set; }
    public int? Age { get; set; }
    public string? Email { get; set; }
}

// DTO for returning the result of a user creation attempt, including success status and any error messages
public class CreateUserResult
{ 
    public bool IsValid { get; set; }
    public bool Conflict { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public UserDTO? User { get; set; }
}

// DTO for returning the result of a get user by ID attempt, including success status and any error messages
public class GetUserByIdResult
{
    public bool NotFound { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public UserDTO? User { get; set; }
}

// DTO for returning the result of an update user by ID attempt, including success status and any error messages
public class UpdateUserByIdResult
{
    public bool IsValid { get; set; }
    public bool NotFound { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
    public UserDTO? User { get; set; }
}

// DTO for returning the result of a delete user by ID attempt, including success status and any error messages
public class DeleteUserByIdResult
{
    public bool NotFound { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
}

// DTO for updating a user's password, includes current password for verification and new password fields for validation
public class UpdateUserPasswordDTO
{
    required public string CurrentPassword { get; set; }
    required public string NewPassword { get; set; }
    required public string ConfirmPassword { get; set; }
}

// DTO for returning the result of an update user password attempt, including success status and any error messages
public class UpdateUserPasswordResult
{
    public bool IsValid { get; set; }
    public bool NotFound { get; set; }
    public bool Failed { get; set; }
    public string? ErrorMessage { get; set; }
}
using Qilma_API.DTOs;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Qilma_API.Validators;

public class UserValidator
{
    private const int MIN_NAME_LENGTH = 2;
    private const int MAX_NAME_LENGTH = 30;
    private const int MIN_AGE_LENGTH = 6;
    private const int MAX_AGE_LENGTH = 120;

    private (bool IsValid, string? ErrorMessage) ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return (false, "Name is required");
        }
        if (name.Length < MIN_NAME_LENGTH)
        {
            return (false, "Name is too short");
        }
        if (name.Length > MAX_NAME_LENGTH)
        {
            return (false, "Name is too long");
        }
        if (!name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
        {
            return (false, "Letters and spaces only");
        }
        if (name.StartsWith(" ") || name.EndsWith(" "))
        {
            return (false, "No leading or trailing spaces");
        }
        if (name.Contains("  "))
        {
            return (false, "No double spaces");
        }
        return (true, null);
    }

    private (bool IsValid, string? ErrorMessage) ValidateAge(int age)
    {
        if (age < 0){
            return (false, "Age cannot be negative");
        }

        if (age < MIN_AGE_LENGTH){
            return (false, "Age must be 6+");
        }

        if (age > MAX_AGE_LENGTH){
            return (false, "Age is too high");
        }
        if (!int.TryParse(age.ToString(), out _)){
            return (false, "Age must be a number");
        }
        return (true, null);
    }

    // Validates the user input for creating a new user according to the specified rules
    public (bool IsValid, string? ErrorMessage) ValidateNewUser(CreateUserDTO newUser)
    {
        var nameResult = ValidateName(newUser.Name);
        if (!nameResult.IsValid)
        {
            return (false, nameResult.ErrorMessage);
        }

        var ageResult = ValidateAge(newUser.Age);
        if (!ageResult.IsValid)
        {
            return (false, ageResult.ErrorMessage);         
        }

        var emailResult = EmailValidator.ValidateEmail(newUser.Email);
        if (!emailResult.IsValid)
        {
            return (false, emailResult.ErrorMessage);
        }

        var passwordResult = PasswordValidator.ValidatePassword(newUser.Password, newUser.ConfirmPassword);
        if (!passwordResult.IsValid){
            return (false, passwordResult.ErrorMessage);
        }
        return (true, null);
    }

    // Validates the user input for updating a user according to the specified rules
    public (bool IsValid, string? ErrorMessage) ValidateUpdatedUser(UpdateUserDTO updatedUser)
    {
        if (updatedUser.Name != null)
        {
            var nameResult = ValidateName(updatedUser.Name);
            if (!nameResult.IsValid)
            {
                return (false, nameResult.ErrorMessage);
            }
        }

        if (updatedUser.Age.HasValue)
        {
            var ageResult = ValidateAge(updatedUser.Age.Value);
            if (!ageResult.IsValid)
            {
                return (false, ageResult.ErrorMessage);
            }
        }

        if (updatedUser.Email != null)
        {
            var emailResult = EmailValidator.ValidateEmail(updatedUser.Email);
            if (!emailResult.IsValid)
            {
                return (false, emailResult.ErrorMessage);
            }
        }
        return (true, null);
    }

    // Validates the user input for updating a user's password according to the specified rules
    public (bool IsValid, string? ErrorMessage) ValidateUpdatedPassword(string password, string confirmPassword)
    {
        var passwordResult = PasswordValidator.ValidatePassword(password, confirmPassword);
        if (!passwordResult.IsValid){
            return (false, passwordResult.ErrorMessage);
        }
        return (true, null);
    }
}
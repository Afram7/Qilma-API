using Qilma_API.DTOs;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Qilma_API.Validators;

public class UserValidator
{
    private const int MIN_NAME_LENGTH = 2;
    private const int MAX_NAME_LENGTH = 30;
    private const int MIN_EMAIL_LENGTH = 5;
    private const int MAX_EMAIL_LENGTH = 254;
    private const int MIN_AGE_LENGTH = 6;
    private const int MAX_AGE_LENGTH = 120;
    private const int MIN_PASSWORD_LENGTH = 8;

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

    private (bool IsValid, string? ErrorMessage) ValidateEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (string.IsNullOrWhiteSpace(trimmedEmail))
        {
            return (false, "Email is required");
        }
        if (trimmedEmail.EndsWith("."))
        {
            return (false, "No trailing dot");
        }
        if (trimmedEmail.Length > MAX_EMAIL_LENGTH)
        {
            return (false, "Email is too long");
        }
        if (trimmedEmail.Length < MIN_EMAIL_LENGTH)
        {
            return (false, "Email is too short");
        }

        try
        {
            var mailAddress = new MailAddress(trimmedEmail);
            return (true, null);
        }
        catch (FormatException ex)
        {
            Console.WriteLine(ex.Message);
            return (false, "Invalid email");
        }
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

    private (bool IsValid, string? ErrorMessage) ValidatePassword(string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "Password is required");
        }
        if (password.Length < MIN_PASSWORD_LENGTH)
        {
            return (false, "Password too short");
        }
        if (password != confirmPassword)
        {
            return (false, "Passwords do not match");
        }
        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            return (false, "Need uppercase");
        }
        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            return (false, "Need lowercase");
        }
        if (!Regex.IsMatch(password, @"[0-9]"))
        {
            return (false, "Need digit");
        }
        if (!Regex.IsMatch(password, @"[\W_]"))
        {
            return (false, "Need special character");
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

        var emailResult = ValidateEmail(newUser.Email);
        if (!emailResult.IsValid)
        {
            return (false, emailResult.ErrorMessage);
        }

        var ageResult = ValidateAge(newUser.Age);
        if (!ageResult.IsValid)
        {
            return (false, ageResult.ErrorMessage);         
        }

        var passwordResult = ValidatePassword(newUser.Password, newUser.ConfirmPassword);
        if (!passwordResult.IsValid){
            return (false, passwordResult.ErrorMessage);
        }
        
        return (true, null);
    }
}
using System.Net.Mail;

namespace Qilma_API.Validators;

public class EmailValidator
{
    private const int MIN_EMAIL_LENGTH = 5;
    private const int MAX_EMAIL_LENGTH = 254;

    public (bool IsValid, string? ErrorMessage) ValidateEmail(string email)
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
        catch
        {
            return (false, "Invalid email");
        }
    }
}

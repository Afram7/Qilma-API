using MimeKit;

namespace Qilma_API.Services.Interfaces;

public interface IEmailService
{
    // Sends an email asynchronously with the given subject and body to the specified email address
    Task SendEmailAsync(string email, string name, string subject, BodyBuilder builder);
}

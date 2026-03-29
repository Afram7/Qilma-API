using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Qilma_API.Services.Interfaces;
using Qilma_API.Settings;

namespace Qilma_API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string email, string name, string subject, BodyBuilder builder)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.Name, _settings.SenderAddress));
        message.To.Add (new MailboxAddress (name, email));
        message.Subject = subject;
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_settings.SenderAddress, _settings.AppPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

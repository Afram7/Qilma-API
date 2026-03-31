namespace Qilma_API.Settings;

public class EmailSettings
{
    public string EmailSenderName { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
    public string SmtpHost { get; set; } = string.Empty;
    public string SmtpPort { get; set; } = string.Empty;
}

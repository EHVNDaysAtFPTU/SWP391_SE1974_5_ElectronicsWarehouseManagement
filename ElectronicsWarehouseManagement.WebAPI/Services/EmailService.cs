using MailKit.Net.Smtp;
using MimeKit;

namespace ElectronicsWarehouseManagement.WebAPI.Services;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string email, string resetLink);
    Task SendEmailAsync(string to, string subject, string htmlBody);
}

internal class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IWebHostEnvironment _environment;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetLink)
    {
        var templatePath = Path.Combine(_environment.ContentRootPath, "EmailTemplates", "PasswordReset.html");
        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template not found at {templatePath}");
        var htmlBody = await File.ReadAllTextAsync(templatePath);
        htmlBody = htmlBody.Replace("{{ResetLink}}", resetLink);
        await SendEmailAsync(email, "Đặt lại mật khẩu của bạn", htmlBody);
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var senderPassword = _configuration["EmailSettings:SenderPassword"];
        var senderName = _configuration["EmailSettings:SenderName"];
        if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(senderEmail))
            throw new InvalidOperationException("Email settings are not configured properly.");
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress("", to));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();
        using var client = new SmtpClient();
        await client.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(senderEmail, senderPassword);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

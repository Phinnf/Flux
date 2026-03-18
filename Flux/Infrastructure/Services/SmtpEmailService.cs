using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Flux.Infrastructure.Services;

public class SmtpEmailService(ILogger<SmtpEmailService> logger, IConfiguration configuration) : IEmailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var host = configuration["Smtp:Host"];
            var portString = configuration["Smtp:Port"];
            var username = configuration["Smtp:Username"];
            var password = configuration["Smtp:Password"];
            var fromEmail = configuration["Smtp:FromEmail"] ?? username;
            var fromName = configuration["Smtp:FromName"] ?? "Flux App";

            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromEmail))
            {
                logger.LogWarning("SMTP is not fully configured. Falling back to simulation.");
                logger.LogInformation($"--- EMAIL SIMULATION ---\nTo: {toEmail}\nSubject: {subject}\nBody:\n{body}\n------------------------");
                return;
            }

            if (!int.TryParse(portString, out int port))
            {
                port = 587;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            
            // For Gmail on port 587, StartTls is recommended
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

            // Authenticate with the app password
            await client.AuthenticateAsync(username, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation($"Email successfully sent to {toEmail} via MailKit");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email via MailKit SMTP.");
        }
    }
}

using System.Net;
using System.Net.Mail;
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

            // If SMTP is not configured or email is invalid, fallback to logging
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fromEmail) || !fromEmail.Contains("@"))
            {
                logger.LogWarning("SMTP is not fully configured or FromEmail is invalid. Falling back to simulation.");
                logger.LogInformation($"--- EMAIL SIMULATION ---\nTo: {toEmail}\nSubject: {subject}\nBody:\n{body}\n------------------------");
                return;
            }

            if (!int.TryParse(portString, out int port))
            {
                port = 587; // default SMTP port
            }

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            
            mailMessage.To.Add(toEmail);
            
            await client.SendMailAsync(mailMessage);
            logger.LogInformation($"Email successfully sent to {toEmail}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email.");
        }
    }
}

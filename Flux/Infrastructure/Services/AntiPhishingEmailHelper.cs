namespace Flux.Infrastructure.Services;

public static class AntiPhishingEmailHelper
{
    public static string GenerateSecurityEmail(string username, string title, string message, string? code = null, string? ipAddress = null, string? userAgent = null)
    {
        var codeSection = string.IsNullOrEmpty(code) ? "" : $"""
            <div style="text-align: center; margin: 35px 0;">
                <span style="font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #2b6cb0; background: #ebf8ff; padding: 15px 30px; border-radius: 8px; border: 2px dashed #90cdf4;">{code}</span>
                <p style="font-size: 13px; color: #4a5568; margin-top: 15px;">This code expires in 15 minutes.</p>
            </div>
            """;

        var ipSection = string.IsNullOrEmpty(ipAddress) ? "" : $"<li><strong>Request IP:</strong> <code>{ipAddress}</code></li>";

        return $"""
            <div style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: auto; padding: 25px; border: 1px solid #e2e8f0; border-radius: 12px; color: #1a202c;">
                <div style="text-align: center; margin-bottom: 25px;">
                    <h1 style="color: #2d3748; font-size: 24px; margin: 0;">Flux Security</h1>
                    <p style="color: #718096; font-size: 14px; margin-top: 5px;">Unified Communication & Task Management</p>
                </div>

                <p style="font-size: 16px;">Hi <strong>{username}</strong>,</p>
                <p style="font-size: 16px; line-height: 1.6;">{message}</p>

                {codeSection}

                <div style="background-color: #fffaf0; border-left: 4px solid #ed8936; padding: 15px; margin-top: 30px; border-radius: 4px;">
                    <h3 style="margin-top: 0; color: #9c4221; font-size: 16px;">🛡️ Security Verification</h3>
                    <ul style="font-size: 13px; color: #744210; padding-left: 20px; margin-bottom: 0;">
                        <li><strong>Official Source:</strong> This email was sent from <code>noreply@flux.com</code>.</li>
                        <li><strong>No Sensitive Requests:</strong> Flux will <strong>never</strong> ask for your password via email.</li>
                        {ipSection}
                        <li><strong>Timestamp:</strong> {DateTime.UtcNow:f} UTC</li>
                    </ul>
                </div>

                <div style="margin-top: 30px; padding-top: 20px; border-top: 1px solid #edf2f7; text-align: center;">
                    <p style="font-size: 14px; color: #4a5568;">If you didn't request this, your account might be at risk.</p>
                    <a href="mailto:security@flux.com?subject=Suspicious Activity Report" style="display: inline-block; background-color: #e53e3e; color: white; padding: 10px 20px; text-decoration: none; border-radius: 6px; font-weight: 600; font-size: 14px;">Report Suspicious Activity</a>
                </div>

                <p style="font-size: 12px; color: #a0aec0; text-align: center; margin-top: 30px;">
                    &copy; {DateTime.UtcNow.Year} Flux. All rights reserved.<br>
                    You received this email because it's related to your security settings.
                </p>
            </div>
            """;
    }
}


using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Flux.Infrastructure.Security;

public class SecurityAuditMiddleware(RequestDelegate next, ILogger<SecurityAuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";
        
        // Audit sensitive endpoints
        if (path.Contains("/api/auth/login") || path.Contains("/api/users/register"))
        {
            var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            
            logger.LogInformation("SECURITY AUDIT: Accessing {Path} from IP: {IP}, User-Agent: {UserAgent}", path, ip, userAgent);
        }

        // --- ANTI-OPEN REDIRECT ---
        if (context.Request.Query.TryGetValue("returnUrl", out var returnUrl))
        {
            if (!IsLocalUrl(context, returnUrl!))
            {
                logger.LogWarning("SECURITY ALERT: Potential Open Redirect detected. Host: {Host}, ReturnUrl: {ReturnUrl}", context.Request.Host, returnUrl);
                // We don't block here, but the controller should validate it
            }
        }

        await next(context);
    }

    private static bool IsLocalUrl(HttpContext context, string url)
    {
        if (string.IsNullOrEmpty(url)) return true;

        // Check if it's a relative URL or matches current host
        if (url.StartsWith("/") && !url.StartsWith("//") && !url.StartsWith("/\\")) return true;
        
        // Add more robust local URL checks if needed
        return false;
    }
}

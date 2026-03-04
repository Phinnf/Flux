using Flux.Domain.Entities;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Flux.Features.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly FluxDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthController(FluxDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    [HttpGet("google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync("ExternalCookie");

        if (!result.Succeeded)
            return BadRequest("Google authentication failed.");

        var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
        var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var googleId = claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (email == null || googleId == null)
            return BadRequest("Required claims not received from Google.");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        if (user == null)
        {
            // Auto-register new user
            user = new User
            {
                Email = email,
                Username = name?.Replace(" ", "") ?? email.Split('@')[0],
                ExternalProvider = "Google",
                ExternalId = googleId,
                EmailConfirmed = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = _jwtService.GenerateToken(user);
        
        // Sign out of the temporary cookie
        await HttpContext.SignOutAsync("ExternalCookie");

        // Return a small script to save the token in localStorage and redirect to workspaces
        var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>Logging in...</title>
        </head>
        <body>
            <script>
                localStorage.setItem('authToken', '{token}');
                window.location.href = '/workspaces';
            </script>
        </body>
        </html>";

        return Content(html, "text/html");
    }
}

using Flux.Components;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using Flux.Infrastructure.Security;
using Flux.Infrastructure.Services;
using Flux.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// --- RATE LIMITING ---
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddFixedWindowLimiter("auth-limit", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
// --------------------

builder.Services.AddHttpClient("ExternalApi", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "FluxApp/1.0 (contact: support@flux.com)");
});
builder.Services.AddHttpClient();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddSignalR();

// --- IDENTITY & JWT SERVICES ---
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie("ExternalCookie", options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.SignInScheme = "ExternalCookie";
        var clientId = builder.Configuration["Authentication:Google:ClientId"];
        var clientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

        googleOptions.ClientId = string.IsNullOrEmpty(clientId) ? "placeholder-client-id" : clientId;
        googleOptions.ClientSecret = string.IsNullOrEmpty(clientSecret) ? "placeholder-client-secret" : clientSecret;
        googleOptions.SaveTokens = true;
    })
    .AddGitHub(githubOptions =>
    {
        githubOptions.SignInScheme = "ExternalCookie";
        var clientId = builder.Configuration["Authentication:GitHub:ClientId"];
        var clientSecret = builder.Configuration["Authentication:GitHub:ClientSecret"];

        githubOptions.ClientId = string.IsNullOrEmpty(clientId) ? "placeholder-client-id" : clientId;
        githubOptions.ClientSecret = string.IsNullOrEmpty(clientSecret) ? "placeholder-client-secret" : clientSecret;
        githubOptions.SaveTokens = true;
        // GitHub sometimes doesn't return email by default, request it
        githubOptions.Scope.Add("user:email");
    });
// ------------------------------

// Add Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthenticationStateProvider, FluxAuthStateProvider>();

// Register our Flux API client services
Action<IServiceProvider, HttpClient> configureHttpClient = (sp, client) =>
{
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;

    if (httpContext != null)
    {
        var request = httpContext.Request;
        client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
    }
    else
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7274";
        client.BaseAddress = new Uri(baseUrl);
    }
};

builder.Services.AddHttpClient<Flux.Infrastructure.Client.AuthClientService>(configureHttpClient);
builder.Services.AddHttpClient<Flux.Infrastructure.Client.WorkspaceClientService>(configureHttpClient);
builder.Services.AddHttpClient<Flux.Infrastructure.Client.MessageClientService>(configureHttpClient);
builder.Services.AddHttpClient<Flux.Infrastructure.Client.UserClientService>(configureHttpClient);
builder.Services.AddHttpClient<Flux.Infrastructure.Client.UploadClientService>(configureHttpClient);
builder.Services.AddHttpClient<Flux.Infrastructure.Client.WeatherClientService>(configureHttpClient);
builder.Services.AddHttpClient<Flux.Infrastructure.Client.WikipediaClientService>(configureHttpClient);

builder.Services.AddScoped<Flux.Infrastructure.Client.WorkspaceStateService>(); // State management service
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddHttpContextAccessor(); // Required to get the base address dynamically

// Swagger is a great tool for testing our APIs before connecting the Blazor frontend
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DATABASE CONFIGURATION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Register the factory as Singleton (default)
builder.Services.AddDbContextFactory<FluxDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register the scoped context for Controllers/API usage, creating it from the factory
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IDbContextFactory<FluxDbContext>>().CreateDbContext());
// ------------------------------

var app = builder.Build();

// --- SEED DATABASE ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<Flux.Infrastructure.Database.FluxDbContext>();
    await Flux.Infrastructure.Database.DbInitializer.SeedAsync(context);
}
// ---------------------

// Configure the HTTP request pipeline (Middlewares).
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseRateLimiter();
app.UseMiddleware<SecurityAuditMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

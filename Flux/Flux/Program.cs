using FluentValidation;
using FluentValidation.AspNetCore;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.Identity;
using Flux.Infrastructure.Services;
using Flux.Infrastructure.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddSignalR();
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<Program>();

// CORS Configuration for Next.js Frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // Next.js frontend
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Important for SignalR and Cookies
        });
});

// --- IDENTITY & JWT SERVICES ---
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddCookie("ExternalCookie")
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

        // Configuration for SignalR to use JWT via query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
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
        githubOptions.Scope.Add("user:email");
    });
// ------------------------------

builder.Services.AddHttpContextAccessor(); // Required to get the base address dynamically

// Swagger is a great tool for testing our APIs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DATABASE CONFIGURATION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FluxDbContext>(options =>
    options.UseNpgsql(connectionString));
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

// Enable CORS
app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
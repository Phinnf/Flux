using FluentValidation;
using FluentValidation.AspNetCore;
using Flux.Components;
using Flux.Infrastructure.Database;
using Flux.Infrastructure.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddSignalR();
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters()
    .AddValidatorsFromAssemblyContaining<Program>();

// Add Blazor and Fluent UI
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddFluentUIComponents();

// Register our Flux API client service
builder.Services.AddHttpClient<Flux.Infrastructure.Client.FluxClientService>((sp, client) =>
{
    // Point to the local server address
    var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    var httpContext = httpContextAccessor.HttpContext;
    
    if (httpContext != null)
    {
        var request = httpContext.Request;
        client.BaseAddress = new Uri($"{request.Scheme}://{request.Host}");
    }
    else
    {
        // Fallback for cases where HttpContext is not available (common in Blazor Server interactive mode)
        // You can also get this from appsettings.json
        var config = sp.GetRequiredService<IConfiguration>();
        var baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7274"; 
        client.BaseAddress = new Uri(baseUrl);
    }
});

builder.Services.AddScoped<Flux.Infrastructure.Client.WorkspaceStateService>(); // State management service
builder.Services.AddHttpContextAccessor(); // Required to get the base address dynamically

// Swagger is a great tool for testing our APIs before connecting the Blazor frontend
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
app.UseAntiforgery();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

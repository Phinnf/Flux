using Flux.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// We register Controllers here, though we will use them within our Vertical Slices (Features).
builder.Services.AddControllers();

// Swagger is a great tool for testing our APIs before connecting the Blazor frontend
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- DATABASE CONFIGURATION ---
// 1. Fetch the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Register the FluxDbContext with the Dependency Injection container
// We tell Entity Framework Core to use PostgreSQL via the UseNpgsql method
builder.Services.AddDbContext<FluxDbContext>(options =>
    options.UseNpgsql(connectionString));
// ------------------------------

var app = builder.Build();

// Configure the HTTP request pipeline (Middlewares).
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI only in development environment
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

// Map the API endpoints so they can respond to requests
app.MapControllers();

// Start the application
app.Run();

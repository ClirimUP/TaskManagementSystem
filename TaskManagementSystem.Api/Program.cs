using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TaskManagementSystem.Api;
using TaskManagementSystem.Api.Middleware;
using TaskManagementSystem.Features.Common;
using TaskManagementSystem.Infrastructure.Auth;
using TaskManagementSystem.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ----- Configuration validation -----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var corsOrigin = builder.Configuration.GetValue<string>("Cors:AllowedOrigin");
var jwtSecret = builder.Configuration.GetValue<string>("Jwt:Secret");

if (builder.Environment.IsDevelopment())
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        connectionString = "Data Source=taskmanagement.db";
        Log.Startup(builder, "ConnectionStrings:DefaultConnection is not configured. Using default: \"{Value}\"", connectionString);
    }

    if (string.IsNullOrWhiteSpace(corsOrigin))
    {
        corsOrigin = "http://localhost:5173";
        Log.Startup(builder, "Cors:AllowedOrigin is not configured. Using default: \"{Value}\"", corsOrigin);
    }

    if (string.IsNullOrWhiteSpace(jwtSecret))
    {
        jwtSecret = "DEV-ONLY-SECRET-KEY-REPLACE-IN-PRODUCTION-MIN-32-CHARS!!";
        Log.Startup(builder, "Jwt:Secret is not configured. Using development-only key: \"{Value}\"", jwtSecret);
    }
}
else
{
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured for non-Development environments.");

    if (string.IsNullOrWhiteSpace(corsOrigin))
        throw new InvalidOperationException("Cors:AllowedOrigin must be configured for non-Development environments.");

    if (string.IsNullOrWhiteSpace(jwtSecret))
        throw new InvalidOperationException("Jwt:Secret must be configured for non-Development environments.");
}

// ----- JSON -----
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ----- Database -----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

// ----- MediatR + Pipeline -----
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ValidationBehavior<,>).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// ----- FluentValidation -----
builder.Services.AddValidatorsFromAssembly(typeof(ValidationBehavior<,>).Assembly);

// ----- Swagger / OpenAPI -----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Task Management API",
        Version = "v1",
        Description = "A RESTful API for managing tasks — create, read, update, delete, and filter by status."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
});

// ----- CORS -----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ----- Authentication & Authorization -----
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
jwtSettings.Secret = jwtSecret;
builder.Services.Configure<JwtSettings>(opt =>
{
    opt.Secret = jwtSecret;
    opt.Issuer = jwtSettings.Issuer;
    opt.Audience = jwtSettings.Audience;
    opt.ExpirationInMinutes = jwtSettings.ExpirationInMinutes;
});

builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ----- Global Exception Handling -----
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// ----- Auto-apply migrations in Development -----
// Non-Development environments require manual migration: run 'dotnet ef database update'
// or set ASPNETCORE_ENVIRONMENT=Development to enable auto-migration.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// ----- Middleware Pipeline -----
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Task Management API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapTasksEndpoints();

app.Run();

// ----- Startup logging helper -----
internal static class Log
{
    internal static void Startup(WebApplicationBuilder builder, string message, string value)
    {
        using var loggerFactory = LoggerFactory.Create(b => b.AddConfiguration(builder.Configuration.GetSection("Logging")).AddConsole());
        var logger = loggerFactory.CreateLogger("Startup");
        logger.LogWarning(message, value);
    }
}

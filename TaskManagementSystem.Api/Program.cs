using System.Text.Json.Serialization;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TaskManagementSystem.Api;
using TaskManagementSystem.Api.Middleware;
using TaskManagementSystem.Features.Common;
using TaskManagementSystem.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ----- Configuration validation -----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var corsOrigin = builder.Configuration.GetValue<string>("Cors:AllowedOrigin");

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
}
else
{
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured for non-Development environments.");

    if (string.IsNullOrWhiteSpace(corsOrigin))
        throw new InvalidOperationException("Cors:AllowedOrigin must be configured for non-Development environments.");
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

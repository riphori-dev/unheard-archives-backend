using System.Threading.RateLimiting;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Tywynh.API.Middleware;
using Tywynh.API.Services;
using Tywynh.Application;
using Tywynh.Infrastructure;

using Microsoft.Extensions.Configuration.Json;

var builder = WebApplication.CreateBuilder(args);

foreach (var source in builder.Configuration.Sources.OfType<JsonConfigurationSource>())
{
    source.ReloadOnChange = false;
}

// JSON: snake_case to match frontend contract
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = new Tywynh.API.Json.SnakeCaseNamingPolicy());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS — allow frontend origin with credentials for visitor_token cookie
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:8080" };
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("submission", o =>
    {
        o.PermitLimit = 3;
        o.Window = TimeSpan.FromHours(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("resonance", o =>
    {
        o.PermitLimit = 60;
        o.Window = TimeSpan.FromHours(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.AddFixedWindowLimiter("echo", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromHours(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

// Application and infrastructure
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API services
builder.Services.AddScoped<VisitorTokenService>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Removed UseHttpsRedirection entirely for local dev
// Add it back in production behind an environment check if needed

app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
app.Run();
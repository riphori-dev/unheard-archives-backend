using System.Threading.RateLimiting;
using System.Text.Json;
using Microsoft.AspNetCore.RateLimiting;
using Tywynh.API.Middleware;
using Tywynh.API.Services;
using Tywynh.Application;
using Tywynh.Infrastructure;

using Microsoft.Extensions.Configuration.Json;

// Prevent the runtime from creating many inotify watchers in container environments
// (e.g. Render). This forces the configuration system to use a polling file
// watcher which avoids the inotify instance limit.
System.Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "1");

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 20000);

foreach (var source in builder.Configuration.Sources.OfType<JsonConfigurationSource>())
{
    source.ReloadOnChange = false;
}

// JSON: snake_case to match frontend contract
builder.Services.AddControllers()
    .AddJsonOptions(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = new Tywynh.API.Json.SnakeCaseNamingPolicy();
            o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase, false));
        });

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

// Limits are per client address, not a shared allowance for the whole archive.
builder.Services.AddRateLimiter(options =>
{
    foreach (var entry in new[] { ("submission", 3), ("resonance", 60), ("echo", 10), ("reflection", 10), ("report", 5), ("receipt", 20), ("owner", 600), ("owner-login", 10) })
        options.AddPolicy(entry.Item1, context => RateLimitPartition.GetFixedWindowLimiter(
            context.Items["ArchiveClient"] as string ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = entry.Item2, Window = TimeSpan.FromHours(1), QueueLimit = 0 }));
    options.OnRejected = async (context, ct) =>
    {
        var seconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry) ? (int)Math.Ceiling(retry.TotalSeconds) : 3600;
        context.HttpContext.Response.StatusCode = 429;
        context.HttpContext.Response.Headers["Retry-After"] = seconds.ToString();
        await context.HttpContext.Response.WriteAsJsonAsync(new { title = "Please wait before trying again.", status = 429 }, ct);
    };
});
builder.Services.AddScoped<ArchiveWorkflowService>();
// Trust only explicitly configured reverse proxies; never trust arbitrary forwarded addresses.
builder.Services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    foreach (var address in builder.Configuration.GetSection("Hosting:KnownProxies").Get<string[]>() ?? Array.Empty<string>())
        options.KnownProxies.Add(System.Net.IPAddress.Parse(address));
});

// Application and infrastructure
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// API services
builder.Services.AddScoped<VisitorTokenService>();
// Gemini reflection service and HttpClient
builder.Services.AddHttpClient<Tywynh.API.Services.IReflectionService, Tywynh.API.Services.ReflectionService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.UseForwardedHeaders();
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-store";
    var timer = System.Diagnostics.Stopwatch.StartNew();
    try { await next(); } finally { app.Logger.LogInformation("HTTP {Method} {Status} {DurationMs}ms {TraceId}", context.Request.Method, context.RequestAborted.IsCancellationRequested ? 499 : context.Response.StatusCode, timer.ElapsedMilliseconds, context.TraceIdentifier); }
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Removed UseHttpsRedirection entirely for local dev
// Add it back in production behind an environment check if needed

// The gateway supplies a pseudonymous rate-limit partition only after authentication.
app.Use(async (context, next) =>
{
    var key = app.Configuration["Gateway:SharedKey"];
    var supplied = context.Request.Headers["X-Gateway-Key"].ToString();
    if (!string.IsNullOrWhiteSpace(key) && key.Length >= 32 && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(key), System.Text.Encoding.UTF8.GetBytes(supplied)))
    {
        var client = context.Request.Headers["X-Archive-Client"].ToString();
        if (client.Length == 64 && client.All(Uri.IsHexDigit)) context.Items["ArchiveClient"] = client;
    }
    else if (!app.Environment.IsDevelopment() && context.Request.Path != "/" && !context.Request.Path.StartsWithSegments("/api/health"))
    {
        context.Response.StatusCode = 403;
        await context.Response.WriteAsJsonAsync(new { title = "Use the archive gateway.", status = 403 });
        return;
    }
    await next();
});
app.UseRateLimiter();
app.UseAuthorization();
app.MapControllers();
// Lightweight health endpoint so the platform can probe the root path.
app.MapGet("/", () => Results.Ok(new { status = "ok" }));
app.Run();
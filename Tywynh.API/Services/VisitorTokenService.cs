using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Tywynh.API.Services;

public class VisitorTokenService
{
    private const string CookieName = "visitor_token";

    public string GetOrCreateVisitorToken(HttpContext ctx)
    {
        if (ctx.Request.Cookies.TryGetValue(CookieName, out var existing)
            && existing.Length == 32 && Guid.TryParseExact(existing, "N", out _))
            return existing;

        var token = Guid.NewGuid().ToString("N");
        ctx.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = ctx.Request.IsHttps || !ctx.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            Path = "/"
        });
        return token;
    }

    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

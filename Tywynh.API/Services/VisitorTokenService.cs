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
            && !string.IsNullOrWhiteSpace(existing))
            return existing;

        var token = Guid.NewGuid().ToString("N");
        ctx.Response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
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

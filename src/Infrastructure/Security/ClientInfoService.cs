using Application.Abstractions.Security;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Security;

public sealed class ClientInfoService(
    IHttpContextAccessor httpContextAccessor
) : IClientInfoService
{
    public string GetIpAddress()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            return string.Empty;

        // Check for X-Forwarded-For header (for reverse proxy scenarios)
        var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP in the list (client IP)
            return forwardedFor.Split(',')[0].Trim();
        }

        // Check for X-Real-IP header
        var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Fall back to RemoteIpAddress
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }

    public string GetUserAgent()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
            return string.Empty;

        return httpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? string.Empty;
    }
}

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PasswordManager.SecureData.KeyStorage;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Extensions;

/// <summary>
/// Helper for cookie authentication
/// </summary>
public static class CookieExtensions
{
    public const string IpAddressClaim = "IpAddress";

    /// <summary>
    /// Sign in with cookie
    /// </summary>
    public static async Task SignInWithCookieAsync(this HttpContext context, ConnectionOptions connectionOptions)
    {
        var ip = GetUserIpAddress(context, connectionOptions.IsProxyUsed)
            ?? throw new InvalidOperationException("Cannot determine IP of user");
        var identity = new ClaimsIdentity([new(IpAddressClaim, ip)], CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    /// <summary>
    /// Sign out with cookie
    /// </summary>
    public static async Task SignOutWithCookieAsync(this HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// Validate principal
    /// </summary>
    public static bool ValidatePrincipal(this CookieValidatePrincipalContext context)
    {
        var keyStorage = context.HttpContext.RequestServices.GetRequiredService<IMasterKeyStorage>();
        if (!keyStorage.IsInitialized())
        {
            context.RejectPrincipal();
            return false;
        }

        var connectionOptions = context.HttpContext.RequestServices
            .GetRequiredService<IOptions<ConnectionOptions>>();
        var ip = GetUserIpAddress(context.HttpContext, connectionOptions.Value.IsProxyUsed)
            ?? throw new InvalidOperationException("Cannot determine IP of user");
        if (!ValidateIpAddress(context.Principal, ip))
        {
            context.RejectPrincipal();
            return false;
        }

        return true;
    }

    private static bool ValidateIpAddress(ClaimsPrincipal principal, string ip)
    {
        return principal.Claims.SingleOrDefault(m => m.ValueType == IpAddressClaim)?.Value == ip;
    }

    private static string GetUserIpAddress(HttpContext context, bool withProxy)
    {
        if (withProxy)
        {
            if (!context.Request.Headers.TryGetValue("X-Forwarded-For", out var realIp))
            {
                throw new InvalidOperationException(
                    "Cannot determine IP of user. Proxy must sent X-Forwarded-For header");
            }

            return realIp.FirstOrDefault();
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}

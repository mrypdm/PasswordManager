using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using PasswordManager.SecureData.Storages;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Helpers;

/// <inheritdoc />
public class CookieAuthorizationHelper(
    IKeyStorage keyStorage,
    IOptions<ConnectionOptions> connectionOptions) : ICookieAuthorizationHelper
{
    private const string IpAddressClaim = "IpAddress";

    /// <inheritdoc />
    public async Task SignInAsync(HttpContext context, ConnectionOptions connectionOptions)
    {
        var ip = GetUserIpAddress(context, connectionOptions.IsProxyUsed)
            ?? throw new InvalidOperationException("Cannot determine IP of user");
        var identity = new ClaimsIdentity([new(IpAddressClaim, ip)], CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    /// <inheritdoc />
    public async Task SignOutAsync(HttpContext context)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    /// <inheritdoc />
    public bool ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        if (!keyStorage.IsInitialized)
        {
            return false;
        }

        var ip = GetUserIpAddress(context.HttpContext, connectionOptions.Value.IsProxyUsed)
            ?? throw new InvalidOperationException("Cannot determine IP of user");
        if (!ValidateIpAddress(context.Principal, ip))
        {
            return false;
        }

        return true;
    }

    private static bool ValidateIpAddress(ClaimsPrincipal principal, string ip)
    {
        return principal.Claims.SingleOrDefault(m => m.Type == IpAddressClaim)?.Value == ip;
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

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Helpers;

/// <summary>
/// Helper for cookie authorization
/// </summary>
public interface ICookieAuthorizationHelper
{
    /// <summary>
    /// Sign in with cookie
    /// </summary>
    Task SignInAsync(HttpContext context, ConnectionOptions connectionOptions);

    /// <summary>
    /// Sign out with cookie
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task SignOutAsync(HttpContext context);

    /// <summary>
    /// Verify principal cookie
    /// </summary>
    bool ValidatePrincipal(CookieValidatePrincipalContext context);
}

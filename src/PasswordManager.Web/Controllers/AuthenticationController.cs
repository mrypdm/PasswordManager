using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.SecureData.Services;
using PasswordManager.Web.Helpers;
using PasswordManager.Web.Views.Authentication;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for login view
/// </summary>
[AllowAnonymous]
[Route("auth")]
public class AuthenticationController(
    IKeyService keyService,
    ICookieAuthorizationHelper cookieAuthorizationHelper) : Controller
{
    /// <summary>
    /// Get login view
    /// </summary>
    [HttpGet("login", Name = "Login")]
    public async Task<ActionResult> GetViewAsync([FromQuery] string returnUrl, CancellationToken token)
    {
        var isKeyDataExist = await keyService.IsKeyDataExistAsync(token);
        return View("Login", new LoginModel(returnUrl, isKeyDataExist));
    }

    /// <summary>
    /// Logout
    /// </summary>
    [HttpGet("logout", Name = "Logout")]
    public async Task<ActionResult> LogoutAsync(CancellationToken token)
    {
        await keyService.ClearKeyAsync(token);
        await cookieAuthorizationHelper.SignOutAsync(HttpContext);
        return Redirect("/auth/login");
    }
}

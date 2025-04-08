using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.SecureData.Services;
using PasswordManager.Web.Extensions;
using PasswordManager.Web.Views.Authentication;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for login view
/// </summary>
[AllowAnonymous]
[Route("auth")]
public class AuthenticationController(IMasterKeyService masterKeyService) : Controller
{
    /// <summary>
    /// Get login view
    /// </summary>
    [HttpGet("login", Name = "Login")]
    public async Task<ActionResult> GetViewAsync([FromQuery] string returnUrl, CancellationToken token)
    {
        var isKeyDataExist = await masterKeyService.IsMasterKeyDataExistsAsync(token);
        return View("Login", new LoginModel(returnUrl, isKeyDataExist));
    }

    /// <summary>
    /// Logout
    /// </summary>
    [HttpGet("logout", Name = "Logout")]
    public async Task<ActionResult> LogoutAsync(CancellationToken token)
    {
        await masterKeyService.ClearMasterKeyAsync(token);
        await HttpContext.SignOutWithCookieAsync();
        return Redirect("/auth/login");
    }
}

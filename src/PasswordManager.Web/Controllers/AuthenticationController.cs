using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Services;
using PasswordManager.Web.Views.Authentication;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for login view
/// </summary>
[AllowAnonymous]
[Route("auth")]
public class AuthenticationController(IKeyService keyService) : Controller
{
    /// <summary>
    /// Get login view
    /// </summary>
    [HttpGet("login")]
    public async Task<ActionResult> GetLoginViewAsync([FromQuery] string returnUrl, CancellationToken token)
    {
        var isKeyDataExist = await keyService.IsKeyDataExistAsync(token);
        return View("Login", new LoginModel(returnUrl, isKeyDataExist));
    }

    /// <summary>
    /// Get logout view
    /// </summary>
    [HttpGet("logout")]
    public ActionResult GetLogoutView()
    {
        return View("Logout");
    }
}

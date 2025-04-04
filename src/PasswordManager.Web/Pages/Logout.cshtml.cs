using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PasswordManager.SecureData.Services;
using PasswordManager.Web.Extensions;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Model for sign out user
/// </summary>
/// <param name="masterKeyService"></param>
[AllowAnonymous]
public class LogoutModel(IMasterKeyService masterKeyService) : PageModel
{
    /// <summary>
    /// Sign out user
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken token)
    {
        await masterKeyService.ClearMasterKeyAsync(token);
        await HttpContext.SignOutWithCookieAsync();
        return Redirect("/login");
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Model for sign out user
/// </summary>
[AllowAnonymous]
public class LogoutModel : PageModel
{
    /// <summary>
    /// Sign out user
    /// </summary>
    public void OnGet()
    {
    }
}

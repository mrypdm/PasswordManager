using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Page for Login and Logout
/// </summary>
[AllowAnonymous]
public class LoginModel : PageModel
{
    /// <summary>
    /// URL for redirect after login
    /// </summary>
    public string ReturnUrl { get; private set; }

    /// <summary>
    /// Get login page
    /// </summary>
    public void OnGet([FromQuery] string returnUrl)
    {
        ReturnUrl = returnUrl ?? "/";
    }
}

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PasswordManager.SecureData.Services;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Page for Login and Logout
/// </summary>
[AllowAnonymous]
public class LoginModel(IMasterKeyService masterKeyService) : PageModel
{
    /// <summary>
    /// URL for redirect after login
    /// </summary>
    public string ReturnUrl { get; private set; }

    /// <summary>
    /// If master key data exists
    /// </summary>
    public bool IsMasterKeyDataExists { get; private set; }

    /// <summary>
    /// Get login page
    /// </summary>
    public async Task OnGetAsync([FromQuery] string returnUrl, CancellationToken token)
    {
        ReturnUrl = returnUrl ?? "/";
        IsMasterKeyDataExists = await masterKeyService.IsMasterKeyDataExistsAsync(token);
    }
}

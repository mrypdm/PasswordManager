using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PasswordManager.SecureData.Services;
using PasswordManager.UserSettings;
using PasswordManager.Web.Extensions;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Page for Login and Logout
/// </summary>
[AllowAnonymous]
public class LoginModel(
    IMasterKeyService masterKeyService,
    IWritableOptions<UserOptions> userOptions,
    IOptions<ConnectionOptions> connectionOptions,
    IOptions<DevOptions> devOptions,
    ILogger<LoginModel> logger) : PageModel
{
    /// <summary>
    /// Message for alert
    /// </summary>
    public string AlertMessage { get; private set; }

    /// <summary>
    /// Get login page
    /// </summary>
    public async Task<IActionResult> OnGetAsync(CancellationToken token)
    {
        if (devOptions.Value.MasterPassword is not null)
        {
            return await OnPostAsync("/", devOptions.Value.MasterPassword, token);
        }

        return Page();
    }

    /// <summary>
    /// Tries to sign in user with <paramref name="masterPassword"/>
    /// </summary>
    public async Task<IActionResult> OnPostAsync(string returnUrl, string masterPassword,
        CancellationToken token)
    {
        try
        {
            await masterKeyService.InitMasterKeyAsync(masterPassword, userOptions.Value.SessionTimeout, token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while initializing master key");
            AlertMessage = $"Unhandled exception while initializing master key: {e.Message}";
            return Page();
        }

        try
        {
            await HttpContext.SignInWithCookieAsync(connectionOptions.Value);
        }
        catch (Exception e)
        {

            logger.LogError(e, "Error while authenticating user");
            AlertMessage = $"Unhandled exception while authenticating user: {e.Message}";
            return Page();
        }

        return Redirect(returnUrl ?? "/");
    }
}

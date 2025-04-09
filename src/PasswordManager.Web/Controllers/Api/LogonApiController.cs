using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Services;
using PasswordManager.UserSettings;
using PasswordManager.Web.Extensions;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Controllers.Api;

/// <summary>
/// Controller for logon
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/logon")]
public class LogonApiController(
    IMasterKeyService masterKeyService,
    IWritableOptions<UserOptions> userOptions,
    IOptions<ConnectionOptions> connectionOptions) : Controller
{
    /// <summary>
    /// Sign in user with cookie
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> SignInAsync([FromBody] LoginRequest request, CancellationToken token)
    {
        try
        {
            await masterKeyService.InitMasterKeyAsync(request.MasterPassword, userOptions.Value.SessionTimeout, token);
        }
        catch (InvalidMasterKeyException)
        {
            return Unauthorized("Master password is invalid");
        }
        catch (StorageBlockedException)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, "Storage is blocked");
        }

        await HttpContext.SignInWithCookieAsync(connectionOptions.Value);
        return Ok();
    }

    /// <summary>
    /// Sign out user with cookie
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> SignOutAsync(CancellationToken token)
    {
        await masterKeyService.ClearMasterKeyAsync(token);
        await HttpContext.SignOutWithCookieAsync();
        return Ok();
    }
}

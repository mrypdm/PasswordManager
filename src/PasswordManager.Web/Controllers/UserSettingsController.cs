using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Factories;
using PasswordManager.SecureData.Services;
using PasswordManager.UserSettings;
using PasswordManager.Web.Extensions;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for use settings
/// </summary>
[Route("api/settings")]
public class UserSettingsController(
    IWritableOptions<UserOptions> userOptions,
    IKeyGeneratorFactory keyGeneratorFactory,
    IMasterKeyService masterKeyService) : Controller
{
    /// <summary>
    /// Get current settings
    /// </summary>
    [HttpGet]
    public ActionResult<UserOptions> GetSettings()
    {
        return userOptions.Value;
    }

    /// <summary>
    /// Changes session timeout
    /// </summary>
    [HttpPatch]
    [Route("session-timeout")]
    public async Task<IActionResult> ChangeSessionTimeoutAsync(ChangeSessionTimeoutRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        await userOptions.UpdateAsync(opt => opt.SessionTimeout = request.Timeout, token);
        return Ok();
    }

    /// <summary>
    /// Changes master key parameters
    /// </summary>
    [HttpPatch]
    [Route("master-key")]
    public async Task<IActionResult> ChangeSessionTimeoutAsync(ChangeMasterKeySettingsRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var keyGenerator = keyGeneratorFactory.Create(
            request.SaltBytes ?? userOptions.Value.MasterKeySaltBytes,
            request.Iterations ?? userOptions.Value.MasterKeyIterations);
        await masterKeyService.ChangeMasterKeySettingsAsync(
            request.MasterPassword,
            request.NewMasterPassword ?? request.MasterPassword,
            keyGenerator,
            token);
        await userOptions.UpdateAsync(opt =>
        {
            opt.MasterKeySalt = request.Salt ?? opt.MasterKeySalt;
            opt.MasterKeyIterations = request.Iterations ?? opt.MasterKeyIterations;
        }, token);
        await HttpContext.SignOutWithCookieAsync();
        return Redirect("/login");
    }
}

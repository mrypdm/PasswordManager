using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Factories;
using PasswordManager.SecureData.Services;
using PasswordManager.UserSettings;
using PasswordManager.Web.Extensions;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Options;

namespace PasswordManager.Web.Controllers.Api;

/// <summary>
/// Controller for use settings
/// </summary>
[ApiController]
[Route("api/settings")]
[ValidateAntiForgeryToken]
public class UserSettingsApiController(
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
    /// Change session timeout
    /// </summary>
    [HttpPatch("session-timeout")]
    public async Task<ActionResult> ChangeSessionTimeoutAsync([FromBody] ChangeSessionTimeoutRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        await userOptions.UpdateAsync(opt => opt.SessionTimeout = request.Timeout, token);
        await masterKeyService.ChangeKeyTimeoutAsync(request.Timeout, token);
        return Ok();
    }

    /// <summary>
    /// Change master key parameters
    /// </summary>
    [HttpPatch("master-key")]
    public async Task<ActionResult> ChangeSessionTimeoutAsync([FromBody] ChangeMasterKeySettingsRequest request,
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
        return Ok();
    }

    /// <summary>
    /// Delete all data
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> DeleteStorageAsync(CancellationToken token)
    {
        await masterKeyService.ClearMasterKeyDataAsync(token);
        await HttpContext.SignOutWithCookieAsync();
        await userOptions.UpdateAsync(opt =>
        {
            opt.MasterKeySalt = new UserOptions().MasterKeySalt;
        }, token);
        return Ok();
    }
}

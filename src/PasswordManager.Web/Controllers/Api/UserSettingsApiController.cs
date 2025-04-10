using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Services;
using PasswordManager.UserSettings;
using PasswordManager.Web.Helpers;
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
    ICookieAuthorizationHelper cookieAuthorizationHelper,
    IKeyGeneratorFactory keyGeneratorFactory,
    IKeyService keyService) : Controller
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
        await keyService.ChangeKeyTimeoutAsync(request.Timeout, token);
        return Ok();
    }

    /// <summary>
    /// Change key parameters
    /// </summary>
    [HttpPatch("key")]
    public async Task<ActionResult> ChangeKeySettingsAsync([FromBody] ChangeKeySettingsRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        if (!request.HasSense())
        {
            return Ok();
        }

        var keyGenerator = keyGeneratorFactory.Create(
            request.SaltBytes ?? userOptions.Value.SaltBytes,
            request.Iterations ?? userOptions.Value.Iterations);
        await keyService.ChangeKeySettingsAsync(
            request.MasterPassword,
            request.NewMasterPassword ?? request.MasterPassword,
            keyGenerator,
            token);
        await userOptions.UpdateAsync(opt =>
        {
            opt.Salt = request.Salt ?? opt.Salt;
            opt.Iterations = request.Iterations ?? opt.Iterations;
        }, token);

        await cookieAuthorizationHelper.SignOutAsync(HttpContext);

        return Ok();
    }

    /// <summary>
    /// Delete all data
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> DeleteStorageAsync(CancellationToken token)
    {
        await keyService.ClearKeyDataAsync(token);
        await userOptions.UpdateAsync(opt =>
        {
            opt.Salt = new UserOptions().Salt;
        }, token);
        await cookieAuthorizationHelper.SignOutAsync(HttpContext);

        return Ok();
    }
}

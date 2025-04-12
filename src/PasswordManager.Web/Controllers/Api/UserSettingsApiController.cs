using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Factories;
using PasswordManager.Abstractions.Options;
using PasswordManager.Abstractions.Services;
using PasswordManager.Core.Options;
using PasswordManager.Web.Filters;
using PasswordManager.Web.Helpers;
using PasswordManager.Web.Models.Requests;

namespace PasswordManager.Web.Controllers.Api;

/// <summary>
/// Controller for use settings
/// </summary>
[Route("api/settings")]
[ValidateModelState]
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

        var oldKeyGenerator = keyGeneratorFactory.Create(userOptions.Value);
        var newKeyGenerator = keyGeneratorFactory.Create(new UserOptions
        {
            Salt = request.Salt ?? userOptions.Value.Salt,
            Iterations = request.Iterations ?? userOptions.Value.Iterations
        });

        var oldKey = oldKeyGenerator.Generate(request.MasterPassword);
        var newKey = newKeyGenerator.Generate(request.NewMasterPassword ?? request.MasterPassword);

        if (oldKey.SequenceEqual(newKey))
        {
            return Ok();
        }

        await keyService.ChangeKeySettingsAsync(oldKey, newKey, token);
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

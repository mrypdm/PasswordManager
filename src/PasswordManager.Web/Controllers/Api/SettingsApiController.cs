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
/// Controller for settings
/// </summary>
[Route("api/settings")]
[ValidateModelState]
[ValidateAntiForgeryToken]
public class SettingsApiController(
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

        await keyService.ChangeKeyTimeoutAsync(request.Timeout, token);
        userOptions.Update(opt => opt.SessionTimeout = request.Timeout);
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

        var newOptions = ToUserOptions(request);
        var oldKey = keyGeneratorFactory
            .Create(userOptions.Value)
            .Generate(request.MasterPassword);
        var newKey = keyGeneratorFactory
            .Create(newOptions)
            .Generate(request.NewMasterPassword ?? request.MasterPassword);

        // we got same key, so there is no sense to change it
        if (oldKey.SequenceEqual(newKey))
        {
            return Ok();
        }

        await keyService.ChangeKeySettingsAsync(oldKey, newKey, token);
        await cookieAuthorizationHelper.SignOutAsync(HttpContext);
        userOptions.Update(opt =>
        {
            opt.Salt = newOptions.Salt;
            opt.Iterations = newOptions.Iterations;
        });

        return Ok();
    }

    /// <summary>
    /// Delete all data
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> DeleteStorageAsync(CancellationToken token)
    {
        await keyService.ClearKeyDataAsync(token);
        await cookieAuthorizationHelper.SignOutAsync(HttpContext);
        userOptions.Update(opt =>
        {
            opt.Salt = new UserOptions().Salt;
        });

        return Ok();
    }

    private UserOptions ToUserOptions(ChangeKeySettingsRequest request)
    {
        return new UserOptions
        {
            Salt = request.Salt ?? userOptions.Value.Salt,
            Iterations = request.Iterations ?? userOptions.Value.Iterations
        };
    }
}

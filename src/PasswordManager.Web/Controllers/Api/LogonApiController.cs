using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Generators;
using PasswordManager.Abstractions.Services;
using PasswordManager.Options;
using PasswordManager.Web.Helpers;
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
    IKeyService keyService,
    IKeyGenerator keyGenerator,
    ICookieAuthorizationHelper cookieAuthorizationHelper,
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
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        try
        {
            var key = keyGenerator.Generate(request.MasterPassword);
            await keyService.InitKeyAsync(key, userOptions.Value.SessionTimeout, token);
        }
        catch (KeyValidationException)
        {
            return Unauthorized("Master password is invalid");
        }
        catch (StorageBlockedException)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, "Storage is blocked");
        }

        await cookieAuthorizationHelper.SignInAsync(HttpContext, connectionOptions.Value);
        return Ok();
    }

    /// <summary>
    /// Sign out user with cookie
    /// </summary>
    [HttpDelete]
    public async Task<ActionResult> SignOutAsync(CancellationToken token)
    {
        await keyService.ClearKeyAsync(token);
        await cookieAuthorizationHelper.SignOutAsync(HttpContext);
        return Ok();
    }
}

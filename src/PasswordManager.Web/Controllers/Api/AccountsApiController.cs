using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.Abstractions.Exceptions;
using PasswordManager.Abstractions.Models;
using PasswordManager.Abstractions.Services;
using PasswordManager.Web.Filters;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Models.Responses;

namespace PasswordManager.Web.Controllers.Api;

/// <summary>
/// Controller for account manipulation
/// </summary>
[Route("api/account")]
[ValidateModelState]
[ValidateAntiForgeryToken]
public class AccountsApiController(IAccountService accountService) : Controller
{
    /// <summary>
    /// Get account by id
    /// </summary>
    [HttpPost("{accountId}")]
    public async Task<ActionResult<Account>> GetAccountByIdAsync(int accountId, CancellationToken token)
    {
        try
        {
            return await accountService.GetAccountByIdAsync(accountId, token);
        }
        catch (ItemNotExistsException)
        {
            return NotFound($"Cannot find account with id={accountId}");
        }
    }

    /// <summary>
    /// Get all accounts headers
    /// </summary>
    [HttpGet("headers")]
    public async Task<ActionResult<AccountHeaderResponse[]>> GetAccountsWithoutDataAsync(CancellationToken token)
    {
        var names = await accountService.GetAccountsWithoutDataAsync(token);
        return names.Select(m => new AccountHeaderResponse { Id = m.Id, Name = m.Name }).ToArray();
    }

    /// <summary>
    /// Add new account
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AddAccountDataResponse>> AddAccountAsync([FromBody] UploadAccountRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var account = new Account
        {
            Name = request.Name,
            Data = new AccountData
            {
                Login = request.Login,
                Password = request.Password
            }
        };

        account = await accountService.AddAccountAsync(account, token);
        return new AddAccountDataResponse
        {
            Id = account.Id
        };
    }

    /// <summary>
    /// Updates account
    /// </summary>
    [HttpPut("{accountId}")]
    public async Task<ActionResult> UpdateAccountAsync(int accountId, [FromBody] UploadAccountRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var account = new Account
        {
            Id = accountId,
            Name = request.Name,
            Data = new AccountData
            {
                Login = request.Login,
                Password = request.Password
            }
        };

        try
        {
            await accountService.UpdateAccountAsync(account, token);
            return Ok();
        }
        catch (ItemNotExistsException)
        {
            return NotFound($"Cannot find account with id={accountId}");
        }
    }

    /// <summary>
    /// Delete account
    /// </summary>
    [HttpDelete("{accountId}")]
    public async Task<ActionResult> DeleteAccountAsync(int accountId, CancellationToken token)
    {
        await accountService.DeleteAccountAsync(accountId, token);
        return Ok();
    }
}

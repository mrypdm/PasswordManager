using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Repositories;
using PasswordManager.Web.Models.Requests;
using PasswordManager.Web.Models.Responses;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for account manipulation
/// </summary>
/// <param name="secureItemsRepository"></param>
[Route("api/account")]
[ValidateAntiForgeryToken]
public class AccountsController(ISecureItemsRepository secureItemsRepository) : Controller
{
    /// <summary>
    /// Get account data by id
    /// </summary>
    [HttpPost]
    [Route("{accountId}")]
    public async Task<ActionResult<AccountData>> GetAccountByIdAsync(int accountId, CancellationToken token)
    {
        try
        {
            return await secureItemsRepository.GetAccountByIdAsync(accountId, token);
        }
        catch (ItemNotExistsException)
        {
            return NotFound($"Cannot find account with id={accountId}");
        }
    }

    /// <summary>
    /// Get all items headers in storage
    /// </summary>
    [HttpGet]
    [Route("headers")]
    public async Task<ActionResult<ItemHeaderResponse[]>> GetAllHeadersAsync(CancellationToken token)
    {
        var items = await secureItemsRepository.GetItemsAsync(token);
        return items.Select(m => new ItemHeaderResponse(m.Id, m.Name)).ToArray();
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

        var account = new AccountData
        {
            Name = request.Name,
            Login = request.Login,
            Password = request.Password
        };
        var id = await secureItemsRepository.AddAccountAsync(account, token);
        return new AddAccountDataResponse
        {
            Id = id
        };
    }

    /// <summary>
    /// Updates current account
    /// </summary>
    [HttpPut]
    [Route("{accountId}")]
    public async Task<ActionResult> UpdateAccountAsync(int accountId, [FromBody] UploadAccountRequest request,
        CancellationToken token)
    {
        if (!request.Validate(out var error))
        {
            return BadRequest(error);
        }

        var account = new AccountData
        {
            Name = request.Name,
            Login = request.Login,
            Password = request.Password
        };

        try
        {
            await secureItemsRepository.UpdateAccountAsync(accountId, account, token);
            return Ok();
        }
        catch (ItemNotExistsException)
        {
            return NotFound($"Cannot find account with id={accountId}");
        }
    }
}

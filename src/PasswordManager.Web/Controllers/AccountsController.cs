using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PasswordManager.SecureData.Exceptions;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Repositories;
using PasswordManager.Web.Models;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for account manipulation
/// </summary>
/// <param name="secureItemsRepository"></param>
[Route("api/account")]
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
            return NotFound();
        }
    }

    /// <summary>
    /// Add new account
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AddAccountDataResponse>> AddAccountAsync([FromBody] AccountData request,
        CancellationToken token)
    {
        var id = await secureItemsRepository.AddAccountAsync(request, token);
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
    public async Task<ActionResult> UpdateAccountAsync(int accountId, [FromBody] AccountData request,
        CancellationToken token)
    {
        try
        {
            await secureItemsRepository.UpdateAccountAsync(accountId, request, token);
            return Ok();
        }
        catch (ItemNotExistsException)
        {
            return NotFound();
        }
    }
}

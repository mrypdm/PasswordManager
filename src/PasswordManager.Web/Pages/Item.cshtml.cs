using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PasswordManager.SecureData.Models;
using PasswordManager.SecureData.Repositories;
using PasswordManager.Web.Models;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Page for item
/// </summary>
[Authorize]
public class ItemModel(ISecureItemsRepository repository) : PageModel
{
    [BindProperty]
    public AcountDataRequest AccountDataModel { get; set; }

    public async Task<IActionResult> OnGetAsync([FromQuery] int? id, CancellationToken token)
    {
        AccountDataModel = new()
        {
            Id = id ?? -1
        };

        if (id is not null)
        {
            var accountData = await repository.GetAccountByIdAsync(id.Value, token);
            if (accountData is null)
            {
                return NotFound();
            }

            AccountDataModel.Name = accountData.Name;
            AccountDataModel.Login = accountData.Login;
            AccountDataModel.Password = accountData.Password;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken token)
    {
        var accountData = new AccountData
        {
            Name = AccountDataModel.Name,
            Login = AccountDataModel.Login,
            Password = AccountDataModel.Password
        };

        if (AccountDataModel.Id == -1)
        {
            var id = await repository.AddAccountAsync(accountData, token);
            return Redirect($"/item?id={id}");
        }
        else
        {
            await repository.UpdateAccountAsync(AccountDataModel.Id, accountData, token);
        }

        return Page();
    }
}

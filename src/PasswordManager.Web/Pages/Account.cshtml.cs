using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Page for account
/// </summary>
[Authorize]
public class AccountModel : PageModel
{
    /// <summary>
    /// Id of account
    /// </summary>
    public int AccountId { get; private set; }

    /// <summary>
    /// Get account page
    /// </summary>
    public ActionResult OnGet([FromQuery] int? id)
    {
        if (!ModelState.IsValid)
        {
            return Redirect("/account");
        }

        AccountId = id ?? -1;
        return Page();
    }
}

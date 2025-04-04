using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PasswordManager.Web.Pages;

/// <summary>
/// Page for item
/// </summary>
[Authorize]
public class ItemModel : PageModel
{
    /// <summary>
    /// Id of account
    /// </summary>
    public int AccountId { get; private set; }

    /// <summary>
    /// Get account page
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public ActionResult OnGet([FromQuery] int? id)
    {
        if (!ModelState.IsValid)
        {
            return Redirect("/item");
        }

        AccountId = id ?? -1;
        return Page();
    }
}

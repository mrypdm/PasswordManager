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
    public int AccountId { get; private set; }

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

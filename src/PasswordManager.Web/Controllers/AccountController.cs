using Microsoft.AspNetCore.Mvc;
using PasswordManager.Web.Views.Account;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for account view
/// </summary>
[Route("account")]
public class AccountController : Controller
{
    /// <summary>
    /// Get account view
    /// </summary>
    [HttpGet]
    public ActionResult GetView([FromQuery] int id = -1)
    {
        return View("Account", new AccountModel(id));
    }
}

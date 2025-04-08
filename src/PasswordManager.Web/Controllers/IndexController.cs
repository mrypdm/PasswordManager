using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for index view
/// </summary>
[Route("")]
public class IndexController : Controller
{
    /// <summary>
    /// Get index view
    /// </summary>
    [HttpGet]
    public ActionResult GetView()
    {
        return View("Index");
    }
}

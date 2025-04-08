using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for settings view
/// </summary>
[Route("settings")]
public class SettingsController : Controller
{
    /// <summary>
    /// Get settings view
    /// </summary>
    [HttpGet]
    public ActionResult GetView()
    {
        return View("Settings");
    }
}

using Microsoft.AspNetCore.Mvc;

namespace PasswordManager.Web.Controllers;

/// <summary>
/// Controller for generator view
/// </summary>
[Route("generator")]
public class GeneratorController : Controller
{
    /// <summary>
    /// Get generator view
    /// </summary>
    [HttpGet]
    public ActionResult GetView()
    {
        return View("Generator");
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace AcademicAIAssistant.Controllers;

[Authorize]
public class WritingStudioController : Controller
{
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Writing");
    }
}

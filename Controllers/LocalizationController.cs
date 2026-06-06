using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AcademicAIAssistant.Controllers;

public class LocalizationController : Controller
{
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        if (culture != "en" && culture != "vi")
        {
            culture = "en";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true
            });

        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            returnUrl = Url.Action("Index", "Home") ?? "/";
        }

        return LocalRedirect(returnUrl);
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyAPI.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        // Hvis bruger er logget ind ? Dashboard
        if (User.Identity?.IsAuthenticated ?? false)
        {
            return RedirectToPage("/Dashboard");
        }

        // Hvis ikke logget ind ? Login
        return RedirectToPage("/Account/Login");
    }
}
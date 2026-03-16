using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VTTGROUP.Blazor.Components.Pages
{
    public class loginredirectModel : PageModel
    {
        public IActionResult OnGet()
        {
            return Redirect("/dashboard");
        }
    }
}

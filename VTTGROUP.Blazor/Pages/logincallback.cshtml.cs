using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VTTGROUP.Blazor.Components.Pages
{
    public class logincallbackModel : PageModel
    {
        public async Task OnGetAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                Response.Redirect("/login");
                return;
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwtToken.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // ✅ Redirect về lại Blazor app
            Response.Redirect("/loginredirect");
        }
    }
}

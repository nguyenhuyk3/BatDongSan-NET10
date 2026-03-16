using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VTTGROUP.Application.Menu;

namespace VTTGROUP.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // [HttpGet("{username}")]
        [HttpGet("tree")]
        //public async Task<IActionResult> GetMenu(string username)
        public async Task<IActionResult> GetMenu()
        {
            var username = User.Identity?.Name ?? string.Empty;            
            var menus = await _menuService.GetMenuByUserAsync(username);
            return Ok(menus);
        }
    }
}

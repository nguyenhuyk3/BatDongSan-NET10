using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VTTGROUP.Application.Auth;
using VTTGROUP.Application.Auth.Dto;

namespace VTTGROUP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("loginhome")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginHome(LoginRequest request)
        {
            try
            {
                var token = await _authService.LoginAsync(request);
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Ghi log chi tiết yêu cầu đăng nhập thất bại
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                _logger.LogWarning(ex, "Yêu cầu đăng nhập thất bại cho user {Username} từ IP {Ip}", request.Username, ip);

                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            try
            {
                var token = await _authService.RegisterAsync(request);
                return Ok(token);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Đăng ký tài khoản thất bại cho user {Username}", request.Username);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            try
            {
                var token = await _authService.RefreshTokenAsync(refreshToken);
                return Ok(token);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Yêu cầu refresh token không hợp lệ");
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke([FromBody] string refreshToken)
        {
            var result = await _authService.RevokeTokenAsync(refreshToken);
            return Ok(new { revoked = result });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            var result = await _authService.LogoutAsync(refreshToken);
            return Ok(new { revoked = result });
        }

        [HttpPost("set-cookie")]
        public IActionResult SetCookie([FromBody] TokenResponse token)
        {
            Response.Cookies.Append("accessToken", token.AccessToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = false, // Localhost: false; production: true
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(15)
            });

            Response.Cookies.Append("refreshToken", token.RefreshToken, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok();
        }
    }
}

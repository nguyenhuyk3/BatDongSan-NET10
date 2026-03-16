using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Blazor.Services.Auth
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly IJSRuntime _js;
        private readonly AppDbContext _db;

        public AuthorizationService(IJSRuntime js, AppDbContext db)
        {
            _js = js;
            _db = db;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            // Đọc access token từ cookie trình duyệt
            var token = await _js.InvokeAsync<string>("blazorGetCookie", "accessToken");
            if (string.IsNullOrWhiteSpace(token))
                return false;

            JwtSecurityToken? jwt;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                jwt = handler.ReadJwtToken(token);
            }
            catch
            {
                // Token không đúng định dạng => coi như chưa đăng nhập
                return false;
            }

            // Kiểm tra hết hạn (exp) trên JWT
            var expClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp || c.Type == "exp")?.Value;
            if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expUnix))
            {
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                if (expUtc <= DateTime.UtcNow)
                    return false;
            }

            // Lấy username & jti (session id) từ token
            var username = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name")?.Value;
            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti || c.Type == "jti")?.Value
                      ?? jwt.Id;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(jti))
                return false;

            // Đối chiếu với bảng refresh token để đảm bảo phiên còn hiệu lực và chưa bị thu hồi
            var now = DateTime.UtcNow;
            var active = await _db.TblRefreshtokens
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.TenDangNhap == username
                                           && r.JwtId == jti
                                           && r.IsRevoked == false
                                           && r.NgayHetHan > now);

            return active != null;
        }
    }
}

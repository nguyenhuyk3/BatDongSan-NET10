using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VTTGROUP.Application.Auth;
using VTTGROUP.Application.Auth.Dto;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IMemoryCache _cache;
        //private readonly INotificationSender _notifier;

        private readonly int _maxFailedAttempts;
        private readonly TimeSpan _failedWindow;
        private readonly TimeSpan _blockDuration;

        public AuthService(AppDbContext db, IConfiguration config, IJwtService jwtService, ILogger<AuthService> logger, IMemoryCache cache)
        {
            _db = db;
            _config = config;
            _jwtService = jwtService;
            _logger = logger;
            _cache = cache;

            // Đọc cấu hình LoginSecurity từ appsettings
            _maxFailedAttempts = int.TryParse(_config["LoginSecurity:MaxFailedAttempts"], out var max) ? max : 5;
            var failedMinutes = int.TryParse(_config["LoginSecurity:FailedWindowMinutes"], out var fw) ? fw : 5;
            var blockMinutes = int.TryParse(_config["LoginSecurity:BlockDurationMinutes"], out var bd) ? bd : 5;

            _failedWindow = TimeSpan.FromMinutes(failedMinutes);
            _blockDuration = TimeSpan.FromMinutes(blockMinutes);
        }

        private string GetFailedKey(string username)
            => $"login-fail:{username.ToLowerInvariant()}";

        private string GetBlockKey(string username)
            => $"login-block:{username.ToLowerInvariant()}";


        private bool IsUserTemporarilyBlocked(string username)
        {
            var key = GetBlockKey(username);
            return _cache.TryGetValue(key, out _);
        }

        private void RegisterFailedAttempt(string username)
        {
            var failKey = GetFailedKey(username);
            var blockKey = GetBlockKey(username);

            var count = _cache.TryGetValue<int>(failKey, out var current) ? current + 1 : 1;

            _cache.Set(failKey, count, _failedWindow);

            if (count >= _maxFailedAttempts)
            {
                // đặt cờ block tạm thời
                _cache.Set(blockKey, true, _blockDuration);
                _logger.LogWarning("User {Username} bị khóa tạm thời {Duration} phút do đăng nhập sai quá {Max} lần", username, _blockDuration.TotalMinutes, _maxFailedAttempts);
            }
        }

        private void ResetFailedAttempts(string username)
        {
            _cache.Remove(GetFailedKey(username));
            _cache.Remove(GetBlockKey(username));
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request)
        {
            var username = request.Username?.Trim() ?? string.Empty;

            // Kiểm tra bị khóa tạm thời do brute-force
            if (IsUserTemporarilyBlocked(username))
            {
                _logger.LogWarning("Đăng nhập bị chặn tạm thời cho user {Username} do vượt quá số lần thử cho phép", username);
                // Không tiết lộ lý do cụ thể cho client
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu");
            }

            // Chuẩn hóa: log và trả lỗi thống nhất khi đăng nhập thất bại
            var user = await _db.TblUsers
                .Include(u => u.TblUserthuocnhoms)
                    .ThenInclude(r => r.MaNhomUserNavigation)
                .FirstOrDefaultAsync(u => u.TenDangNhap == username);

            if (user == null)
            {
                _logger.LogWarning("Đăng nhập thất bại: không tìm thấy user {Username}", username);
                RegisterFailedAttempt(username);
                await Task.Delay(300); // delay nhỏ chống brute-force
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu");
            }

            if (!user.TrangThai)
            {
                _logger.LogWarning("Đăng nhập bị từ chối: tài khoản {Username} đang bị khóa hoặc không hoạt động", username);
                throw new UnauthorizedAccessException("Tài khoản đang bị khóa hoặc không hoạt động");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.MatKhau))
            {
                _logger.LogWarning("Đăng nhập thất bại: sai mật khẩu cho user {Username}", username);
                RegisterFailedAttempt(username);
                await Task.Delay(300); // delay nhỏ chống brute-force
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu");
            }

            // Đăng nhập thành công -> reset bộ đếm thất bại
            ResetFailedAttempts(username);

            try
            {
                // Tạo Access Token + Refresh Token
                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();
                var jwtId = _jwtService.GetJti(accessToken);
                var expired = DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpiryDays);

                await using var tx = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                // Lưu vào bảng TBL_REFRESHTOKEN
                var refresh = new TblRefreshtoken
                {
                    MaNhanVien = user.MaNhanVien,
                    TenDangNhap = user.TenDangNhap,
                    ToKen = refreshToken,
                    NgayHetHan = expired,
                    IsRevoked = false,
                    JwtId = jwtId,
                    UserId = user.Id
                };

                _db.TblRefreshtokens.Add(refresh);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();

                return new TokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Username = user.TenDangNhap ?? string.Empty,
                    Role = user.TblUserthuocnhoms?.FirstOrDefault()?.MaNhomUserNavigation?.TenNhomUser ?? "User"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý đăng nhập cho user {Username}", username);
                throw;
            }
        }

        public async Task<TokenResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _db.TblUsers.AnyAsync(u => u.TenDangNhap == request.Username))
                throw new InvalidOperationException("Tên đăng nhập đã tồn tại");

            var user = new TblUser
            {
                TenDangNhap = request.Username,
                MatKhau = BCrypt.Net.BCrypt.HashPassword(request.Password),
                MaNhanVien = request.MaNhanVien,
                TrangThai = true,
                NgayLap = DateTime.Now
            };

            _db.TblUsers.Add(user);
            await _db.SaveChangesAsync();

            return await GenerateTokensAsync(user);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var storedToken = await _db.TblRefreshtokens
                    .FirstOrDefaultAsync(x => x.ToKen == refreshToken && x.IsRevoked != true);

                if (storedToken == null || storedToken.NgayHetHan < DateTime.UtcNow)
                {
                    _logger.LogWarning("Yêu cầu refresh token không hợp lệ hoặc hết hạn");
                    throw new UnauthorizedAccessException("Refresh token không hợp lệ hoặc đã hết hạn");
                }

                var user = await _db.TblUsers
                    .FirstOrDefaultAsync(x => x.TenDangNhap == storedToken.TenDangNhap);

                if (user == null)
                {
                    _logger.LogWarning("Yêu cầu refresh token cho user không tồn tại: {Username}", storedToken.TenDangNhap);
                    throw new UnauthorizedAccessException("Người dùng không tồn tại");
                }

                // Thu hồi token cũ
                storedToken.IsRevoked = true;

                // Tạo token mới
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var newJti = _jwtService.GetJti(newAccessToken);

                // Lưu refresh token mới
                var newTokenEntry = new TblRefreshtoken
                {
                    MaNhanVien = user.MaNhanVien,
                    TenDangNhap = user.TenDangNhap,
                    ToKen = newRefreshToken,
                    NgayHetHan = DateTime.UtcNow.AddDays(_jwtService.RefreshTokenExpiryDays),
                    JwtId = newJti,
                    IsRevoked = false,
                    UserId = user.Id
                };

                _db.TblRefreshtokens.Add(newTokenEntry);
                await _db.SaveChangesAsync();

                return new TokenResponse
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý refresh token");
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string refreshToken)
        {
            try
            {
                var token = await _db.TblRefreshtokens.FirstOrDefaultAsync(x => x.ToKen == refreshToken);
                if (token == null)
                    return false;

                token.IsRevoked = true;
                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi revoke refresh token");
                throw;
            }
        }

        private async Task<TokenResponse> GenerateTokensAsync(TblUser user)
        {
            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];
            var jwtAudience = _config["Jwt:Audience"];
            var expiresDays = int.Parse(_config["Jwt:AccessTokenExpirationDays"] ?? "1");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(jwtKey!);
            var jwtId = Guid.NewGuid().ToString();
            var role = user.TblUserthuocnhoms.FirstOrDefault()?.MaNhomUserNavigation?.TenNhomUser ?? "User";
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.TenDangNhap),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(expiresDays),
                Issuer = jwtIssuer,
                Audience = jwtAudience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(token);

            var refreshToken = new TblRefreshtoken
            {
                ToKen = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                MaNhanVien = user.MaNhanVien,
                TenDangNhap = user.TenDangNhap,
                JwtId = jwtId,
                NgayHetHan = DateTime.UtcNow.AddDays(int.Parse(_config["Jwt:RefreshTokenExpirationDays"] ?? "7")),
                IsRevoked = false,
                UserId = user.Id
            };

            _db.TblRefreshtokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return new TokenResponse
            {
                AccessToken = accessToken,
                ExpiresAt = tokenDescriptor.Expires!.Value
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            return await RevokeTokenAsync(refreshToken);
        }
    }
}

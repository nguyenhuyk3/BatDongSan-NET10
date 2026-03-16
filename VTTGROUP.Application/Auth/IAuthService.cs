using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Application.Auth.Dto;

namespace VTTGROUP.Application.Auth
{
    public interface IAuthService
    {
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RegisterAsync(RegisterRequest request);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(string refreshToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public interface IJwtService
    {
        string GenerateAccessToken(TblUser user);
        string GenerateRefreshToken();
        string GetJti(string token);
        int RefreshTokenExpiryDays { get; }
    }
}

using Microsoft.EntityFrameworkCore;
using VTTGROUP.Domain.Model.Hubs;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public interface ISessionKiller
    {
        Task ForceLogoutByUsernameAsync(string userName);
        Task ForceLogoutBySidAsync(string jwtId);
        Task ForceLogoutOthersAsync(string userName, string currentSid);
    }
    public class SessionKiller : ISessionKiller
    {
        private readonly AppDbContext _db;
        private readonly INotificationSender _hub;

        public SessionKiller(AppDbContext db, INotificationSender hub)
        {
            _db = db; _hub = hub;
        }

        // Đá tất cả phiên hiện hành của 1 user (nếu cần)
        public async Task ForceLogoutByUsernameAsync(string userName)
        {
            var olds = await _db.TblRefreshtokens
                .Where(r => r.TenDangNhap == userName && r.IsRevoked == false && r.JwtId != null)
                .Select(r => r.JwtId!)
                .Distinct()
                .ToListAsync();

            foreach (var sid in olds)
                await _hub.ForceLogout(sid);
        }

        // Đá đúng 1 phiên theo JwtId (sid)
        public Task ForceLogoutBySidAsync(string jwtId) =>
            _hub.ForceLogout(jwtId);

        public async Task ForceLogoutOthersAsync(string userName, string currentSid)
        {
            var sids = await _db.TblRefreshtokens
                .Where(r => r.TenDangNhap == userName && r.IsRevoked == false
                            && r.JwtId != null && r.JwtId != currentSid)
                .Distinct()
                .ToListAsync();

            foreach (var r in sids) r.IsRevoked = true;
            await _db.SaveChangesAsync();

            foreach (var sid in sids)
                await _hub.ForceLogout(sid.JwtId);
        }
    }
}

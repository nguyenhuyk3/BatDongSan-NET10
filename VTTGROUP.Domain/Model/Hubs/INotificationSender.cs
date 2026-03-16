using VTTGROUP.Domain.Model.CanHo;

namespace VTTGROUP.Domain.Model.Hubs
{
    public interface INotificationSender
    {
        Task SendAsync(string userId, string message);
        Task UpdateTTCHAsync();
        Task ForceLogout(string jwtId);
        Task DangKyCountdownsAsync(IEnumerable<DangKyCountdownDto> items, DateTime serverUtcNow);
    }
}

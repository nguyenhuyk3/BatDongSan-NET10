using Microsoft.AspNetCore.SignalR;
using VTTGROUP.Domain.Model.CanHo;
using VTTGROUP.Domain.Model.Hubs;
namespace VTTGROUP.Blazor.Services.Hubs
{
    public class NotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendAsync(string user, string message)
        {
            await _hubContext.Clients.User(user).SendAsync("ReceiveMessage", user, message);
        }
        public async Task UpdateTTCHAsync()
        {
            await _hubContext.Clients.All.SendAsync("TinhTrangCanHoUpdated");
        }
        public async Task ForceLogout(string JwtId)
        {
            await _hubContext.Clients.Group($"sid:{JwtId}").SendAsync("ForceLogout");
        }
        public Task DangKyCountdownsAsync(IEnumerable<DangKyCountdownDto> items, DateTime serverUtcNow)
        => _hubContext.Clients.All.SendAsync("DangKyCountdowns", new
        {
            serverUtcNow,
            items
        });
    }
}

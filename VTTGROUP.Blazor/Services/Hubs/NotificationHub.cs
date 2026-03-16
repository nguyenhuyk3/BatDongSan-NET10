using Microsoft.AspNetCore.SignalR;
namespace VTTGROUP.Blazor.Services.Hubs
{
    public class NotificationHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst("MaNhanVien")?.Value;

            var sid = Context.User?.FindFirst("sid")?.Value;
            if (!string.IsNullOrEmpty(sid))
                Groups.AddToGroupAsync(Context.ConnectionId, $"sid:{sid}");

            return base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            var sid = Context.User?.FindFirst("sid")?.Value;
            if (!string.IsNullOrEmpty(sid))
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"sid:{sid}");

            await base.OnDisconnectedAsync(ex);
        }
        public async Task SendMessage(string user, string message)
        {
            await Clients.User(user).SendAsync("ReceiveMessage", user, message);
        }
        public async Task TinhTrangCanHoUpdate()
        {
            await Clients.All.SendAsync("TinhTrangCanHoUpdated");
        }
        public async Task ForceLogout()
        {
            await Clients.All.SendAsync("ForceLogout");
        }
    }
}

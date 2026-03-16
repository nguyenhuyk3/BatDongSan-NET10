using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace VTTGROUP.Infrastructure.Services
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            var claims = connection.User?.Claims.Select(c => $"{c.Type}: {c.Value}");
            Console.WriteLine("🟢 SignalR Claims: " + string.Join(", ", claims));
            // Lấy userId từ claim "MaNhanVien" trong token
            return connection.User?.FindFirst("MaNhanVien")?.Value;
        }
    }
}

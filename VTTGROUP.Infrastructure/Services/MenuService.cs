using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Application.Menu;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class MenuService : IMenuService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenuService> _logger;
        public MenuService(AppDbContext context, ILogger<MenuService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SystemMenu>> GetMenuByUserAsync(string username)
        {
            var menus = new List<SystemMenu>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_CongViecCuaUser";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@Username", username));
                command.Parameters.Add(new SqlParameter("@UserGroupId", DBNull.Value));


                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    menus.Add(new SystemMenu
                    {
                        MaCongViec = reader["MaCongViec"]?.ToString(),
                        TenCongViec = reader["TenCongViec"]?.ToString(),
                        TenController = reader["TenController"]?.ToString(),
                        TenAction = reader["TenAction"]?.ToString(),
                        GhiChu = reader["GhiChu"]?.ToString(),
                        DoUuTien = string.IsNullOrEmpty(reader["DoUuTien"]?.ToString()) ? 0 : Convert.ToInt32(reader["DoUuTien"]),
                        MaCha = string.IsNullOrEmpty(reader["MaCha"].ToString()) ? string.Empty : reader["MaCha"].ToString(),
                        MaVuViec = reader["MaVuViec"]?.ToString(),
                        SoLuongCongViecCon = Convert.ToInt32(reader["soLuongCongViecCon"])
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy menu cho user: {Username}", username);
                // Có thể throw lại nếu bạn muốn xử lý ở tầng controller
                // throw;
            }
            var listBuildTree = BuildTree(null, menus);
            return listBuildTree;
        }

        private List<SystemMenu> BuildTree(string? parentId, List<SystemMenu> allMenus)
        {
            parentId = string.IsNullOrEmpty(parentId) ? string.Empty : parentId;
            return allMenus
                .Where(x => x.MaCha == parentId)
                .Select(x => new SystemMenu
                {
                    MaCongViec = x.MaCongViec,
                    TenCongViec = x.TenCongViec,
                    TenController = x.TenController,
                    TenAction = x.TenAction,
                    GhiChu = x.GhiChu,
                    DoUuTien = x.DoUuTien,
                    MaCha = x.MaCha,
                    MaVuViec = x.MaVuViec,
                    SoLuongCongViecCon = x.SoLuongCongViecCon,
                    Children = BuildTree(x.MaCongViec, allMenus)
                }).ToList();
        }
    }
}

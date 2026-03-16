using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Application.KyThanhToan;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class KyThanhToanService : IKyThanhToanService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<KyThanhToanService> _logger;
        public KyThanhToanService(AppDbContext context, ILogger<KyThanhToanService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SystemKyThanhToan>> GetKyThanhToanAsync(string maDuAn, string maKhachHang, string maHopDong)
        {
            var listTDTT = new List<SystemKyThanhToan>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_TienDoThucHienHopDong_API";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@maDuAn", maDuAn));
                command.Parameters.Add(new SqlParameter("@maKhachHang", maKhachHang));
                command.Parameters.Add(new SqlParameter("@maHopDong", maHopDong));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    listTDTT.Add(new SystemKyThanhToan
                    {
                        MaKyTT = reader["MaKyTT"]?.ToString(),
                        TenKyTT = reader["TenKyTT"]?.ToString(),
                        MaDuAn = reader["MaDuAn"]?.ToString(),
                        ThuTuHT = string.IsNullOrEmpty(reader["ThuTuHT"]?.ToString()) ? 1 : Convert.ToInt32(reader["ThuTuHT"]?.ToString()),
                        NgayDuKien = string.IsNullOrEmpty(reader["NgayDuKien"]?.ToString()) ? null : (Convert.ToDateTime(reader["NgayDuKien"]?.ToString())),
                        NgayThucHien = string.IsNullOrEmpty(reader["NgayThucHien"]?.ToString()) ? null : (Convert.ToDateTime(reader["NgayThucHien"]?.ToString())),
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách kỳ thanh toán: ");
            }
            return listTDTT;
        }
    }
}

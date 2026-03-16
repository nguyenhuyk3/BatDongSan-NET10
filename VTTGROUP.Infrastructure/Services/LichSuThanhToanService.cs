using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Application.KhachHang;
using VTTGROUP.Application.LichSuThanhToan;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class LichSuThanhToanService : ILichSuThanhToanService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LichSuThanhToanService> _logger;
        public LichSuThanhToanService(AppDbContext context, ILogger<LichSuThanhToanService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SystemLichSuThanhToan>> GetLichSuThanhToanAsync(string maDuAn, string maKhachHang, string maCanHo, string maHopDong, string maGiaiDoanTT)
        {
            var listTDTT = new List<SystemLichSuThanhToan>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_LichSuThanhToan_API";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@maDuAn", maDuAn));
                command.Parameters.Add(new SqlParameter("@maKhachHang", maKhachHang));
                command.Parameters.Add(new SqlParameter("@maCanHo", maCanHo));
                command.Parameters.Add(new SqlParameter("@MaHopDong", maHopDong));
                command.Parameters.Add(new SqlParameter("@maGiaiDoanTT", maGiaiDoanTT));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    listTDTT.Add(new SystemLichSuThanhToan
                    {
                        projectCode = reader["projectCode"]?.ToString(),
                        customerCode = reader["customerCode"]?.ToString(),
                        apartmentCode = reader["apartmentCode"]?.ToString(),
                        paidAmount = string.IsNullOrEmpty(reader["paidAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["paidAmount"]?.ToString()),
                        paymentDate = string.IsNullOrEmpty(reader["paymentDate"]?.ToString()) ? null : (Convert.ToDateTime(reader["paymentDate"]?.ToString())),
                        maChungTu = reader["maChungTu"]?.ToString(),
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lịch sử thanh toán: ");
            }
            return listTDTT;
        }
    }
}

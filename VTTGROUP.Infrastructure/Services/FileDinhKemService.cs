using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Application.FileDinhKem;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{

    public class FileDinhKemService : IFileDinhKemService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FileDinhKemService> _logger;
        public FileDinhKemService(AppDbContext context, ILogger<FileDinhKemService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SystemFileDinhKem>> GetFileDinhKemAsync(string maDuAn, string maKhachHang, string maHopDong, string maKyTT)
        {
            var listTDTT = new List<SystemFileDinhKem>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_TienDoThucHienHopDong_FileDinhKem_API";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@maDuAn", maDuAn));
                command.Parameters.Add(new SqlParameter("@maKhachHang", maKhachHang));
                command.Parameters.Add(new SqlParameter("@maHopDong", maHopDong));
                command.Parameters.Add(new SqlParameter("@maKyTT", maKyTT));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    listTDTT.Add(new SystemFileDinhKem
                    {
                        MaPhieu = reader["MaPhieu"]?.ToString(),
                        TenFileDinhKem = reader["TenFileDinhKem"]?.ToString(),
                        TenFileDinhKemLuu = reader["TenFileDinhKemLuu"]?.ToString(),
                        FileSize = reader["FileSize"]?.ToString(),
                        FileType = reader["FileType"]?.ToString(),
                        FullDomain = reader["FullDomain"]?.ToString(),
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách file đính kèm: ");
            }
            return listTDTT;
        }
    }
}

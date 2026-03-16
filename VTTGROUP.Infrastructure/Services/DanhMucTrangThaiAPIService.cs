using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Application.DanhMucTrangThai;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class DanhMucTrangThaiAPIService : IDanhMucTrangThaiService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DanhMucTrangThaiAPIService> _logger;
        public DanhMucTrangThaiAPIService(AppDbContext context, ILogger<DanhMucTrangThaiAPIService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<SysDanhMucTrangThai>> GetDanhMucTrangThaiAsync(int? pageIndex, int? numOfPage)
        {
            var listDMTT = new List<SysDanhMucTrangThai>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_DanhMucTrangThai_API";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@pageIndex", pageIndex));
                command.Parameters.Add(new SqlParameter("@numOfPage", numOfPage));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    listDMTT.Add(new SysDanhMucTrangThai
                    {
                        StatusCode = Convert.ToInt32(reader["StatusCode"]?.ToString()),
                        StatusNameVi = reader["StatusNameVi"]?.ToString(),
                        StatusNameEn = reader["StatusNameEn"]?.ToString(),
                        StatusColor = reader["StatusColor"]?.ToString(),
                        StatusBgColor = reader["StatusBgColor"]?.ToString(),
                        StatusTextColor = reader["StatusTextColor"]?.ToString(),
                    });
                }

            }
            catch (Exception ex)
            {
                listDMTT = new List<SysDanhMucTrangThai>();
                _logger.LogError(ex, "Lỗi khi lấy danh sách trạng thái: ");
            }
            return listDMTT;
        }
    }
}

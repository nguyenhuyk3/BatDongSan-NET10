using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Application.LaiPhatQuaHan;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class LaiPhatQuaHanService : ILaiPhatQuaHanService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LaiPhatQuaHanService> _logger;
        public LaiPhatQuaHanService(AppDbContext context, ILogger<LaiPhatQuaHanService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SystemLaiPhatQuaHan>> GetLaiPhatQuaHanAsync(string maDuAn, string maKhachHang, string maCanHo, string maGiaiDoanTT)
        {
            var listTDTT = new List<SystemLaiPhatQuaHan>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_LaiPhatQuaHan_API";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@maDuAn", maDuAn));
                command.Parameters.Add(new SqlParameter("@maKhachHang", maKhachHang));
                command.Parameters.Add(new SqlParameter("@maCanHo", maCanHo));
                command.Parameters.Add(new SqlParameter("@maGiaiDoanTT", maGiaiDoanTT));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    listTDTT.Add(new SystemLaiPhatQuaHan
                    {
                        apartmentCode = reader["apartmentCode"]?.ToString(),
                        paymentCode = reader["paymentCode"]?.ToString(),
                        startDate = string.IsNullOrEmpty(reader["startDate"]?.ToString()) ? null : (Convert.ToDateTime(reader["startDate"]?.ToString())),
                        endDate = string.IsNullOrEmpty(reader["endDate"]?.ToString()) ? null : (Convert.ToDateTime(reader["endDate"]?.ToString())),
                        interestPrincipal = string.IsNullOrEmpty(reader["interestPrincipal"]?.ToString()) ? 0 : Convert.ToDouble(reader["interestPrincipal"]?.ToString()),
                        overdueDate = string.IsNullOrEmpty(reader["overdueDate"]?.ToString()) ? 0 : Convert.ToInt32(reader["overdueDate"]?.ToString()),
                        dailyPenaltyRate = string.IsNullOrEmpty(reader["dailyPenaltyRate"]?.ToString()) ? 0 : Convert.ToDouble(reader["dailyPenaltyRate"]?.ToString()),
                        interestAmount = string.IsNullOrEmpty(reader["interestAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["interestAmount"]?.ToString()),
                        paidAmount = string.IsNullOrEmpty(reader["paidAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["paidAmount"]?.ToString()),
                        discountAmount = string.IsNullOrEmpty(reader["discountAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["discountAmount"]?.ToString()),
                        amountDue = string.IsNullOrEmpty(reader["amountDue"]?.ToString()) ? 0 : Convert.ToDouble(reader["amountDue"]?.ToString()),
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách lãi phạt quá hạn: ");
            }
            return listTDTT;
        }
    }
}

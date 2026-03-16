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
using VTTGROUP.Application.TienDoThanhToan;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class TienDoThanhToanService : ITienDoThanhToanService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TienDoThanhToanService> _logger;
        public TienDoThanhToanService(AppDbContext context, ILogger<TienDoThanhToanService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SystemTienDoThanhToan>> GetTienDoThanhToanAsync(string maDuAn, string maHopDong, string maKhachHang, string QSearch,int? trangThaiThanhToan)
        {
            var listTDTT = new List<SystemTienDoThanhToan>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_TienDoThanhToan_API";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@maDuAn", maDuAn));
                command.Parameters.Add(new SqlParameter("@maHopDong", maHopDong));
                command.Parameters.Add(new SqlParameter("@maKhachHang", maKhachHang));
                command.Parameters.Add(new SqlParameter("@QSearch", QSearch));
                command.Parameters.Add(new SqlParameter("@TrangThaiThanhToan", trangThaiThanhToan));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    listTDTT.Add(new SystemTienDoThanhToan
                    {
                        projectCode = reader["projectCode"]?.ToString(),
                        customerCode = reader["customerCode"]?.ToString(),
                        apartmentCode = reader["apartmentCode"]?.ToString(),
                        apartmentName = reader["apartmentName"]?.ToString(),
                        contractCode = reader["contractCode"]?.ToString(),
                        contractAmount = string.IsNullOrEmpty(reader["contractAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["contractAmount"]?.ToString()),
                        paymentCode = reader["paymentCode"]?.ToString(),
                        paymentNameVi = reader["paymentNameVi"]?.ToString(),
                        paymentNameEn = reader["paymentNameEn"]?.ToString(),
                        paymentAmount = string.IsNullOrEmpty(reader["paymentAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["paymentAmount"]?.ToString()),
                        paidAmount = string.IsNullOrEmpty(reader["paidAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["paidAmount"]?.ToString()),
                        interestAmount = string.IsNullOrEmpty(reader["interestAmount"]?.ToString()) ? 0 : Convert.ToDouble(reader["interestAmount"]?.ToString()),
                        paymentDate = string.IsNullOrEmpty(reader["paymentDate"]?.ToString()) ? null : (Convert.ToDateTime(reader["paymentDate"]?.ToString())),
                        paymentStatusCode = string.IsNullOrEmpty(reader["paymentStatusCode"]?.ToString()) ? 0 : Convert.ToInt32(reader["paymentStatusCode"]?.ToString()),
                        paymentStatusName = reader["paymentStatusName"]?.ToString(),
                        paymentStatusNameEn = reader["paymentStatusNameEn"]?.ToString(),
                        paymentDueDate = reader["paymentDueDate"]?.ToString(),
                        StatusColor = reader["StatusColor"]?.ToString(),
                        StatusBgColor = reader["StatusBgColor"]?.ToString(),
                        StatusTextColor = reader["StatusTextColor"]?.ToString(),
                        TextNumberColor = reader["TextNumberColor"]?.ToString(),
                        isShowDetail = Convert.ToBoolean(reader["isShowDetail"]?.ToString()),
                    });
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách khách hàng: ");
            }
            return listTDTT;
        }
    }
}

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Application.KhachHang;
using VTTGROUP.Domain.Entities;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class KhachHangService : IKhachHangService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MenuService> _logger;
        public KhachHangService(AppDbContext context, ILogger<MenuService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<List<SystemKhachHang>> GetKhachHangAsync(string QSearch, char? LoaiHinh, int? Page, int? PageSize)
        {
            var lisTKH = new List<SystemKhachHang>();
            try
            {
                using var connection = _context.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "Proc_KhachHang_API";
                command.CommandType = CommandType.StoredProcedure;

                //command.Parameters.Add(new SqlParameter("@LoaiHinh", DBNull.Value));
                //command.Parameters.Add(new SqlParameter("@Page", DBNull.Value));
                //command.Parameters.Add(new SqlParameter("@PageSize", DBNull.Value));
                //command.Parameters.Add(new SqlParameter("@QSearch", DBNull.Value));

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    lisTKH.Add(new SystemKhachHang
                    {
                        MaKhachHang = reader["MaKhachHang"]?.ToString(),
                        TenKhachHang = reader["TenKhachHang"]?.ToString(),
                        LoaiIdCard = reader["LoaiIdCard"]?.ToString(),
                        TenLoaiIdCard = reader["TenLoaiIdCard"]?.ToString(),
                        SoIdCard = reader["SoIdCard"]?.ToString(),
                        NgaySinh = string.IsNullOrEmpty(reader["NgaySinh"]?.ToString()) ? null : Convert.ToDateTime(reader["NgaySinh"]?.ToString()),
                        DiaChi = reader["DiaChi"]?.ToString(),
                        SoDienThoai = reader["SoDienThoai"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        MaDuAn = reader["MaDuAn"]?.ToString(),
                        MaCanHo = reader["MaCanHo"]?.ToString(),
                        MaHopDong = reader["MaHopDong"]?.ToString(),
                        HinhAnhMatBangTang = reader["HinhAnhMatBangTang"]?.ToString(),
                        HinhAnhLayoutLoaiCan = reader["HinhAnhLayoutLoaiCan"]?.ToString(),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách khách hàng: ");
                SystemKhachHang kh = new SystemKhachHang();
                kh.TenKhachHang = "Lỗi: " + ex.Message.ToString();
                kh.MaKhachHang = "KH0001";
                kh.SoIdCard = "123456789";
                kh.SoDienThoai = "0948665257";
                kh.Email = "tuanthinvobatbai@gmail.com";
                kh.DiaChi = "17 Mai Chí Thọ";
                lisTKH.Add(kh);
            }
            return lisTKH;
        }
    }
}

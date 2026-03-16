using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.PhieuXacNhanThanhToan;
using VTTGROUP.Domain.Model.TongHopCongNoPhaiThu;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuTongHopCongNoPhaiTraService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<PhieuTongHopCongNoPhaiTraService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public PhieuTongHopCongNoPhaiTraService(IDbContextFactory<AppDbContext> factory, ILogger<PhieuTongHopCongNoPhaiTraService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }
        #region Hiển thị danh sách công nợ phải thu
        public async Task<(List<TongHopCongNoPhaiThuPaginDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, int page, int pageSize, string? qSearch, string fromDate, string toDate)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", maDuAn);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            param.Add("@NgayLapFrom", fromDate);
            param.Add("@NgayLapTo", toDate);

            var result = (await connection.QueryAsync<TongHopCongNoPhaiThuPaginDto>(
                "Proc_TongHopCongNoPhaiTra_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Chi tiết phiếu tổng hợp công nợ phải trả
        public async Task<ResultModel> GetByIdAsync(string? id, string? maDuAn, CancellationToken ct = default)
        {
            try
            {
                id = string.IsNullOrEmpty(id) ? null : id;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaDuAn", maDuAn);
                param.Add("@Page", null);
                param.Add("@PageSize", null);
                param.Add("@QSearch", id);
                param.Add("@NgayLapFrom", string.Empty);
                param.Add("@NgayLapTo", string.Empty);

                var result = (await connection.QueryAsync<TongHopCongNoPhaiThuPaginDto>(
                    "Proc_TongHopCongNoPhaiTra_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).FirstOrDefault();

                return ResultModel.SuccessWithData(result, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin chi tiết phiếu tổng hợp công nợ phải trả");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion
    }
}


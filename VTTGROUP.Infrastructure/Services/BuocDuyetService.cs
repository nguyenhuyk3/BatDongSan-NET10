using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.QuyTrinhDuyet;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class BuocDuyetService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<BuocDuyetService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        public BuocDuyetService(AppDbContext context, ILogger<BuocDuyetService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
        }

        #region Paging index
        public async Task<(List<BuocDuyetPagingDto> Data, int TotalCount)> GetPagingBuocDuyetAsync(int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<BuocDuyetPagingDto>(
                "Proc_BuocDuyet_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa , sửa
        public async Task<ResultModel> SaveBuocDuyetAsync(BuocDuyetModel? model)
        {
            try
            {
                var entity = await _context.HtBuocDuyets.FirstOrDefaultAsync(d => d.MaBuocDuyet == model.MaBuocDuyet);
                if (model.IsCreate == true)
                {
                    if (entity != null)
                        return ResultModel.Fail("Mã bước duyệt đã tồn tại");
                    int maxIdBuocDuyet = (int?)await _context.HtBuocDuyets.OrderByDescending(d => d.IdbuocDuyet).Select(d => d.IdbuocDuyet).FirstOrDefaultAsync() ?? 0;
                    HtBuocDuyet record = new HtBuocDuyet
                    {
                        MaBuocDuyet = model.MaBuocDuyet,
                        TenBuocDuyet = model.TenBuocDuyet,
                        TenHienThi = model.TenHienThi,
                        GhiChu = model.GhiChu,
                        IdbuocDuyet = maxIdBuocDuyet + 1
                    };
                    await _context.HtBuocDuyets.AddAsync(record);
                }
                else
                {
                    if (entity == null)
                        return ResultModel.Fail("Không tìm thấy thông tin bước duyệt");

                    entity.TenBuocDuyet = model.TenBuocDuyet;
                    entity.TenHienThi = model.TenHienThi;
                    entity.GhiChu = model.GhiChu;
                }

                await _context.SaveChangesAsync();
                return ResultModel.Success($"Lưu bước duyệt {model.TenBuocDuyet} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SaveBuocDuyetAsync] Lỗi khi lưu bước duyệt");
                return ResultModel.Fail($"Lỗi hệ thống: không thể lưu: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeletePhieuAsync(string? Id)
        {
            try
            {
                var entity = await _context.HtBuocDuyets.FirstOrDefaultAsync(d => d.MaBuocDuyet == Id);
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy thông tin bước duyệt.");
                }

                var delBuocDuyets = await _context.HtQuyTrinhDuyetBuocDuyets.Where(d => d.MaBuocDuyet == Id).ToListAsync();
                if (delBuocDuyets.Any())
                {
                    return ResultModel.Fail("Bước duyệt này đã phát sinh dữ liệu, không thể xóa.");
                }

                _context.HtBuocDuyets.Remove(entity);

                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xóa bước duyệt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePhieuAsync] Lỗi khi xóa bước duyệt");
                return ResultModel.Fail($"Lỗi hệ thống: không thể xóa: {ex.Message}");
            }
        }
        #endregion
    }
}

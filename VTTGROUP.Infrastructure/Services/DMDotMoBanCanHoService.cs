using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.DMDotMoBan;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class DMDotMoBanCanHoService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<DMDotMoBanCanHoService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        public DMDotMoBanCanHoService(AppDbContext context, ILogger<DMDotMoBanCanHoService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
        }

        #region Hiển thị danh sách đợt mở bán
        public async Task<(List<DanhMucDotMoBanPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? maDuAn, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<DanhMucDotMoBanPagingDto>(
                "Proc_DMDotMoBan_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa đợt mở bán
        public async Task<ResultModel> SaveDMBAsync(DotMoBanModel model)
        {
            try
            {
                var dmb = await _context.DaDanhMucDotMoBans
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(d => 
                                                                d.MaDuAn.ToLower() == model.MaDuAn && 
                                                                d.MaDotMoBan.ToLower() == model.MaDotMoBan.ToLower());
                if (dmb != null)
                {
                    return ResultModel.Fail("Đợt mở bán đã tồn tại.");
                }
                var record = new DaDanhMucDotMoBan
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaDotMoBan = model.MaDotMoBan ?? string.Empty,
                    TenDotMoBan = model.TenDotMoBan ?? string.Empty,
                    MaMau = model.MaMau,
                    ThuTuHienThi = model.ThuTuHienThi,
                    NgayBatDau = !string.IsNullOrEmpty(model.NgayBatDau) ? DateTime.ParseExact(model.NgayBatDau, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    NgayKetThuc = !string.IsNullOrEmpty(model.NgayKetThuc) ? DateTime.ParseExact(model.NgayKetThuc, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    NgayLap = DateTime.Now,
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                await _context.DaDanhMucDotMoBans.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm đợt mở bán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm dự án");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm dự án: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateByIdAsync(DotMoBanModel model)
        {
            try
            {
                var dmb = await _context.DaDanhMucDotMoBans.FirstOrDefaultAsync(d => d.MaDotMoBan.ToLower() == model.MaDotMoBan.ToLower());
                if (dmb == null)
                {
                    return ResultModel.Fail("Không tìm thấy đợt mở bán.");
                }
                dmb.MaDuAn = model.MaDuAn ?? string.Empty;
                dmb.MaDotMoBan = model.MaDotMoBan ?? string.Empty;
                dmb.TenDotMoBan = model.TenDotMoBan ?? string.Empty;
                dmb.MaMau = model.MaMau;
                dmb.ThuTuHienThi = model.ThuTuHienThi;
                dmb.NgayBatDau = !string.IsNullOrEmpty(model.NgayBatDau) ? DateTime.ParseExact(model.NgayBatDau, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                dmb.NgayKetThuc = !string.IsNullOrEmpty(model.NgayKetThuc) ? DateTime.ParseExact(model.NgayKetThuc, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                await _context.SaveChangesAsync();
                return ResultModel.Success("Cập nhật đợt mở bán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật đợt mở bán");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật đợt mở bán: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteDMBAsync(string maDotMoBan, CancellationToken ct = default)
        {
            try
            {
                _context.ChangeTracker.Clear();

                // B1: lấy đợt
                var dot = await _context.DaDanhMucDotMoBans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MaDotMoBan == maDotMoBan, ct);

                if (dot == null)
                    return ResultModel.Fail("Không tìm thấy đợt mở bán.");

                // ================== B2: KIỂM TRA RÀNG BUỘC ==================
                // 2.1 Căn hộ / sản phẩm thuộc đợt này
                var khbhCount = await _context.BhKeHoachBanHangDotMoBans
                    .CountAsync(x => x.MaDotMoBan == maDotMoBan, ct);

                // 2.2 Phiếu giữ chỗ thuộc đợt này
                var phieuGiuChoCount = await _context.BhPhieuGiuChos
                    .CountAsync(x => x.DotMoBan == maDotMoBan, ct);

                // 2.3 Hợp đồng / phiếu cọc thuộc đợt này
                var datCocCount = await _context.BhPhieuDatCocs
                    .CountAsync(x => x.MaDotMoBan == maDotMoBan, ct);

                if (khbhCount > 0 || phieuGiuChoCount > 0 || datCocCount > 0)
                {
                    // gộp message để admin biết đang dính ở đâu
                    var msg =
                        $"Đợt mở bán \"{dot.TenDotMoBan}\" đang được sử dụng nên không thể xoá. " +
                        $"Kế hoạch bán hàng đợt mở bán: {khbhCount}, Giữ chỗ: {phieuGiuChoCount}, Đặt cọc: {datCocCount}.";
                    return ResultModel.Fail(msg);
                }

                // ================== B3: XOÁ ==================
                _context.DaDanhMucDotMoBans.Remove(new DaDanhMucDotMoBan
                {
                    MaDotMoBan = maDotMoBan
                });
                await _context.SaveChangesAsync(ct);

                return ResultModel.Success($"Xóa đợt mở bán \"{dot.TenDotMoBan}\" thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteDMBAsync] Lỗi khi xóa đợt mở bán {MaDotMoBan}", maDotMoBan);
                return ResultModel.Fail("Lỗi hệ thống, vui lòng thử lại sau.");
            }
        }

        #endregion

        #region Thông tin đợt mở bán căn hộ
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                var entity = await (
                      from dmb in _context.DaDanhMucDotMoBans
                      join duan in _context.DaDanhMucDuAns on dmb.MaDuAn equals duan.MaDuAn
                      where dmb.MaDotMoBan == id
                      select new DotMoBanModel
                      {
                          MaDuAn = dmb.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaDotMoBan = dmb.MaDotMoBan,
                          TenDotMoBan = dmb.TenDotMoBan,
                          ThuTuHienThi = dmb.ThuTuHienThi ?? 1,
                          MaMau = dmb.MaMau,
                          NgayBatDau = string.Format("{0:dd/MM/yyyy}", dmb.NgayBatDau),
                          NgayKetThuc = string.Format("{0:dd/MM/yyyy}", dmb.NgayKetThuc),
                          NgayLap = dmb.NgayLap ?? DateTime.Now,
                          MaNhanVien = dmb.NguoiLap
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new DotMoBanModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một block");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion
    }
}

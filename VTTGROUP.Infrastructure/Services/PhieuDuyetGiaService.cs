using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.CanHo;
using VTTGROUP.Domain.Model.PhieuDuyetGia;
using VTTGROUP.Infrastructure.Database;
using static Dapper.SqlMapper;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuDuyetGiaService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<PhieuDuyetGiaService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public PhieuDuyetGiaService(IDbContextFactory<AppDbContext> factory, ILogger<PhieuDuyetGiaService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách kế hoạch bán hàng
        public async Task<(List<PhieuDuyetGiaPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? maDuAn, int page, int pageSize, string? qSearch, string fromDate, string toDate, string trangThaiDuyet)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            param.Add("@TrangThai", trangThaiDuyet);
            param.Add("@NgayLapFrom", fromDate);
            param.Add("@NgayLapTo", toDate);

            var result = (await connection.QueryAsync<PhieuDuyetGiaPagingDto>(
                "Proc_PhieuDuyetGia_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sủa phiếu duyệt giá

        public async Task<ResultModel> SavePhieuDuyetGiaAsync(PhieuDuyetGiaModel? model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                var maPhieu = await SinhMaPhieuAsync("PDG-", _context, 5);
                var record = new BhPhieuDuyetGium();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.MaPhieu = maPhieu;
                record.MaDuAn = model.MaDuAn;
                record.MaDotMoBan = model.MaDotMoBan;
                record.GiaBanKeHoach = model.GiaBanKeHoach;
                record.GiaBanThucTe = model.GiaBanThucTe;
                record.NgayLap = DateTime.Now;
                record.MaPhieuKh = model.MaKeHoach;
                record.NoiDung = model.NoiDung;
                record.TyLeChuyenDoi = model.TyLeChuyenDoi;
                await _context.BhPhieuDuyetGia.AddAsync(record);

                if (model.ListChinhSachThanhToan.Any())
                {
                    List<BhPhieuDuyetGiaChinhSachThanhToan> listCSTT = new List<BhPhieuDuyetGiaChinhSachThanhToan>();
                    foreach (var item in model.ListChinhSachThanhToan)
                    {
                        var r = new BhPhieuDuyetGiaChinhSachThanhToan();
                        r.MaCstt = item.MaCSTT;
                        r.MaPhieuDuyetGia = record.MaPhieu;
                        listCSTT.Add(r);
                    }
                    await _context.BhPhieuDuyetGiaChinhSachThanhToans.AddRangeAsync(listCSTT);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaPhieu, "Thêm phiếu duyệt giá theo đợt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm phiếu duyệt giá theo đợt");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu duyệt giá theo đợt: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuDuyetGiaAsync(PhieuDuyetGiaModel? model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhPhieuDuyetGia.FirstOrDefaultAsync(d => d.MaPhieu == model.MaPhieu);
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy thông tin phiếu duyệt giá theo đợt");

                entity.GiaBanThucTe = model.GiaBanThucTe;
                entity.TyLeChuyenDoi = model.TyLeChuyenDoi;
                entity.NoiDung = model.NoiDung;

                var del = await _context.BhPhieuDuyetGiaChinhSachThanhToans.Where(d => d.MaPhieuDuyetGia == entity.MaPhieu).ToListAsync();
                _context.BhPhieuDuyetGiaChinhSachThanhToans.RemoveRange(del);

                if (model.ListChinhSachThanhToan.Any())
                {
                    List<BhPhieuDuyetGiaChinhSachThanhToan> listCSTT = new List<BhPhieuDuyetGiaChinhSachThanhToan>();
                    foreach (var item in model.ListChinhSachThanhToan)
                    {
                        var r = new BhPhieuDuyetGiaChinhSachThanhToan();
                        r.MaCstt = item.MaCSTT;
                        r.MaPhieuDuyetGia = entity.MaPhieu;
                        listCSTT.Add(r);
                    }
                    await _context.BhPhieuDuyetGiaChinhSachThanhToans.AddRangeAsync(listCSTT);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật phiếu duyệt giá theo đợt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu duyệt giá theo đợt");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật phiếu duyệt giá theo đợt: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteRecordAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhPhieuDuyetGia.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy phiếu duyệt giá theo đợt");

                var del = await _context.BhPhieuDuyetGiaChinhSachThanhToans.Where(d => d.MaPhieuDuyetGia == entity.MaPhieu).ToListAsync();
                _context.BhPhieuDuyetGiaChinhSachThanhToans.RemoveRange(del);

                var deND = await _context.HtDmnguoiDuyets.Where(d => d.MaPhieu == entity.MaPhieu).ToListAsync();
                _context.HtDmnguoiDuyets.RemoveRange(deND);

                _context.BhPhieuDuyetGia.Remove(entity);
                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xóa phiếu duyệt giá theo đợt {entity.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteRecordAsync] Lỗi khi xóa phiếu duyệt giá theo đợt");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<PhieuDuyetGiaPagingDto> listPDG)
        {
            try
            {
                var ids = listPDG?
            .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
            .Select(x => x.MaPhieu!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                var targetIds = ids;
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.BhPhieuDuyetGia
                .Where(d => targetIds.Contains(d.MaPhieu))
                .ExecuteDeleteAsync();

                var c2 = await _context.BhPhieuDuyetGiaChinhSachThanhToans
             .Where(d => targetIds.Contains(d.MaPhieuDuyetGia))
             .ExecuteDeleteAsync();

                var c4 = await _context.HtDmnguoiDuyets
                    .Where(d => targetIds.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success($"Xóa danh sách phiếu duyệt giá theo đợt thành công");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách thanh phiếu duyệt giá theo đợt");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin phiếu duyệt giá
        public async Task<ResultModel> FindGetByPhieuAsync(string? id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new PhieuDuyetGiaModel();
                if (!string.IsNullOrEmpty(id))
                {
                    record = await (
                      from sp in _context.BhPhieuDuyetGia
                      join duan in _context.DaDanhMucDuAns on sp.MaDuAn equals duan.MaDuAn

                      join kh in _context.BhKeHoachBanHangs on sp.MaPhieuKh equals kh.MaPhieuKh into khGroup
                      from kh in khGroup.DefaultIfEmpty()

                      join dmb in _context.DaDanhMucDotMoBans on sp.MaDotMoBan equals dmb.MaDotMoBan into dmbGroup
                      from dmb in dmbGroup.DefaultIfEmpty()

                      where sp.MaPhieu == id
                      select new PhieuDuyetGiaModel
                      {
                          MaPhieu = sp.MaPhieu,
                          NgayLap = sp.NgayLap,
                          MaDuAn = sp.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaKeHoach = sp.MaPhieuKh,
                          TenKeHoach = kh.NoiDung,
                          NoiDung = sp.NoiDung,
                          MaNhanVien = sp.NguoiLap,
                          MaDotMoBan = sp.MaDotMoBan,
                          TenDotMoBan = dmb.TenDotMoBan,
                          GiaBanKeHoach = sp.GiaBanKeHoach ?? 0,
                          GiaBanThucTe = sp.GiaBanThucTe ?? 0,
                          TyLeChuyenDoi = sp.TyLeChuyenDoi ?? 0,
                          MaQuiTrinhDuyet = sp.MaQuiTrinhDuyet ?? 0,
                          TrangThaiDuyet = sp.TrangThaiDuyet ?? 0,

                      }).FirstOrDefaultAsync();
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("PhieuDuyetGia", record.MaPhieu);
                    record.MaNhanVienDP = ttnd == null ? string.Empty : ttnd.MaNhanVien;
                    record.TrangThaiDuyetCuoi = await _baseService.BuocDuyetCuoi(record.MaQuiTrinhDuyet);
                    if (record.TrangThaiDuyet == 0 && record.NguoiLap != null && record.NguoiLap.MaNhanVien == _currentUser.MaNhanVien)
                    {
                        record.FlagTong = true;
                    }
                    else if (record.MaNhanVienDP == _currentUser.MaNhanVien && record.TrangThaiDuyet != record.TrangThaiDuyetCuoi)
                    {
                        record.FlagTong = true;
                    }
                    record.ListChinhSachThanhToan = await (from ct in _context.BhPhieuDuyetGiaChinhSachThanhToans
                                                           join cs in _context.BhChinhSachThanhToans on ct.MaCstt equals cs.MaCstt
                                                           where ct.MaPhieuDuyetGia == id
                                                           select new PhieuDuyetGiaChinhSachThanhToanModel
                                                           {
                                                               MaCSTT = cs.MaCstt,
                                                               TenCSTT = cs.TenCstt
                                                           }).ToListAsync();
                }
                else
                {
                    record.MaPhieu = await SinhMaPhieuAsync("PDG-", _context, 5);
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    record.NgayLap = DateTime.Now;
                }
                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (Exception ex)
            {
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin phiếu duyệt giá theo đợt: {ex.Message}");
            }
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu
        public async Task<string> SinhMaPhieuAsync(string prefix, AppDbContext context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await context.BhPhieuDuyetGia
                .Where(kh => kh.MaPhieu.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaPhieu)
                .FirstOrDefaultAsync();

            // B2: Tách phần số
            int maxSo = 0;
            if (!string.IsNullOrEmpty(maLonNhat))
            {
                var soPart = maLonNhat.Replace(prefix, "");
                int.TryParse(soPart, out maxSo);
            }

            // B3: Tăng lên và format
            string maMoi = $"{prefix}{(maxSo + 1).ToString($"D{padding}")}";
            return maMoi;
        }
        #endregion

        #region Lấy giá bán theo kế hoạch
        public async Task<decimal> GetGiaBanTheoKeHoach(string maKeHoach, string maDotMoBan)
        {
            using var _context = _factory.CreateDbContext();
            var giaBanKeHoach = await _context.BhKeHoachBanHangDotMoBanGiaBans.Where(d => d.MaDotMoBan == maDotMoBan && d.MaPhieuKh == maKeHoach && d.IsXacNhan == true).Select(d => d.DonGiaTbdot).FirstOrDefaultAsync();
            if (giaBanKeHoach == null)
                return 0;
            else return giaBanKeHoach;
        }
        #endregion

        #region Get Droplist
        public async Task<List<BhKeHoachBanHang>> GetKeHoachBHAsync(string maDuAn)
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<BhKeHoachBanHang>();
            try
            {
                entity = await _context.BhKeHoachBanHangs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<BhKeHoachBanHang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách kế hoạch");
            }
            return entity;
        }

        //public async Task<List<KHDotMoBanModel>> GetDotMoBanAsync(string maDuAn)
        //{
        //	using var _context = _factory.CreateDbContext();
        //	var entity = new List<KHDotMoBanModel>();
        //	try
        //	{
        //		using var connection = new SqlConnection(_connectionString);
        //		var param = new DynamicParameters();
        //		param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);

        //		entity = (await connection.QueryAsync<KHDotMoBanModel>(
        //			"Proc_PhieuDuyetGia_DotMoBanByKeHoachMoiNhat",
        //			param,
        //			commandType: CommandType.StoredProcedure
        //		)).ToList();

        //		if (entity == null)
        //		{
        //			entity = new List<KHDotMoBanModel>();
        //		}
        //	}
        //	catch (Exception ex)
        //	{
        //		_logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách đợt mở bán theo kế hoạch mới nhất của dự án");
        //	}
        //	return entity;
        //}

        public async Task<KHDotMoBanModel> GetDotMoBanAsync(string maDuAn)
        {
            using var _context = _factory.CreateDbContext();
            var entity = new KHDotMoBanModel();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);

                entity = (await connection.QueryAsync<KHDotMoBanModel>(
                    "Proc_PhieuDuyetGia_DotMoBanByKeHoachMoiNhat",
                    param,
                    commandType: CommandType.StoredProcedure
                )).FirstOrDefault();

                if (entity == null)
                {
                    entity = new KHDotMoBanModel();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách đợt mở bán theo kế hoạch mới nhất của dự án");
            }
            return entity;
        }
        #endregion

        #region lấy danh sách chính sách thanh toán

        public async Task<(List<PhieuDuyetGiaCSTTPagingDto> Data, int TotalCount)> GetChinhSachThanhToanByDuAnPopupAsync(
     string? maDuAn, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<PhieuDuyetGiaCSTTPagingDto>(
                "Proc_PhieuDuyetGia_ChinhSachTT_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region get sơ đồ căn hộ theo đợt
        public async Task<List<SoDoCanHoModel>> GetSoDoCanHoAsync(string maDuAn, string maBlock, string? maPhieuDuyetGia)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@MaDuAn", maDuAn);
                parameters.Add("@MaDotMoBan", maBlock);
                parameters.Add("@MaPhieuDuyetGia", maPhieuDuyetGia);

                var result = await connection.QueryAsync<SoDoCanHoModel>(
                    "Proc_PhieuDuyetGia_ViewSoDoCanHo",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
        }
        #endregion
    }
}

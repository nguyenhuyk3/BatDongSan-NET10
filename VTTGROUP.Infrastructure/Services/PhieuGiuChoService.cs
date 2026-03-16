using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.KeHoachBanHang;
using VTTGROUP.Domain.Model.KhachHang;
using VTTGROUP.Domain.Model.PhieuGiuCho;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuGiuChoService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        //  private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<PhieuGiuChoService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public PhieuGiuChoService(IDbContextFactory<AppDbContext> factory, ILogger<PhieuGiuChoService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách phiếu giữ chỗ
        public async Task<(List<PhieuGiuChoPagingDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, string? maSanGG, string? trangThai, int page, int pageSize, string? qSearch, string fromDate, string toDate)
        {
            try
            {
                qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaSanGD", maSanGG);
                param.Add("@TrangThai", trangThai);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                param.Add("@NgayLapFrom", fromDate);
                param.Add("@NgayLapTo", toDate);

                var result = (await connection.QueryAsync<PhieuGiuChoPagingDto>(
                    "Proc_PhieuGiuCho_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;
                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sanh sách phiếu booking giữ chỗ sàn giao dịch");
                var result = new List<PhieuGiuChoPagingDto>();
                return (result, 0);
            }
        }
        #endregion

        #region Hiển thị danh sách phiếu giữ chỗ booking còn hiệu lực
        public async Task<(List<PhieuGiuChoPagingDto> Data, int TotalCount)> GetPagingCHLAsync(
       string? maDuAn, string? maSanGG, string? trangThai, int page, int pageSize, string? qSearch, string fromDate, string toDate)
        {
            try
            {
                qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaSanGD", maSanGG);
                param.Add("@TrangThai", "3");
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                param.Add("@NgayLapFrom", fromDate);
                param.Add("@NgayLapTo", toDate);

                var result = (await connection.QueryAsync<PhieuGiuChoPagingDto>(
                    "Proc_PhieuGiuChoConHienLuc_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;
                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sanh sách phiếu booking giữ chỗ sàn giao dịch");
                var result = new List<PhieuGiuChoPagingDto>();
                return (result, 0);
            }
        }
        #endregion

        #region Thêm, xóa, sửa phiếu giữ chỗ
        public async Task<ResultModel> SavePhieuGiuChoAsync(PhieuGiuChoModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new BhPhieuGiuCho
                {
                    MaPhieu = await SinhMaPhieuGCTuDongAsync("PGC-", 5),
                    SoPhieu = await SinhTuDonSoPhieuAsync(model.MaSanMoiGioi, model.MaDuAn, model.DotMoBan),
                    MaKhachHangTam = model.MaKhachHang,
                    MaDuAn = model.MaDuAn,
                    DotMoBan = model.DotMoBan,
                    MaSanMoiGioi = model.MaSanMoiGioi,
                    TenNhanVienMg = model.TenNhanVienMoiGioi,
                    NoiDung = model.NoiDung,
                    IsxacNhan = false,
                    SoTienGiuCho = model.SoTienGiuCho,
                    MaMatKhoi = model.MaMatKhoi,
                    MaLoaiThietKe = model.MaLoaiThietKe,
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;

                await _context.BhPhieuGiuChos.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaPhieu, "Thêm phiếu booking giữ chỗ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu booking giữ chỗ: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(PhieuGiuChoModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhPhieuGiuChos.FirstOrDefaultAsync(d => d.MaPhieu.ToLower() == model.MaPhieu.ToLower());
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu booking giữ chỗ.");
                }
                entity.SoTienGiuCho = model.SoTienGiuCho;
                entity.TenNhanVienMg = model.TenNhanVienMoiGioi;
                entity.NoiDung = model.NoiDung;
                entity.MaMatKhoi = model.MaMatKhoi;
                entity.MaLoaiThietKe = model.MaLoaiThietKe;
                entity.MaKhachHangTam = model.MaKhachHang;
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật phiếu booking giữ chỗ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật phiếu booking giữ chỗ: {ex.Message.ToString()}");
            }
        }
        public async Task<ResultModel> DeletePGCAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var pgc = await _context.BhPhieuGiuChos.Where(d => d.MaPhieu == maPhieu && string.IsNullOrWhiteSpace(d.MaPhieuTh)).FirstOrDefaultAsync();
                if (pgc == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu booking giữ chỗ");
                }
                _context.BhPhieuGiuChos.Remove(pgc);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {pgc.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePGCAsync] Lỗi khi xóa phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<PhieuGiuChoPagingDto> listPGC)
        {
            try
            {
                var ids = listPGC?
            .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
            .Select(x => x.MaPhieu!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                var targetIds = ids;
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.BhPhieuGiuChos
                .Where(d => targetIds.Contains(d.MaPhieu) && string.IsNullOrWhiteSpace(d.MaPhieuTh))
                .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success($"Xóa danh sách phiếu booking giữ chỗ thành công");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> XacNhanPhieuGiuCho(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var pgc = await _context.BhPhieuGiuChos.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (pgc == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu booking giữ chỗ");
                }
                pgc.IsxacNhan = true;
                _context.SaveChanges();
                return ResultModel.Success($"Xác nhận {pgc.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[XacNhanPhieuGiuCho] Lỗi khi xác nhận phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin phiếu giữ chỗ
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from pgc in _context.BhPhieuGiuChos
                      join khGoc in _context.KhDmkhachHangTams on pgc.MaKhachHangTam equals khGoc.MaKhachHangTam into dtKH
                      from khGoc2 in dtKH.DefaultIfEmpty()

                      join da in _context.DaDanhMucDuAns on pgc.MaDuAn equals da.MaDuAn into dtDuAn
                      from da2 in dtDuAn.DefaultIfEmpty()

                      join sgg in _context.DmSanGiaoDiches on pgc.MaSanMoiGioi equals sgg.MaSanGiaoDich into dtSGG
                      from sgg2 in dtSGG.DefaultIfEmpty()

                      join dmb in _context.DaDanhMucDotMoBans on pgc.DotMoBan equals dmb.MaDotMoBan into dtDMB
                      from dmb2 in dtDMB.DefaultIfEmpty()

                      join lch in _context.DaDanhMucLoaiCanHos on pgc.MaLoaiThietKe equals lch.MaLoaiCanHo into dtLCH
                      from lch2 in dtLCH.DefaultIfEmpty()

                      join vmk in _context.DaDanhMucViewMatKhois on pgc.MaMatKhoi equals vmk.MaMatKhoi into dtVMK
                      from vmk2 in dtVMK.DefaultIfEmpty()

                      where pgc.MaPhieu == id
                      select new PhieuGiuChoModel
                      {
                          MaPhieu = pgc.MaPhieu,
                          SoPhieu = pgc.SoPhieu ?? string.Empty,
                          MaKhachHang = pgc.MaKhachHangTam ?? string.Empty,
                          TenKhachHang = khGoc2.TenKhachHang,
                          NgayLap = khGoc2.NgayLap ?? DateTime.Now,
                          MaNhanVien = pgc.NguoiLap ?? string.Empty,
                          MaDuAn = pgc.MaDuAn ?? string.Empty,
                          TenDuAn = da2.TenDuAn,
                          DotMoBan = pgc.DotMoBan ?? string.Empty,
                          TenDotMoBan = dmb2.TenDotMoBan,
                          SoTienGiuCho = pgc.SoTienGiuCho ?? 0,
                          MaSanMoiGioi = pgc.MaSanMoiGioi ?? string.Empty,
                          TenSanGiaoDich = sgg2.TenSanGiaoDich ?? string.Empty,
                          TenNhanVienMoiGioi = pgc.TenNhanVienMg ?? string.Empty,
                          NoiDung = pgc.NoiDung ?? string.Empty,
                          ISXacNhan = pgc.IsxacNhan ?? false,
                          MaLoaiThietKe = pgc.MaLoaiThietKe ?? string.Empty,
                          TenLoaiCanHo = lch2.TenLoaiCanHo,
                          MaMatKhoi = pgc.MaMatKhoi ?? string.Empty,
                          TenMatKhoi = vmk2.TenMatKhoi,
                          MaPhieuTH = pgc.MaPhieuTh ?? string.Empty,
                          MaQuiTrinhDuyet = pgc.MaQuiTrinhDuyet ?? 0,
                          TrangThaiDuyet = pgc.TrangThaiDuyet ?? 0,
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new PhieuGiuChoModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaPhieu = await SinhMaPhieuGCTuDongAsync("PGC-", 5);
                }
                else
                {
                    var ttnd = await _baseService.ThongTinNguoiDuyet("PhieuGiuCho", entity.MaPhieu);
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    entity.MaNhanVienDP = ttnd == null ? string.Empty : ttnd.MaNhanVien;
                    entity.TrangThaiDuyetCuoi = await _baseService.BuocDuyetCuoi(entity.MaQuiTrinhDuyet);
                    if (entity.TrangThaiDuyet == 0 && entity.NguoiLap != null && entity.NguoiLap.MaNhanVien == _currentUser.MaNhanVien)
                    {
                        entity.FlagTong = true;
                    }
                    else if (entity.MaNhanVienDP == _currentUser.MaNhanVien && entity.TrangThaiDuyet != entity.TrangThaiDuyetCuoi)
                    {
                        entity.FlagTong = true;
                    }
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu giữ chỗ   
        public async Task<string> SinhMaPhieuGCTuDongAsync(string prefix, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.BhPhieuGiuChos
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

        public async Task<string> SinhTuDonSoPhieuAsync(string maSan, string maDuAn, string maDot)
        {
            try
            {
                using var _context = _factory.CreateDbContext(); _context.ChangeTracker.Clear();
                string tenDot = await _context.DaDanhMucDotMoBans.Where(d => d.MaDotMoBan == maDot).Select(d => d.TenDotMoBan ?? string.Empty).FirstOrDefaultAsync() ?? string.Empty;
                int coutDot = (await _context.BhPhieuGiuChos.Where(d => d.MaDuAn == maDuAn && d.DotMoBan == maDot && d.MaSanMoiGioi == maSan).CountAsync()) + 1;
                string maMoi = maSan + "_" + maDuAn + "_" + maDot + "_" + coutDot.ToString();
                return maMoi;
            }
            catch
            {
                return string.Empty;
            }

        }
        #endregion

        #region Load danh sách combobox

        public async Task<List<DaDanhMucDuAn>> GetByDuAnAsync()
        {
            var entity = new List<DaDanhMucDuAn>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucDuAns.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucDuAn>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách dự án");
            }
            return entity;
        }
        public async Task<List<DmSanGiaoDich>> GetBySanGiaoDichAsync()
        {
            var entity = new List<DmSanGiaoDich>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DmSanGiaoDiches.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DmSanGiaoDich>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách sàn giao dịch");
            }
            return entity;
        }

        public DaDanhMucDotMoBan GetByDotMoBanAsync(string maDuAn)
        {
            var entity = new DaDanhMucDotMoBan();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = _context.DaDanhMucDotMoBans.Where(d => d.MaDuAn == maDuAn && d.IsKichHoat == true).OrderBy(d => d.ThuTuHienThi).FirstOrDefault();
                if (entity == null)
                {
                    entity = new DaDanhMucDotMoBan();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách đợt mở bán");
            }
            return entity;
        }

        public List<DaDanhMucLoaiCanHo> GetByLoaiThietKeTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucLoaiCanHo>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = _context.DaDanhMucLoaiCanHos.Where(d => d.MaDuAn == maDuAn).ToList();
                if (entity == null)
                {
                    entity = new List<DaDanhMucLoaiCanHo>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách thiết kế");
            }
            return entity;
        }

        public List<DaDanhMucViewMatKhoi> GetBViewMatKhoiTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucViewMatKhoi>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = _context.DaDanhMucViewMatKhois.Where(d => d.MaDuAn == maDuAn).ToList();
                if (entity == null)
                {
                    entity = new List<DaDanhMucViewMatKhoi>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách view mặt khối");
            }
            return entity;
        }

        public decimal SoTienGiuChoTheoDuAn(string maDuAn)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                decimal soTienGC = _context.DaDanhMucDuAnCauHinhChungs.Where(d => d.MaDuAn == maDuAn).Select(d => d.SoTienGiuCho).FirstOrDefault() ?? 0;
                return soTienGC;
            }
            catch
            {
                return 0;
            }

        }
        #endregion

        #region Thông tin khách hàng
        public async Task<(List<KhachHangPagingDto> Data, int TotalCount)> GetPagingKhachHangPopupAsync(
     string? loaiHinh, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@LoaiHinh", !string.IsNullOrEmpty(loaiHinh) ? loaiHinh : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<KhachHangPagingDto>(
                 //"Proc_PhieuGC_KhachHangPopup_GetPaging",
                 "Proc_PhieuGC_KhachHangTamPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Hiển thị số thứ tự tăng dần theo dự án
        //public async Task<int> SoThuTuTangDanTheoDuAn(string maPhieu, string maCongTrinh, string maDotMB)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var query = _context.BhPhieuGiuChos
        //            .AsNoTracking()
        //            .Where(d => d.MaDuAn == maCongTrinh && d.DotMoBan == maDotMB);

        //        // MaxAsync trên nullable để không lỗi khi không có bản ghi
        //        int maxSTT = (await query.MaxAsync(d => (int?)d.SoTtbooking)) ?? 0;

        //        // Nếu cần số thứ tự kế tiếp:
        //        return maxSTT + 1;
        //    }
        //    catch
        //    {

        //    }
        //    return 0;
        //}

        public async Task<int> SoThuTuTangDanTheoDuAn(string maPhieu, string maCongTrinh, string maDotMB)
        {
            // maPhieu hiện tại chưa dùng đến, giữ tham số để không phải sửa chỗ gọi

            try
            {
                await using var context = _factory.CreateDbContext();

                // Query join trực tiếp 2 bảng, không load List tạm -> tối ưu cho DB lớn
                var maxSttQuery =
                    from stt in context.BhPhieuGiuChoStts.AsNoTracking()
                    join pgc in context.BhPhieuGiuChos.AsNoTracking()
                        on stt.MaPhieuGiuCho equals pgc.MaPhieu
                    where pgc.MaDuAn == maCongTrinh
                          && stt.MaDotBanHang == maDotMB
                    select (int?)stt.SoTtbooking;

                var maxStt = await maxSttQuery.MaxAsync() ?? 0;

                // Trả về số thứ tự kế tiếp
                return maxStt + 1;
            }
            catch (Exception ex)
            {
                // TODO: log lại cho đúng chuẩn hệ thống
                // _logger.LogError(ex, "Lỗi tính số thứ tự booking cho dự án {MaDuAn} - Đợt {MaDot}", maCongTrinh, maDotMB);
                return 0;
            }
        }

        public async Task<ResultModel> CapNhatSTTAsync(string maPhieu, string maDuAn, string maDotMB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maPhieu))
                    return ResultModel.Fail("Thiếu mã phiếu.");

                if (string.IsNullOrWhiteSpace(maDuAn))
                    return ResultModel.Fail("Thiếu mã công trình.");

                if (string.IsNullOrWhiteSpace(maDotMB))
                    return ResultModel.Fail("Thiếu mã đợt mở bán.");

                await using var context = _factory.CreateDbContext();

                // Lấy entity trước cho chắc
                var entity = await context.BhPhieuGiuChos
                    .FirstOrDefaultAsync(d => d.MaPhieu == maPhieu
                                           && d.MaDuAn == maDuAn
                                           && d.DotMoBan == maDotMB);

                if (entity == null)
                    return ResultModel.Fail($"Không tìm thấy phiếu booking giữ chỗ: {maPhieu}");

                // Nếu đã có STT rồi thì không cho cấp nữa (tuỳ rule bên em)
                if (entity.SoTtbooking.HasValue && entity.SoTtbooking.Value > 0)
                {
                    return ResultModel.Fail(
                        $"Phiếu {maPhieu} đã có số thứ tự booking: {entity.SoTtbooking}. Không thể cấp lại.");
                }

                // Transaction để tránh race-condition khi nhiều user cùng cập nhật
                await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                // Lấy STT lớn nhất trong phạm vi công trình + đợt mở bán -> +1
                //int nextStt = ((await context.BhPhieuGiuChos
                //                    .AsNoTracking()
                //                    .Where(d => d.MaDuAn == maDuAn && d.DotMoBan == maDotMB)
                //                    .MaxAsync(d => (int?)d.SoTtbooking)) ?? 0) + 1;

                var maxSttQuery =
                                from stt in context.BhPhieuGiuChoStts.AsNoTracking()
                                join pgc in context.BhPhieuGiuChos.AsNoTracking()
                                    on stt.MaPhieuGiuCho equals pgc.MaPhieu
                                where pgc.MaDuAn == maDuAn
                                      && stt.MaDotBanHang == maDotMB
                                select (int?)stt.SoTtbooking;

                var maxStt = (await maxSttQuery.MaxAsync() ?? 0) + 1;

                entity.SoTtbooking = maxStt + 1;

                // Insert STT booking vào bảng lịch sử
                // Lấy entitySTT có tracking để kiểm tra khi đã tồn tại phiếu giữ chỗ rùi thì cập nhật số thứ tự theo đợt và ngược lại chưa có thì insert vô
                var entitySTT = await context.BhPhieuGiuChoStts
                                          .FirstOrDefaultAsync(d => d.MaPhieuGiuCho == maPhieu && d.MaDotBanHang == maDotMB);
                if (entitySTT != null)
                {
                    entitySTT.SoTtbooking = maxStt;
                }
                else
                {
                    var sttPGC = new BhPhieuGiuChoStt
                    {
                        MaPhieuGiuCho = maPhieu,
                        MaDotBanHang = maDotMB,
                        SoTtbooking = maxStt,
                    };
                    await context.AddAsync(sttPGC);
                }

                await context.SaveChangesAsync();
                await tx.CommitAsync();

                return ResultModel.Success(
                    $"Cập nhật số thứ tự booking cho {maPhieu} = {maxStt} thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CapNhatSTTAsync] Lỗi khi cập nhật số thứ tự phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> ResetSTTAsync(string maPhieu, string maDuAn, string maDotMB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maPhieu))
                    return ResultModel.Fail("Thiếu mã phiếu.");

                if (string.IsNullOrWhiteSpace(maDuAn))
                    return ResultModel.Fail("Thiếu mã công trình.");

                if (string.IsNullOrWhiteSpace(maDotMB))
                    return ResultModel.Fail("Thiếu mã đợt mở bán.");

                await using var context = _factory.CreateDbContext();

                // Transaction để tránh race-condition khi nhiều user cùng cập nhật
                await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                // Lấy entity có tracking để update
                var entity = await context.BhPhieuGiuChos
                                          .FirstOrDefaultAsync(d => d.MaPhieu == maPhieu);

                if (entity == null)
                    return ResultModel.Fail($"Không tìm thấy phiếu booking giữ chỗ: {maPhieu}");

                entity.SoTtbooking = 0;

                //// Lấy entity có tracking để update
                var entitySTT = await context.BhPhieuGiuChoStts
                                          .FirstOrDefaultAsync(d => d.MaPhieuGiuCho == maPhieu && d.MaDotBanHang == maDotMB);
                if (entitySTT != null)
                {
                    entitySTT.SoTtbooking = 0;
                }
                await context.SaveChangesAsync();
                await tx.CommitAsync();

                return ResultModel.Success($"Cập nhật số thứ tự booking cho {maPhieu} = {0} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ResetSTTAsync] Lỗi khi cập nhật số thứ tự phiếu booking giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Nhân viên sàn giao dịch gửi duyệt sẽ đẩy đi trực tiếp tới 1 qui trình duyệt mới nhất và sẽ tự động đẩy người duyệt mặc định trong qui trình duyệt ra
        public async Task<string> DuyetTheoQuiTrinhSGGBatKy(string maCongViec, string maPhieu, string maNhanVien)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                int idQuiTrinh = (int?)await _context.HtQuyTrinhDuyets.Where(d => d.MaCongViec == maCongViec).OrderByDescending(d => d.Id).Select(d => d.Id).FirstOrDefaultAsync() ?? 0;
                if (idQuiTrinh == 0)
                {
                    return "ChuaTonTai";
                }
                else
                {
                    using var context = _factory.CreateDbContext();
                    maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                    using var connection = new SqlConnection(_connectionString);
                    var param = new DynamicParameters();
                    param.Add("@MaCongViec", maCongViec);
                    param.Add("@MaPhieu", maPhieu);
                    param.Add("@MaNhanVien", maNhanVien);
                    param.Add("@IdQuyTrinh", idQuiTrinh);
                    param.Add("@NoiDung", string.Empty);
                    var result = (await connection.QueryAsync<ThongTinPhieuDuyetModel>(
                           "Proc_DuyetPhieuKeTiep",
                           param,
                           commandType: CommandType.StoredProcedure
                       )).FirstOrDefault();
                    if (result != null)
                    {
                        var capNhatPhieu = await _context.BhPhieuGiuChos.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                        capNhatPhieu.TrangThaiDuyet = result.TrangThai;
                        capNhatPhieu.MaQuiTrinhDuyet = idQuiTrinh;
                        await _context.SaveChangesAsync();
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        #endregion

        #region Lấy danh sách phiếu booking đã chuyển qua các đợt
        /// <summary>
        /// Lấy dữ liệu cho màn "Chuyển booking qua đợt bán hàng mới"
        /// </summary>
        public async Task<ResultModel> GetChuyenBookingInfoAsync(string maPhieu)
        {
            if (string.IsNullOrWhiteSpace(maPhieu))
                return ResultModel.Fail("Mã phiếu không hợp lệ.");

            try
            {
                await using var context = _factory.CreateDbContext();

                // 1. Lấy thông tin phiếu chính
                var pgc = await context.BhPhieuGiuChos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu);

                if (pgc == null)
                    return ResultModel.Fail($"Không tìm thấy phiếu giữ chỗ {maPhieu}.");

                var result = new ChuyenBookingDotBanHangDto
                {
                    MaPhieu = pgc.MaPhieu,
                    MaDuAn = pgc.MaDuAn
                };

                // 2. Lấy danh sách đợt + STT booking đã có (ưu tiên BH_PhieuGiuCho_STT)
                var existingFromStt = await (
                    from st in context.BhPhieuGiuChoStts.AsNoTracking()
                    join dmb in context.DaDanhMucDotMoBans.AsNoTracking()
                        on st.MaDotBanHang equals dmb.MaDotMoBan
                    where st.MaPhieuGiuCho == maPhieu
                    select new DotBanHangBookingDto
                    {
                        MaDotBanHang = st.MaDotBanHang!,
                        TenDotBanHang = dmb.TenDotMoBan!,
                        SoTTBooking = st.SoTtbooking
                    })
                    .OrderBy(x => x.TenDotBanHang)
                    .ToListAsync();

                // 2b. Nếu bảng STT chưa có gì, fallback lấy từ BH_PhieuGiuCho
                if (!existingFromStt.Any() && !string.IsNullOrEmpty(pgc.DotMoBan))
                {
                    var dot = await context.DaDanhMucDotMoBans
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.MaDotMoBan == pgc.DotMoBan);

                    if (dot != null)
                    {
                        existingFromStt.Add(new DotBanHangBookingDto
                        {
                            MaDotBanHang = dot.MaDotMoBan!,
                            TenDotBanHang = dot.TenDotMoBan!,
                            SoTTBooking = pgc.SoTtbooking
                        });
                    }
                }

                result.ExistingDots = existingFromStt;

                // 3. Tập mã đợt đã dùng (để loại trừ khi load danh mục)
                var usedDotCodes = existingFromStt
                    .Select(x => x.MaDotBanHang)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

                // 4. Lấy danh mục đợt theo dự án, chưa dùng
                var availableDotsQuery = context.DaDanhMucDotMoBans
                    .AsNoTracking()
                    .Where(d => d.MaDuAn == pgc.MaDuAn);

                if (usedDotCodes.Any())
                {
                    availableDotsQuery = availableDotsQuery
                        .Where(d => !usedDotCodes.Contains(d.MaDotMoBan!));
                }

                var availableDots = await availableDotsQuery
                    .OrderBy(d => d.ThuTuHienThi)
                    .Select(d => new DotMoBanOptionDto
                    {
                        MaDotMoBan = d.MaDotMoBan!,
                        TenDotMoBan = d.TenDotMoBan!
                    })
                    .ToListAsync();

                result.AvailableDots = availableDots;

                return ResultModel.SuccessWithData(result);   // dùng helper hiện tại
            }
            catch (Exception ex)
            {
                // TODO: nếu có ILogger<PhieuGiuChoService> _logger:
                // _logger.LogError(ex, "Lỗi GetChuyenBookingInfoAsync. MaPhieu={MaPhieu}", maPhieu);
                return ResultModel.Fail("Có lỗi khi lấy thông tin chuyển booking. Vui lòng thử lại sau.");
            }
        }
        #endregion

        #region Chuyển booking giữ chỗ qua đợt bán hàng mới và cập nhật lại STT Booking
        public async Task<ResultModel> ChuyenBookingQuaDBHMoiAsync(string maPhieu, int sttTheoDot, string maDotMB)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maPhieu))
                    return ResultModel.Fail("Thiếu mã phiếu.");

                if (string.IsNullOrWhiteSpace(maDotMB))
                    return ResultModel.Fail("Thiếu mã đợt mở bán.");

                await using var context = _factory.CreateDbContext();

                await using var tx = await context.Database.BeginTransactionAsync();

                // Lấy entitySTT có tracking để kiểm tra khi đã tồn tại phiếu giữ chỗ rùi thì cập nhật số thứ tự theo đợt và ngược lại chưa có thì insert vô
                var entitySTT = await context.BhPhieuGiuChoStts
                                          .FirstOrDefaultAsync(d => d.MaPhieuGiuCho == maPhieu && d.MaDotBanHang == maDotMB);
                if (entitySTT != null)
                {
                    return ResultModel.Fail(
                    $"Chuyển booking giữ chỗ {maPhieu} qua đợt bán hàng mới = {maDotMB} thất bại vì đã tồn tại đợt mới rùi.");
                }
                else
                {
                    var sttPGC = new BhPhieuGiuChoStt
                    {
                        MaPhieuGiuCho = maPhieu,
                        MaDotBanHang = maDotMB,
                        SoTtbooking = sttTheoDot,
                        // Nếu có cột NgayTao / NguoiTao thì set luôn ở đây
                    };

                    await context.AddAsync(sttPGC);
                    await context.SaveChangesAsync();
                    await tx.CommitAsync();
                }

                return ResultModel.Success(
                    $"Chuyển booking giữ chỗ {maPhieu} qua đợt bán hàng mới = {maDotMB} thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChuyenBookingQuaDBHMoiAsync] Lỗi khi chuyển Booking giữ chỗ qua đợt bán hàng mới");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion
    }
}

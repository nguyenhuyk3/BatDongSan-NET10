using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.KeHoachBanHang;
using VTTGROUP.Domain.Model.KhachHang;
using VTTGROUP.Domain.Model.PhieuGiuCho;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuGiuChoSanGDService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<PhieuGiuChoSanGDService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public PhieuGiuChoSanGDService(IDbContextFactory<AppDbContext> factory, ILogger<PhieuGiuChoSanGDService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }
        #region Hiển thị danh sách phiếu giữ chỗ sàn giao dịch
        public async Task<(List<PhieuGiuChoPagingDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, string? maSanGG, string? trangThai, int page, int pageSize, string? qSearch, string fromDate, string toDate)
        {
            try
            {
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                maSanGG = NguoiLap.MaNhanVien;
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
                    "Proc_PhieuGiuChoSGD_GetPaging",
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
                record.MaSanMoiGioi = NguoiLap.MaNhanVien;
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
                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật phiếu booking giữ chổ thành công");
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

                return ResultModel.Success($"Xóa danh sách thanh phiếu booking giữ chỗ thành công");

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
                    entity.MaSanMoiGioi = entity.NguoiLap.MaSanGiaoDich;
                    entity.TenSanGiaoDich = entity.NguoiLap.TenSanGiaoDich;
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("PhieuGiuChoSanGD", entity.MaPhieu);
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

        public async Task<List<DuAnTheoSanModel>> GetByDuAnAsync()
        {
            var entity = new List<DuAnTheoSanModel>();
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                entity = await (from da in _context.DaDanhMucDuAns
                                join san in _context.DmSanGiaoDichDuAns
                                    on da.MaDuAn equals san.MaDuAn
                                where san.MaSan == NguoiLap.MaSanGiaoDich
                                select new DuAnTheoSanModel
                                {
                                    MaDuAn = da.MaDuAn,
                                    TenDuAn = da.TenDuAn
                                }).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DuAnTheoSanModel>();
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
     string? loaiHinh, string? maDuAn, string? maSanGD, int page, int pageSize, string? qSearch)
        {
            var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
            maSanGD = NguoiLap.MaNhanVien;
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@LoaiHinh", !string.IsNullOrEmpty(loaiHinh) ? loaiHinh : null);
            param.Add("@MaDuAn", maDuAn);
            param.Add("@MaSanGD", maSanGD);
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
    }
}

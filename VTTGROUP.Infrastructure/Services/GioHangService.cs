using ClosedXML.Excel;
using Dapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.GioHang;
using VTTGROUP.Domain.Model.PhieuDuyetGia;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class GioHangService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<GioHangService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public GioHangService(IDbContextFactory<AppDbContext> factory, ILogger<GioHangService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách giỏ hàng
        public async Task<(List<GioHangPagingDto> Data, int TotalCount)> GetPagingAsync(
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

            var result = (await connection.QueryAsync<GioHangPagingDto>(
                "Proc_GioHang_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa giỏ hàng
        public async Task<ResultModel> SaveGioHangAsync(GioHangModel? model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                var maPhieu = await SinhMaPhieuAsync("GH-", _context, 5);
                var record = new BhGioHang();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.MaPhieu = maPhieu;
                record.MaSoGioHang = model.MaSoGioHang;
                record.MaDuAn = model.MaDuAn;
                record.MaDotMoBan = model.MaDotMoBan;
                record.GiaBan = model.GiaBan;
                record.LoaiGioHang = model.LoaiGioHang;
                record.MaSanGiaoDich = model.MaSanGiaoDich;
                record.NgayLap = DateTime.Now;
                record.MaPhieuDuyetGia = model.MaPhieuDuyetGia;
                record.MaPhieuKh = model.MaPhieuKH;
                record.NoiDung = model.NoiDung;
                await _context.BhGioHangs.AddAsync(record);

                if (model.ListCanHo.Any())
                {
                    List<BhGioHangCanHo> listCanHo = new List<BhGioHangCanHo>();
                    foreach (var item in model.ListCanHo)
                    {
                        var r = new BhGioHangCanHo
                        {
                            MaPhieuGioHang = model.MaPhieu,
                            MaCanHo = item.MaCanHo,
                            HeSoCanHo = (decimal?)item.HeSoCanHo,
                            DienTichCanHo = item.DienTichCanHo,
                            DienTichPhanBo = item.DienTichPhanBo,
                            GiaBan = model.GiaBan,
                            GiaBanSauPhanBo = item.GiaBanSauPhanBo
                        };
                        listCanHo.Add(r);
                    }
                    await _context.BhGioHangCanHos.AddRangeAsync(listCanHo);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaPhieu, "Thêm giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm giỏ hàng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm giỏ hàng: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuAsync(GioHangModel? model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhGioHangs.FirstOrDefaultAsync(d => d.MaPhieu == model.MaPhieu);
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy thông tin giỏ hàng");

                entity.MaSanGiaoDich = model.MaSanGiaoDich;
                entity.LoaiGioHang = model.LoaiGioHang;
                entity.NoiDung = model.NoiDung;

                var del = await _context.BhGioHangCanHos.Where(d => d.MaPhieuGioHang == entity.MaPhieu).ToListAsync();
                _context.BhGioHangCanHos.RemoveRange(del);

                if (model.ListCanHo.Any())
                {
                    List<BhGioHangCanHo> listCanHo = new List<BhGioHangCanHo>();
                    foreach (var item in model.ListCanHo)
                    {
                        var r = new BhGioHangCanHo
                        {
                            MaPhieuGioHang = model.MaPhieu,
                            MaCanHo = item.MaCanHo,
                            HeSoCanHo = (decimal?)item.HeSoCanHo,
                            DienTichCanHo = item.DienTichCanHo,
                            DienTichPhanBo = item.DienTichPhanBo,
                            GiaBan = model.GiaBan,
                            GiaBanSauPhanBo = item.GiaBanSauPhanBo
                        };
                        listCanHo.Add(r);
                    }
                    await _context.BhGioHangCanHos.AddRangeAsync(listCanHo);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giỏ hàng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật giỏ hàng: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteRecordAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhGioHangs.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy giỏ hàng");

                var del = await _context.BhGioHangCanHos.Where(d => d.MaPhieuGioHang == entity.MaPhieu).ToListAsync();
                _context.BhGioHangCanHos.RemoveRange(del);

                var delND = _context.HtDmnguoiDuyets.Where(d => d.MaPhieu == maPhieu);
                _context.HtDmnguoiDuyets.RemoveRange(delND);

                _context.BhGioHangs.Remove(entity);
                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xóa giỏ hàng {entity.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteRecordAsync] Lỗi khi xóa giỏ hàng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<GioHangPagingDto> listGH)
        {
            try
            {
                var ids = listGH?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
                    .Select(x => x!.MaPhieu.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.BhGioHangCanHos
                    .Where(d => ids.Contains(d.MaPhieuGioHang))
                    .ExecuteDeleteAsync();

                var c3 = await _context.HtDmnguoiDuyets
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var cParent = await _context.BhGioHangs
                    .Where(k => ids.Contains(k.MaPhieu))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success(
                    $"Đã xóa {cParent} phiếu, {c1} giỏ hàng, {c3} người duyệt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách giỏ hàng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin giỏ hàng
        //public async Task<ResultModel> FindGetByPhieuAsync(string? id)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var record = new GioHangModel();
        //        if (!string.IsNullOrEmpty(id))
        //        {
        //            record = await (
        //              from sp in _context.BhGioHangs
        //              join duan in _context.DaDanhMucDuAns on sp.MaDuAn equals duan.MaDuAn

        //              join dmb in _context.DaDanhMucDotMoBans on sp.MaDotMoBan equals dmb.MaDotMoBan into dmbGroup
        //              from dmb in dmbGroup.DefaultIfEmpty()

        //              join san in _context.DmSanGiaoDiches on sp.MaSanGiaoDich equals san.MaSanGiaoDich into sanGroup
        //              from san in sanGroup.DefaultIfEmpty()

        //              where sp.MaPhieu == id
        //              select new GioHangModel
        //              {
        //                  MaPhieu = sp.MaPhieu,
        //                  MaSoGioHang = sp.MaSoGioHang ?? string.Empty,
        //                  NgayLap = sp.NgayLap,
        //                  MaDuAn = sp.MaDuAn ?? string.Empty,
        //                  TenDuAn = duan.TenDuAn,
        //                  NoiDung = sp.NoiDung,
        //                  MaNhanVien = sp.NguoiLap ?? string.Empty,
        //                  MaDotMoBan = sp.MaDotMoBan ?? string.Empty,
        //                  TenDotMoBan = dmb.TenDotMoBan,
        //                  GiaBan = sp.GiaBan ?? 0,
        //                  LoaiGioHang = sp.LoaiGioHang,
        //                  TenSanGiaoDich = san.TenSanGiaoDich ?? string.Empty,
        //                  MaSanGiaoDich = sp.MaSanGiaoDich ?? string.Empty,
        //                  MaQuiTrinhDuyet = sp.MaQuiTrinhDuyet ?? 0,
        //                  TrangThaiDuyet = sp.TrangThaiDuyet ?? 0,
        //                  MaPhieuDuyetGia = sp.MaPhieuDuyetGia ?? string.Empty,
        //                  MaPhieuKH = sp.MaPhieuKh ?? string.Empty,
        //                  IsDong = sp.IsDong ?? false,
        //                  NguoiDong = sp.NguoiDong ?? string.Empty,
        //                  NgayDong = sp.NgayDong
        //              }).FirstOrDefaultAsync();
        //            record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);
        //            record.ListCanHo = await (from gh in _context.BhGioHangCanHos
        //                                      join ch in _context.DaDanhMucSanPhams on gh.MaCanHo equals ch.MaSanPham into dtDong
        //                                      from ch2 in dtDong.DefaultIfEmpty()
        //                                      where gh.MaPhieuGioHang == id
        //                                      select new GioHangCanHoModel
        //                                      {
        //                                          Id = gh.Id,
        //                                          MaCanHo = gh.MaCanHo,
        //                                          TenCanHo = ch2.TenSanPham,
        //                                          HeSoCanHo = (decimal?)gh.HeSoCanHo,
        //                                          DienTichCanHo = gh.DienTichCanHo,
        //                                          DienTichPhanBo = gh.DienTichPhanBo,
        //                                          GiaBan = gh.GiaBan,
        //                                          GiaBanSauPhanBo = gh.GiaBanSauPhanBo
        //                                      }).ToListAsync();
        //            var ttnd = await _baseService.ThongTinNguoiDuyet("GioHang", record.MaPhieu);
        //            record.MaNhanVienDP = ttnd == null ? string.Empty : ttnd.MaNhanVien;
        //            record.TrangThaiDuyetCuoi = await _baseService.BuocDuyetCuoi(record.MaQuiTrinhDuyet);
        //            if (record.TrangThaiDuyet == 0 && record.NguoiLap != null && record.NguoiLap.MaNhanVien == _currentUser.MaNhanVien)
        //            {
        //                record.FlagTong = true;
        //            }
        //            else if (record.MaNhanVienDP == _currentUser.MaNhanVien && record.TrangThaiDuyet != record.TrangThaiDuyetCuoi)
        //            {
        //                record.FlagTong = true;
        //            }
        //        }
        //        else
        //        {
        //            record.MaPhieu = await SinhMaPhieuAsync("GH-", _context, 5);
        //            record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
        //            record.NgayLap = DateTime.Now;
        //            record.LoaiGioHang = false;
        //        }
        //        return ResultModel.SuccessWithData(record, string.Empty);
        //    }
        //    catch (Exception ex)
        //    {
        //        return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin giỏ hàng: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> FindGetByPhieuAsync(string? id, CancellationToken ct = default)
        {
            string? key = id?.Trim();
            await using var db = await _factory.CreateDbContextAsync(ct);

            try
            {
                // ========== CASE: Tạo mới ==========
                if (string.IsNullOrWhiteSpace(key))
                {
                    var newRecord = new GioHangModel
                    {
                        MaPhieu = await SinhMaPhieuAsync("GH-", db, 5),
                        NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(),
                        NgayLap = DateTime.Now,
                        LoaiGioHang = false,
                        ListCanHo = new List<GioHangCanHoModel>()
                    };

                    return ResultModel.SuccessWithData(newRecord, string.Empty);
                }

                // ========== CASE: Load theo phiếu ==========
                var record = await (
                    from sp in db.BhGioHangs.AsNoTracking()
                    join duan in db.DaDanhMucDuAns.AsNoTracking()
                        on sp.MaDuAn equals duan.MaDuAn

                    join dmb0 in db.DaDanhMucDotMoBans.AsNoTracking()
                        on sp.MaDotMoBan equals dmb0.MaDotMoBan into dmbGroup
                    from dmb in dmbGroup.DefaultIfEmpty()

                    join san0 in db.DmSanGiaoDiches.AsNoTracking()
                        on sp.MaSanGiaoDich equals san0.MaSanGiaoDich into sanGroup
                    from san in sanGroup.DefaultIfEmpty()

                    where sp.MaPhieu == key
                    select new GioHangModel
                    {
                        MaPhieu = sp.MaPhieu,
                        MaSoGioHang = sp.MaSoGioHang ?? string.Empty,
                        NgayLap = sp.NgayLap,

                        MaDuAn = sp.MaDuAn ?? string.Empty,
                        TenDuAn = duan.TenDuAn,

                        NoiDung = sp.NoiDung,

                        MaNhanVien = sp.NguoiLap ?? string.Empty,
                        MaDotMoBan = sp.MaDotMoBan ?? string.Empty,
                        TenDotMoBan = dmb != null ? dmb.TenDotMoBan : string.Empty,

                        GiaBan = sp.GiaBan ?? 0m,
                        LoaiGioHang = sp.LoaiGioHang,

                        MaSanGiaoDich = sp.MaSanGiaoDich ?? string.Empty,
                        TenSanGiaoDich = san != null ? san.TenSanGiaoDich ?? string.Empty : string.Empty,

                        MaQuiTrinhDuyet = sp.MaQuiTrinhDuyet ?? 0,
                        TrangThaiDuyet = sp.TrangThaiDuyet ?? 0,

                        MaPhieuDuyetGia = sp.MaPhieuDuyetGia ?? string.Empty,
                        MaPhieuKH = sp.MaPhieuKh ?? string.Empty,

                        IsDong = sp.IsDong ?? false,
                        NguoiDong = sp.NguoiDong ?? string.Empty,
                        NgayDong = sp.NgayDong
                    }
                ).FirstOrDefaultAsync(ct);

                if (record is null)
                    return ResultModel.Fail("Không tìm thấy giỏ hàng.");

                // Lấy thông tin người lập (nếu có mã NV)
                if (!string.IsNullOrWhiteSpace(record.MaNhanVien))
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);

                // ========= ListCanHo: join sản phẩm + filter theo MaDuAn =========
                var maDuAn = record.MaDuAn?.Trim() ?? string.Empty;

                var maDuAnKey = (record.MaDuAn ?? "").Trim();

                record.ListCanHo = await (
                    from gh in db.BhGioHangCanHos.AsNoTracking()
                    join ch in db.DaDanhMucSanPhams.AsNoTracking()
                        on new { MaSanPham = (gh.MaCanHo ?? "").Trim(), MaDuAn = maDuAnKey }
                        equals new { MaSanPham = (ch.MaSanPham ?? "").Trim(), MaDuAn = (ch.MaDuAn ?? "").Trim() }
                        into chGroup
                    from ch in chGroup.DefaultIfEmpty()
                    where gh.MaPhieuGioHang == key
                    select new GioHangCanHoModel
                    {
                        Id = gh.Id,
                        MaCanHo = gh.MaCanHo,
                        TenCanHo = ch != null ? ch.TenSanPham : null,

                        HeSoCanHo = (decimal?)gh.HeSoCanHo,
                        DienTichCanHo = gh.DienTichCanHo,
                        DienTichPhanBo = gh.DienTichPhanBo,
                        GiaBan = gh.GiaBan,
                        GiaBanSauPhanBo = gh.GiaBanSauPhanBo
                    }
                ).ToListAsync(ct);


                // ========= Thông tin duyệt =========
                var ttnd = await _baseService.ThongTinNguoiDuyet("GioHang", record.MaPhieu);
                record.MaNhanVienDP = ttnd?.MaNhanVien ?? string.Empty;

                record.TrangThaiDuyetCuoi = await _baseService.BuocDuyetCuoi(record.MaQuiTrinhDuyet);

                // ========= FlagTong logic =========
                var isNguoiLap = record.TrangThaiDuyet == 0
                                 && record.NguoiLap?.MaNhanVien == _currentUser.MaNhanVien;

                var isNguoiDuyetHienTai = record.MaNhanVienDP == _currentUser.MaNhanVien
                                         && record.TrangThaiDuyet != record.TrangThaiDuyetCuoi;

                record.FlagTong = isNguoiLap || isNguoiDuyetHienTai;

                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "FindGetByPhieuAsync failed. id={Id}", key);
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin giỏ hàng: {ex.Message}");
            }
        }

        #endregion

        #region Hàm tăng tự động của mã phiếu
        public async Task<string> SinhMaPhieuAsync(string prefix, AppDbContext _context, int padding = 5)
        {

            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.BhGioHangs
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

        public async Task<string> SinhMaSoGioHangAsync(string maDuAn, string maDot)
        {
            using var _context = _factory.CreateDbContext();
            var countDot = await _context.BhGioHangs.Where(d => d.MaDotMoBan == maDot && d.MaDuAn == maDuAn).CountAsync();
            var sttDot = await _context.DaDanhMucDotMoBans.Where(d => d.MaDotMoBan == maDot && d.MaDuAn == maDuAn).Select(d => d.ThuTuHienThi).FirstOrDefaultAsync();
            return $"{sttDot}.{countDot + 1}";
        }
        #endregion

        #region get Droplist
        public async Task<List<BhKeHoachBanHang>> GetKeHoachBHAsync(string maDuAn)
        {
            var entity = new List<BhKeHoachBanHang>();
            try
            {
                using var _context = _factory.CreateDbContext();
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
        public async Task<List<DmSanGiaoDich>> GetSanGiaoDichAsync()
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
        public async Task<List<KHDotMoBanModel>> GetDotMoBanAsync(string maDuAn)
        {
            var entity = new List<KHDotMoBanModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);

                entity = (await connection.QueryAsync<KHDotMoBanModel>(
                    "Proc_GioHang_DotMoBanByPhieuDuyetGiaMoiNhat",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<KHDotMoBanModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách đợt mở bán theo phiếu duyệt giá mới nhất của dự án");
            }
            return entity;
        }
        #endregion

        #region Lấy giá bán theo phiếu duyệt giá    
        public async Task<decimal?> GetGiaBanTheoDot(string maDuAn, string maDotMoBan)
        {
            using var _context = _factory.CreateDbContext();
            var giaBan = await _context.BhPhieuDuyetGia.Where(d => d.MaDotMoBan == maDotMoBan && d.MaDuAn == maDuAn).Select(d => d.GiaBanThucTe).FirstOrDefaultAsync();
            if (giaBan == null)
                return 0;
            else return giaBan;
        }
        #endregion

        #region  lấy danh sách căn hộ        
        public async Task<(List<GioHangCanHoModel> Data, int TotalCount)> GetDanhSachCanHoAsync(string maDuAn, string maDotMoBan, string maGioHang, string maBlock, string maTang, string maTruc, int page, int pageSize, string? qSearch)
        {
            var entity = new List<GioHangCanHoModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);
                param.Add("@DotMoBan", !string.IsNullOrEmpty(maDotMoBan) ? maDotMoBan : null);
                param.Add("@MaGioHang", !string.IsNullOrEmpty(maGioHang) ? maGioHang : null);
                param.Add("@Block", maBlock);
                param.Add("@Tang", maTang);
                param.Add("@Truc", maTruc);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                entity = (await connection.QueryAsync<GioHangCanHoModel>(
                    "Proc_GioHang_DanhSachCanHo",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<GioHangCanHoModel>();
                }
                int total = entity.FirstOrDefault()?.TotalCount ?? 0;

                return (entity, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách căn hộ theo phiếu duyệt giá của kế hoạch đó");
                entity = new List<GioHangCanHoModel>();
                return (entity, 0);
            }

        }
        #endregion

        #region Cập nhật loại giỏ hàng chung hoặc riêng
        public async Task<string> UpdateLoaiGioHangAsync(string maPhieu, bool giaTriMoi, string maNhanVien)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var gioHang = await _context.BhGioHangs.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (gioHang == null)
                {
                    return "TB";
                }
                BhGioHangLichSuLgh lichSu = new BhGioHangLichSuLgh();
                lichSu.MaPhieu = maPhieu;
                lichSu.NgayCapNhat = DateTime.Now;
                lichSu.MaNhanVien = maNhanVien;
                lichSu.LoaiGioHangCu = gioHang.LoaiGioHang;
                lichSu.LoaiGioHangMoi = giaTriMoi;
                gioHang.LoaiGioHang = giaTriMoi;
                await _context.BhGioHangLichSuLghs.AddRangeAsync(lichSu);
                await _context.SaveChangesAsync();
                return "TC";
            }
            catch
            {
                return "TB";
            }
        }

        public async Task<string> UpdateSanGiaoDichAsync(string maPhieu, string maSanGiaoDich, string maNhanVien)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var gioHang = await _context.BhGioHangs.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (gioHang == null)
                {
                    return "TB";
                }
                BhGioHangLichSuLgh lichSu = new BhGioHangLichSuLgh();
                lichSu.MaPhieu = maPhieu;
                lichSu.NgayCapNhat = DateTime.Now;
                lichSu.MaNhanVien = maNhanVien;
                lichSu.MaSanGiaoDichCu = gioHang.MaSanGiaoDich;
                lichSu.MaSanGiaoDichMoi = maSanGiaoDich;
                await _context.BhGioHangLichSuLghs.AddRangeAsync(lichSu);
                gioHang.MaSanGiaoDich = maSanGiaoDich;
                _context.SaveChanges();
                return "TC";
            }
            catch
            {
                return "TB";
            }
        }
        #endregion

        #region Đóng giỏ hàng
        public async Task<bool> DongGioHangAsyn(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var capNhatDongGH = await _context.BhGioHangs.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (capNhatDongGH == null)
                {
                    return false;
                }
                capNhatDongGH.IsDong = true;
                capNhatDongGH.NgayDong = DateTime.Now;
                capNhatDongGH.NguoiDong = _currentUser.MaNhanVien ?? string.Empty;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Import, download file mẫu   
        public async Task<byte[]> GenerateTemplateWithDataAsync(string templatePath, string maDuAn, string maDotMoBan)
        {
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);

            var canHoSheet = workbook.Worksheet("Sheet1");

            // Gọi store lấy dữ liệu
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaDuAn", maDuAn);
            param.Add("@DotMoBan", !string.IsNullOrEmpty(maDotMoBan) ? maDotMoBan : null);
            param.Add("@MaGioHang", string.Empty);
            param.Add("@Block", null);
            param.Add("@Tang", null);
            param.Add("@Page", null);
            param.Add("@PageSize", null);
            param.Add("@QSearch", null);
            var entity = (await connection.QueryAsync<GioHangCanHoModel>(
                  "Proc_GioHang_DanhSachCanHo",
                  param,
                  commandType: CommandType.StoredProcedure
              )).ToList();

            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            int row = 2;
            int i = 1;
            foreach (var item in entity)
            {
                canHoSheet.Cell(row, 1).Value = i.ToString();
                canHoSheet.Cell(row, 2).Value = item.MaCanHo;
                row++;
                i++;
            }

            // Set lại sheet "SanPham" là active
            // workbook.Worksheet("SanPham").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }

        public async Task<ResultModel> ImportFromExcelAsync(IBrowserFile file, string maDuAn, string maPhieuKeHoach, string maDotMoBan, string maPhieuDuyetGia, decimal giaBan)
        {
            try
            {
                using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await inputStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset lại vị trí đọc

                var items = await ReadCanHoFromExcel(memoryStream, maDuAn, maPhieuKeHoach, maDotMoBan, maPhieuDuyetGia, giaBan);
                if (items.Count == 0)
                    return ResultModel.Fail("Không có căn hộ nào được import vào. Vì mã căn hộ bạn nhập không tồn tại trong hệ thống.");
                return ResultModel.SuccessWithData(items, "Import file thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        //public async Task<List<GioHangCanHoModel>> ReadCanHoFromExcel(Stream stream, string maPhieuKeHoach, string maPhieuDuyetGia, decimal giaBan)
        //{
        //    using var _context = _factory.CreateDbContext();
        //    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        //    using var reader = ExcelReaderFactory.CreateReader(stream);
        //    var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
        //    {
        //        ConfigureDataTable = _ => new ExcelDataTableConfiguration
        //        {
        //            UseHeaderRow = true
        //        }
        //    });
        //    var table = dataset.Tables["Sheet1"];
        //    if (table == null)
        //        throw new Exception("Không tìm thấy sheet 'Sheet1' trong file Excel.");           

        //    var list = new List<GioHangCanHoModel>();

        //    foreach (DataRow row in table.Rows)
        //    {
        //        string? MaCanHo = row[1]?.ToString()?.Trim().ToUpper();              

        //        var canHo = await _context.DaDanhMucSanPhams.Where(d => d.MaSanPham == MaCanHo).FirstOrDefaultAsync();
        //        if (string.IsNullOrWhiteSpace(MaCanHo) || canHo == null)
        //        {
        //            continue;
        //        }
        //        GioHangCanHoModel ct = new GioHangCanHoModel();
        //        ct.MaCanHo = MaCanHo;
        //        ct.TenCanHo = canHo?.TenSanPham;
        //        ct.HeSoCanHo = 0;
        //        ct.DienTichCanHo = 0;
        //        ct.DienTichPhanBo = 0;
        //        ct.GiaBan = giaBan;
        //        ct.GiaBanSauPhanBo = 0;
        //        list.Add(ct);
        //    }
        //    return list;
        //}

        public async Task<List<GioHangCanHoModel>> ReadCanHoFromExcel(
     Stream stream,
     string maDuAn,
     string maPhieuKeHoach,
     string maDotMoBan,
     string maPhieuDuyetGia,
     decimal giaBan,
     CancellationToken ct = default)
        {
            if (stream is null) throw new ArgumentNullException(nameof(stream));
            if (string.IsNullOrWhiteSpace(maDuAn)) return [];
            if (string.IsNullOrWhiteSpace(maPhieuKeHoach)) return [];
            if (string.IsNullOrWhiteSpace(maDotMoBan)) return [];
            if (giaBan < 0) throw new ArgumentOutOfRangeException(nameof(giaBan), "Giá bán không hợp lệ.");

            var duAn = maDuAn.Trim();
            var maPhieuKH = maPhieuKeHoach.Trim();
            var dot = maDotMoBan.Trim();

            await using var db = await _factory.CreateDbContextAsync(ct);

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataset.Tables["Sheet1"]
                ?? throw new Exception("Không tìm thấy sheet 'Sheet1' trong file Excel.");

            // Guard: cần ít nhất 2 cột vì đọc row[1]
            if (table.Columns.Count < 2)
                throw new Exception("Sheet1 không đúng định dạng (thiếu cột Mã căn hộ).");

            // 1) Collect mã căn hộ (distinct)
            var maCanHos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (DataRow row in table.Rows)
            {
                var raw = row[1]?.ToString();
                if (string.IsNullOrWhiteSpace(raw)) continue;

                var key = raw.Trim().ToUpperInvariant();
                if (!string.IsNullOrWhiteSpace(key))
                    maCanHos.Add(key);
            }

            if (maCanHos.Count == 0)
                return [];

            var maList = maCanHos.ToList();

            // 2) Query sản phẩm theo batch + normalize key
            var sanPhamDict = await db.DaDanhMucSanPhams.AsNoTracking()
                .Where(x => x.MaDuAn == duAn && maList.Contains(x.MaSanPham))
                .Select(x => new { x.MaSanPham, x.TenSanPham })
                .ToListAsync(ct);

            var sanPhamMap = sanPhamDict
                .Where(x => !string.IsNullOrWhiteSpace(x.MaSanPham))
                .ToDictionary(
                    x => x.MaSanPham.Trim().ToUpperInvariant(),
                    x => x.TenSanPham,
                    StringComparer.OrdinalIgnoreCase);

            // 3) Query phân bổ theo batch + normalize key
            var phanBoRows = await db.BhKeHoachBanHangDotMoBanCanHos.AsNoTracking()
                .Where(x => x.MaPhieuKh == maPhieuKH
                         && x.MaDotMoBan == dot
                         && maList.Contains(x.MaCanHo))
                .Select(x => new
                {
                    x.MaCanHo,
                    HeSo = (decimal?)x.HeSoCanHo,
                    DienTich = (decimal?)x.DienTichCanHo
                })
                .ToListAsync(ct);

            var phanBoMap = phanBoRows
                .Where(x => !string.IsNullOrWhiteSpace(x.MaCanHo))
                .ToDictionary(
                    x => x.MaCanHo.Trim().ToUpperInvariant(),
                    x => new { HeSo = x.HeSo ?? 0m, DienTich = x.DienTich ?? 0m },
                    StringComparer.OrdinalIgnoreCase);

            // 4) Build result
            const int RoundAreaDigits = 2;  // m2
            const int RoundMoneyDigits = 0; // VNĐ

            var result = new List<GioHangCanHoModel>(maCanHos.Count);

            foreach (var ma in maCanHos)
            {
                // chỉ lấy căn hộ tồn tại trong danh mục sản phẩm
                if (!sanPhamMap.TryGetValue(ma, out var tenCanHo))
                    continue;

                phanBoMap.TryGetValue(ma, out var pb);

                var heSo = pb?.HeSo ?? 0m;
                var dt = pb?.DienTich ?? 0m;

                var dtPhanBo = Math.Round(dt * heSo, RoundAreaDigits, MidpointRounding.AwayFromZero);
                var giaSauPhanBo = Math.Round(dtPhanBo * giaBan, RoundMoneyDigits, MidpointRounding.AwayFromZero);

                result.Add(new GioHangCanHoModel
                {
                    MaCanHo = ma,
                    TenCanHo = tenCanHo,

                    HeSoCanHo = heSo,
                    DienTichCanHo = dt,
                    DienTichPhanBo = dtPhanBo,

                    GiaBan = giaBan,
                    GiaBanSauPhanBo = giaSauPhanBo
                });
            }

            return result;
        }


        #endregion

        #region Trả về lịch sử giỏ hàng chung hay giỏ hàng riêng       
        public async Task<List<LichSuGioHangDTO>> GetLichSuByMaPhieuAsync(
     string maPhieu,
     int loaiLS,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maPhieu))
                return [];

            var key = maPhieu.Trim();

            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                // Base query: join sàn cũ/mới + nhân viên (LEFT JOIN)
                var q =
                    from ls in db.BhGioHangLichSuLghs.AsNoTracking()
                    where ls.MaPhieu == key

                    // LEFT JOIN: Sàn cũ
                    join sgdCu in db.DmSanGiaoDiches.AsNoTracking()
                        on ls.MaSanGiaoDichCu equals sgdCu.MaSanGiaoDich into sgdCuJoin
                    from sgdCu2 in sgdCuJoin.DefaultIfEmpty()

                        // LEFT JOIN: Sàn mới
                    join sgdMoi in db.DmSanGiaoDiches.AsNoTracking()
                        on ls.MaSanGiaoDichMoi equals sgdMoi.MaSanGiaoDich into sgdMoiJoin
                    from sgdMoi2 in sgdMoiJoin.DefaultIfEmpty()

                        // LEFT JOIN: Nhân viên
                    join nv in db.TblNhanviens.AsNoTracking()
                        on ls.MaNhanVien equals nv.MaNhanVien into nvJoin
                    from nv2 in nvJoin.DefaultIfEmpty()

                    select new { ls, sgdCu2, sgdMoi2, nv2 };

                // Filter theo loại lịch sử
                q = loaiLS switch
                {
                    1 => q.Where(x => x.ls.LoaiGioHangCu != x.ls.LoaiGioHangMoi),

                    2 => q.Where(x =>
                        (x.ls.MaSanGiaoDichCu ?? "").Trim() != (x.ls.MaSanGiaoDichMoi ?? "").Trim()),

                    _ => q
                };

                var result = await q
                    .OrderByDescending(x => x.ls.NgayCapNhat)
                    .ThenByDescending(x => x.ls.Id)
                    .Select(x => new LichSuGioHangDTO
                    {
                        MaPhieu = x.ls.MaPhieu,
                        NgayCapNhat = x.ls.NgayCapNhat,

                        // Tên NV lấy từ bảng nhân viên (thêm field TenNguoiCapNhat trong DTO)
                        NguoiCapNhat = x.nv2 != null ? x.nv2.HoVaTen : null,

                        // Giỏ hàng: false=Riêng, true=Chung
                        LoaiGioHangCu = x.ls.LoaiGioHangCu,
                        LoaiGioHangMoi = x.ls.LoaiGioHangMoi,

                        // Sàn cũ/mới
                        MaSanGiaoDichCu = x.ls.MaSanGiaoDichCu,
                        TenSanGiaoDichCu = x.sgdCu2 != null ? x.sgdCu2.TenSanGiaoDich : null,

                        MaSanGiaoDichMoi = x.ls.MaSanGiaoDichMoi,
                        TenSanGiaoDichMoi = x.sgdMoi2 != null ? x.sgdMoi2.TenSanGiaoDich : null
                    })
                    .ToListAsync(ct);

                return result;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetLichSuByMaPhieuAsync failed. MaPhieu={MaPhieu}, loaiLS={LoaiLS}",
                    key, loaiLS);

                return [];
            }
        }

        #endregion
    }
}

using Dapper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.DocIO.DLS;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.ChinhSachBanHang;
using VTTGROUP.Domain.Model.ChinhSachThanhToan;
using VTTGROUP.Domain.Model.PhieuDatCoc;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ChinhSachBanHangService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<ChinhSachBanHangService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        private decimal DoanhThuDuKien;
        public ChinhSachBanHangService(IDbContextFactory<AppDbContext> factory, ILogger<ChinhSachBanHangService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách chính sách bán hàng
        public async Task<(List<ChinhSachBanHangPaginDto> Data, int TotalCount)> GetPagingAsync(
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

            var result = (await connection.QueryAsync<ChinhSachBanHangPaginDto>(
                "Proc_ChinhSachBanHang_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa chính sách bán hàng
        public async Task<ResultModel> SaveChinhSachTTAsync(ChinhSachBanHangModel model, CancellationToken ct = default)
        {
            try
            {
                if (model == null)
                    return ResultModel.Fail("Dữ liệu không hợp lệ.");

                var maDuAn = (model.MaDuAn ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(maDuAn))
                    return ResultModel.Fail("Vui lòng chọn dự án.");

                await using var db = await _factory.CreateDbContextAsync(ct);

                var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                if (nguoiLap == null || string.IsNullOrWhiteSpace(nguoiLap.MaNhanVien))
                    return ResultModel.Fail("Không xác định được người lập.");

                var maPhieu = await SinhMaPhieuAsync("CSBH-", db, 5);

                var header = new DaChinhSachBanHang
                {
                    MaPhieu = maPhieu,
                    MaDuAn = maDuAn,
                    DotBanHang = (model.MaDotMB ?? string.Empty).Trim(),
                    NoiDung = model.NoiDung,
                    NguoiLap = nguoiLap.MaNhanVien,
                    NgayLap = DateTime.Now
                };

                await db.DaChinhSachBanHangs.AddAsync(header, ct);

                // Details
                var details = BuildDetails(maPhieu, model.ListChinhSachBHs);
                if (details.Count > 0)
                    await db.DaChinhSachBanHangChiTiets.AddRangeAsync(details, ct);

                await db.SaveChangesAsync(ct);

                return ResultModel.SuccessWithId(maPhieu, "Thêm chính sách bán hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm chính sách bán hàng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm chính sách bán hàng: {ex.Message}");
            }
        }

        private static List<DaChinhSachBanHangChiTiet> BuildDetails(string maPhieu, List<ChinhSachBanHangChiTietModel>? input)
        {
            var result = new List<DaChinhSachBanHangChiTiet>();
            if (input == null || input.Count == 0) return result;

            var i = 1;

            foreach (var item in input.Where(x => x != null))
            {
                // Nếu muốn bỏ qua dòng trống hoàn toàn thì mở comment:
                //if (string.IsNullOrWhiteSpace(item.TenCSBH) &&
                //    string.IsNullOrWhiteSpace(item.MaHinhThucKM) &&
                //    string.IsNullOrWhiteSpace(item.MaLoaiDieuKienKM))
                //{
                //    continue;
                //}


                result.Add(new DaChinhSachBanHangChiTiet
                {
                    MaPhieu = maPhieu,
                    MaCsbh = $"{maPhieu}_{i:000}",
                    TenCsbh = (item.TenCSBH ?? string.Empty).Trim(),
                    MaHinhThucKm = (item.MaHinhThucKM ?? string.Empty).Trim(),
                    MaLoaiDieuKienKm = (item.MaLoaiDieuKienKM ?? string.Empty).Trim(),
                    SoLuongKm = item.SoLuongKM,
                    TuNgay = ParseVnDate(item.TuNgay),
                    DenNgay = ParseVnDate(item.DenNgay),
                    GiaTriKm = item.GiaTriKM,
                    SttUuTien = item.SttUuTien <= 0 ? 1 : item.SttUuTien
                });

                i++;
            }

            return result;
        }

        private static DateTime? ParseVnDate(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            var text = s.Trim();
            var formats = new[] { "dd/MM/yyyy", "d/M/yyyy" };

            return DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var dt)
                ? dt
                : null;
        }

        public async Task<ResultModel> UpdateByIdAsync(ChinhSachBanHangModel model, CancellationToken ct = default)
        {
            try
            {
                if (model == null)
                    return ResultModel.Fail("Dữ liệu không hợp lệ.");

                var maPhieu = (model.MaPhieu ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(maPhieu))
                    return ResultModel.Fail("Mã phiếu không hợp lệ.");

                await using var db = await _factory.CreateDbContextAsync(ct);
                await using var tx = await db.Database.BeginTransactionAsync(ct);

                var entity = await db.DaChinhSachBanHangs
                    .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu, ct);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy chính sách bán hàng nào.");

                // Update header
                entity.NoiDung = model.NoiDung;

                // 1) Xoá detail cũ trước
                await db.DaChinhSachBanHangChiTiets
                    .Where(x => x.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct); // EF Core 7/8

                // 2) Insert detail mới
                var details = BuildDetails(maPhieu, model.ListChinhSachBHs);
                if (details.Count > 0)
                    await db.DaChinhSachBanHangChiTiets.AddRangeAsync(details, ct);

                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật chính sách bán hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật chính sách bán hàng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật chính sách bán hàng: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeletePhieuAsync(string maPhieu, CancellationToken ct = default)
        {
            try
            {
                maPhieu = (maPhieu ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(maPhieu))
                    return ResultModel.Fail("Mã phiếu không hợp lệ.");

                await using var db = await _factory.CreateDbContextAsync(ct);
                await using var tx = await db.Database.BeginTransactionAsync(ct);

                var header = await db.DaChinhSachBanHangs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu, ct);

                if (header == null)
                    return ResultModel.Fail("Không tìm thấy phiếu bán hàng.");

                // Xoá các bảng liên quan trước (tránh FK nếu có)
                await db.HtDmnguoiDuyets
                    .Where(x => x.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct);

                await db.DaChinhSachBanHangChiTiets
                    .Where(x => x.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct);

                // Xoá header
                await db.DaChinhSachBanHangs
                    .Where(x => x.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct);

                await tx.CommitAsync(ct);

                return ResultModel.Success($"Xóa chính sách bán hàng {maPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePhieuAsync] Lỗi khi xóa phiếu chính sách bán hàng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<ChinhSachBanHangPaginDto>? listPBH, CancellationToken ct = default)
        {
            const int BatchSize = 1800; // an toàn dưới 2100 (SQL Server parameter limit)

            try
            {
                var ids = (listPBH ?? new List<ChinhSachBanHangPaginDto>())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
                    .Select(x => x!.MaPhieu!.Trim())
                    .Where(x => x.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var db = await _factory.CreateDbContextAsync(ct);
                await using var tx = await db.Database.BeginTransactionAsync(ct);

                var deletedDetails = 0;
                var deletedApprovers = 0;
                var deletedParents = 0;

                foreach (var batch in Chunk(ids, BatchSize))
                {
                    // 1) Child
                    deletedDetails += await db.DaChinhSachBanHangChiTiets
                        .Where(d => batch.Contains(d.MaPhieu))
                        .ExecuteDeleteAsync(ct);

                    // 2) Người duyệt (bảng liên quan)
                    deletedApprovers += await db.HtDmnguoiDuyets
                        .Where(d => batch.Contains(d.MaPhieu))
                        .ExecuteDeleteAsync(ct);

                    // 3) Parent
                    deletedParents += await db.DaChinhSachBanHangs
                        .Where(p => batch.Contains(p.MaPhieu))
                        .ExecuteDeleteAsync(ct);
                }

                await tx.CommitAsync(ct);

                return ResultModel.Success(
                    $"Đã xoá {deletedParents} phiếu, {deletedDetails} chi tiết, {deletedApprovers} người duyệt.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách phiếu chính sách bán hàng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private static IEnumerable<List<string>> Chunk(List<string> source, int size)
        {
            for (var i = 0; i < source.Count; i += size)
                yield return source.GetRange(i, Math.Min(size, source.Count - i));
        }
        #endregion

        #region Thông tin chính sách bách hàng
        public async Task<ResultModel> GetByIdAsync(string? id, CancellationToken ct = default)
        {
            try
            {
                var isNew = string.IsNullOrWhiteSpace(id);
                var phieuId = (id ?? string.Empty).Trim();

                await using var db = await _factory.CreateDbContextAsync(ct);

                if (isNew)
                {
                    var newRecord = new ChinhSachBanHangModel
                    {
                        MaPhieu = await SinhMaPhieuAsync("CSBH-", db, 5),
                        NgayLap = DateTime.Now,
                        NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(),
                        ListChinhSachBHs = new List<ChinhSachBanHangChiTietModel>()
                    };

                    return ResultModel.SuccessWithData(newRecord, string.Empty);
                }

                // ===== Header =====
                var record = await (
                    from cstt in db.DaChinhSachBanHangs.AsNoTracking()
                    join duan in db.DaDanhMucDuAns.AsNoTracking()
                        on cstt.MaDuAn equals duan.MaDuAn

                    join b in db.DaDanhMucDotMoBans.AsNoTracking()
                        on new { MaDuAn = cstt.MaDuAn, MaDot = cstt.DotBanHang }
                        equals new { MaDuAn = b.MaDuAn, MaDot = b.MaDotMoBan } into jb
                    from dmb in jb.DefaultIfEmpty()

                    where cstt.MaPhieu == phieuId
                    select new ChinhSachBanHangModel
                    {
                        MaPhieu = cstt.MaPhieu,
                        NgayLap = cstt.NgayLap,
                        MaDuAn = cstt.MaDuAn ?? string.Empty,
                        TenDuAn = duan.TenDuAn,
                        NoiDung = cstt.NoiDung,
                        MaNhanVien = cstt.NguoiLap ?? string.Empty,
                        MaQuiTrinhDuyet = cstt.MaQuiTrinhDuyet ?? 0,
                        TrangThaiDuyet = cstt.TrangThaiDuyet ?? 0,
                        TenDotMB = dmb != null ? (dmb.TenDotMoBan ?? "") : ""
                    }
                ).FirstOrDefaultAsync(ct);

                if (record == null)
                    return ResultModel.Fail("Không tìm thấy phiếu chính sách bán hàng.");

                // ===== Details (lấy DateTime? ra trước, format sau để tránh lỗi translate) =====
                var detailsRaw = await (
                    from ctcs in db.DaChinhSachBanHangChiTiets.AsNoTracking()
                    join ht in db.HtDmhinhThucKhuyenMais.AsNoTracking()
                        on ctcs.MaHinhThucKm equals ht.MaHinhThucKm into j1
                    from ht2 in j1.DefaultIfEmpty()

                    join dk in db.HtDmloaiDieuKienKhuyenMais.AsNoTracking()
                        on ctcs.MaLoaiDieuKienKm equals dk.MaLoaiDieuKienKm into j2
                    from dk2 in j2.DefaultIfEmpty()

                    where ctcs.MaPhieu == phieuId
                    select new
                    {
                        ctcs.MaPhieu,
                        ctcs.MaCsbh,
                        ctcs.TenCsbh,
                        ctcs.MaHinhThucKm,
                        TenHinhThucKm = ht2 != null ? ht2.TenHinhThucKm : null,
                        ctcs.MaLoaiDieuKienKm,
                        TenLoaiDieuKienKm = dk2 != null ? dk2.TenLoaiDieuKienKm : null,
                        ctcs.SoLuongKm,
                        ctcs.TuNgay,
                        ctcs.DenNgay,
                        ctcs.GiaTriKm,
                        ctcs.SttUuTien
                    }
                ).ToListAsync(ct);

                record.ListChinhSachBHs = detailsRaw.Select(x => new ChinhSachBanHangChiTietModel
                {
                    MaPhieu = x.MaPhieu ?? string.Empty,
                    MaCSBH = x.MaCsbh ?? string.Empty,
                    TenCSBH = x.TenCsbh ?? string.Empty,
                    MaHinhThucKM = x.MaHinhThucKm ?? string.Empty,
                    TenHinhThucKM = x.TenHinhThucKm ?? string.Empty,
                    MaLoaiDieuKienKM = x.MaLoaiDieuKienKm ?? string.Empty,
                    TenLoaiDieuKienKM = x.TenLoaiDieuKienKm ?? string.Empty,
                    SoLuongKM = x.SoLuongKm ?? 0,
                    TuNgay = x.TuNgay.HasValue ? x.TuNgay.Value.ToString("dd/MM/yyyy") : string.Empty,
                    DenNgay = x.DenNgay.HasValue ? x.DenNgay.Value.ToString("dd/MM/yyyy") : string.Empty,
                    GiaTriKM = x.GiaTriKm ?? 0,
                    SttUuTien = x.SttUuTien ?? 1
                }).OrderBy(d => d.SttUuTien).ToList();

                // ===== Các call service độc lập: chạy song song =====
                var nguoiLapTask = _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);
                var ttNguoiDuyetTask = _baseService.ThongTinNguoiDuyet("CSBH", record.MaPhieu);
                var buocCuoiTask = _baseService.BuocDuyetCuoi(record.MaQuiTrinhDuyet);

                await Task.WhenAll(nguoiLapTask, ttNguoiDuyetTask, buocCuoiTask);

                record.NguoiLap = await nguoiLapTask;
                var ttnd = await ttNguoiDuyetTask;

                record.MaNhanVienDP = ttnd?.MaNhanVien ?? string.Empty;
                record.TrangThaiDuyetCuoi = await buocCuoiTask;

                // ===== Flag quyền thao tác =====
                record.FlagTong = CanEdit(record);

                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (Exception ex)
            {
                // nếu có logger thì log thêm ở đây
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin chính sách bán hàng: {ex.Message}");
            }
        }

        private bool CanEdit(ChinhSachBanHangModel record)
        {
            var currentMaNv = _currentUser.MaNhanVien;

            // Người lập được sửa khi chưa duyệt
            if (record.TrangThaiDuyet == 0 &&
                record.NguoiLap != null &&
                string.Equals(record.NguoiLap.MaNhanVien, currentMaNv, StringComparison.OrdinalIgnoreCase))
                return true;

            // Người duyệt hiện tại được thao tác khi chưa tới bước cuối
            if (string.Equals(record.MaNhanVienDP, currentMaNv, StringComparison.OrdinalIgnoreCase) &&
                record.TrangThaiDuyet != record.TrangThaiDuyetCuoi)
                return true;

            return false;
        }

        #endregion

        #region Hàm tăng tự động của mã phiếu
        public async Task<string> SinhMaPhieuAsync(string prefix, AppDbContext context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await context.DaChinhSachBanHangs
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

        #region Load cách danh mục
        public async Task<List<DaDanhMucDotMoBan>> GetByDotMoBanAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucDotMoBan>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucDotMoBans.OrderBy(d => d.ThuTuHienThi).Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucDotMoBan>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh đợt mở bán");
            }
            return entity;
        }

        public async Task<List<HtDmhinhThucKhuyenMai>> GetByHinhThucKhuyenMaiAsync(string maDuAn)
        {
            var entity = new List<HtDmhinhThucKhuyenMai>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.HtDmhinhThucKhuyenMais.ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtDmhinhThucKhuyenMai>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách hình thức khuyến mãi");
            }
            return entity;
        }
        public async Task<List<HtDmloaiDieuKienKhuyenMai>> GetByLoaiDKKhuyenMaiAsync(string maDuAn)
        {
            var entity = new List<HtDmloaiDieuKienKhuyenMai>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.HtDmloaiDieuKienKhuyenMais.ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtDmloaiDieuKienKhuyenMai>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại điều kiện khuyến mãi");
            }
            return entity;
        }

        /// <summary>
        /// Lấy danh sách chi tiết chính sách cũ theo MaDuAn + DotBanHang.
        /// Chỉ lấy phiếu mới nhất (theo NgayLap) của tổ hợp MaDuAn+DotBanHang.
        /// </summary>
        public async Task<List<ChinhSachBanHangChiTietModel>> GetChinhSachCuByDuAnAndDotAsync(
            string maDuAn, string dotBanHang, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn) || string.IsNullOrWhiteSpace(dotBanHang))
                return new List<ChinhSachBanHangChiTietModel>();

            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                // Lấy phiếu mới nhất của MaDuAn + DotBanHang
                var maPhieu = await db.DaChinhSachBanHangs
                    .AsNoTracking()
                    .Where(x => x.MaDuAn == maDuAn && x.DotBanHang == dotBanHang)
                    .OrderByDescending(x => x.NgayLap)
                    .Select(x => x.MaPhieu)
                    .FirstOrDefaultAsync(ct);

                if (string.IsNullOrWhiteSpace(maPhieu))
                    return new List<ChinhSachBanHangChiTietModel>();

                var detailsRaw = await (
                    from ctcs in db.DaChinhSachBanHangChiTiets.AsNoTracking()
                    join ht in db.HtDmhinhThucKhuyenMais.AsNoTracking()
                        on ctcs.MaHinhThucKm equals ht.MaHinhThucKm into j1
                    from ht2 in j1.DefaultIfEmpty()
                    join dk in db.HtDmloaiDieuKienKhuyenMais.AsNoTracking()
                        on ctcs.MaLoaiDieuKienKm equals dk.MaLoaiDieuKienKm into j2
                    from dk2 in j2.DefaultIfEmpty()
                    where ctcs.MaPhieu == maPhieu
                    select new
                    {
                        ctcs.MaPhieu,
                        ctcs.MaCsbh,
                        ctcs.TenCsbh,
                        ctcs.MaHinhThucKm,
                        TenHinhThucKm = ht2 != null ? ht2.TenHinhThucKm : null,
                        ctcs.MaLoaiDieuKienKm,
                        TenLoaiDieuKienKm = dk2 != null ? dk2.TenLoaiDieuKienKm : null,
                        ctcs.SoLuongKm,
                        ctcs.TuNgay,
                        ctcs.DenNgay,
                        ctcs.GiaTriKm,
                        ctcs.SttUuTien
                    }
                ).ToListAsync(ct);

                return detailsRaw.Select(x => new ChinhSachBanHangChiTietModel
                {
                    MaPhieu = x.MaPhieu ?? string.Empty,
                    MaCSBH = x.MaCsbh ?? string.Empty,
                    TenCSBH = x.TenCsbh ?? string.Empty,
                    MaHinhThucKM = x.MaHinhThucKm ?? string.Empty,
                    TenHinhThucKM = x.TenHinhThucKm ?? string.Empty,
                    MaLoaiDieuKienKM = x.MaLoaiDieuKienKm ?? string.Empty,
                    TenLoaiDieuKienKM = x.TenLoaiDieuKienKm ?? string.Empty,
                    SoLuongKM = x.SoLuongKm ?? 0,
                    TuNgay = x.TuNgay.HasValue ? x.TuNgay.Value.ToString("dd/MM/yyyy") : string.Empty,
                    DenNgay = x.DenNgay.HasValue ? x.DenNgay.Value.ToString("dd/MM/yyyy") : string.Empty,
                    GiaTriKM = x.GiaTriKm ?? 0,
                    SttUuTien = x.SttUuTien ?? 1,
                    IsOld = true   // đánh dấu là chính sách cũ
                }).OrderBy(d => d.SttUuTien).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chính sách cũ theo MaDuAn={MaDuAn}, DotBanHang={Dot}", maDuAn, dotBanHang);
                return new List<ChinhSachBanHangChiTietModel>();
            }
        }
        #endregion
    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.KhachHang;
using VTTGROUP.Domain.Model.KhachHangTam;
using VTTGROUP.Infrastructure.Database;
using static Dapper.SqlMapper;

namespace VTTGROUP.Infrastructure.Services
{
    public class DMKhachHangService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<DMKhachHangService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        public DMKhachHangService(IDbContextFactory<AppDbContext> factory, ILogger<DMKhachHangService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
        }

        #region Hiển thị danh sách khách hàng
        public async Task<(List<KhachHangPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? loaiHinh, int page, int pageSize, string? qSearch, string fromDate, string toDate)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@LoaiHinh", !string.IsNullOrEmpty(loaiHinh) ? loaiHinh : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            param.Add("@NgayLapFrom", fromDate);
            param.Add("@NgayLapTo", toDate);
            var result = (await connection.QueryAsync<KhachHangPagingDto>(
                "Proc_KhachHangT_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa khách hàng
        //public async Task<ResultModel> SaveKhachHangTamAsync(KhachHangTamModel model)
        //{
        //    try
        //    {              
        //        //Insert bảng khách hàng
        //        using var _context = _factory.CreateDbContext();
        //        var record = new KhDmkhachHang
        //        {

        //            MaKhachHang = await SinhMaKhachHangTuDongAsync("KH-", _context, 5),
        //            TenKhachHang = model.TenKhachHang,
        //            MaDoiTuongKhachHang = model.MaDoiTuongKhachHang
        //        };
        //        var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
        //        record.MaNhanVien = NguoiLap.MaNhanVien;
        //        record.NgayLap = DateTime.Now;

        //        //Insert bảng khách hàng nguồn
        //        var recordNguon = new KhDmkhachHangNguon
        //        {
        //            MaKhachHang = record.MaKhachHang,
        //            MaKhachHangTam = model.MaKhachHangTam,
        //            //NgayCapNhat = record.NgayLap
        //        };
        //        //Insert bảng chi tiết
        //        var recordCT = new KhDmkhachHangChiTiet
        //        {
        //            MaKhachHang = record.MaKhachHang,
        //            IdlanDieuChinh = await TaoMaKhachHangMoiAsync(record.MaKhachHang, _context),
        //            SoDienThoai = model.SoDienThoai ?? string.Empty,
        //            Email = model.Email ?? string.Empty,
        //            QuocTich = model.QuocTich ?? string.Empty,
        //            NgaySinh = !string.IsNullOrEmpty(model.NgaySinh) ? DateTime.ParseExact(model.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
        //            GioiTinh = model.GioiTinh.ToString(),
        //            MaLoaiIdCard = model.MaLoaiIdCard ?? string.Empty,
        //            IdCard = model.IdCard ?? string.Empty,
        //            NgayCapIdCard = !string.IsNullOrEmpty(model.NgayCapIdCard) ? DateTime.ParseExact(model.NgayCapIdCard, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
        //            NoiCapIdCard = model.NoiCapIdCard,
        //            DiaChiThuongTru = model.DiaChiThuongTru,
        //            DiaChiLienLac = model.DiaChiHienNay,
        //            NguoiDaiDien = model.NguoiDaiDien,
        //            SoDienThoaiDaiDien = model.SoDienThoaiNguoiDaiDien,
        //            NgayCapNhat = record.NgayLap ?? DateTime.Now
        //        };
        //        await _context.KhDmkhachHangs.AddAsync(record);
        //        await _context.KhDmkhachHangNguons.AddAsync(recordNguon);
        //        await _context.KhDmkhachHangChiTiets.AddAsync(recordCT);
        //        await _context.SaveChangesAsync();
        //        //  return ResultModel.Success("Thêm khách hàng thành công");
        //        return ResultModel.SuccessWithId(record.MaKhachHang, "Thêm khách hàng thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi Thêm khách hàng");
        //        return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm khách hàng: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> SaveKhachHangTamAsync(KhachHangTamModel model, CancellationToken ct = default)
        {
            // 1) Validate input mức cơ bản (tối thiểu)
            if (model == null) return ResultModel.Fail("Dữ liệu không hợp lệ.");
            NormalizeModel(model);

            try
            {
                using var context = _factory.CreateDbContext();

                // 2) Lấy thông tin người lập
                var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();

                // 4) Transaction đảm bảo tính toàn vẹn khi insert nhiều bảng
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 5) Sinh mã khách hàng có retry khi đụng unique
                string maKhachHang;
                const int maxRetry = 3;
                int retry = 0;
                while (true)
                {
                    maKhachHang = await SinhMaKhachHangTuDongAsync("KH-", context, 5);
                    // nếu muốn, có thể check tồn tại trước (không bắt buộc vì đã có UNIQUE ở DB)
                    bool existed = await context.KhDmkhachHangs.AnyAsync(x => x.MaKhachHang == maKhachHang, ct);
                    if (!existed) break;

                    retry++;
                    if (retry >= maxRetry)
                        return ResultModel.Fail("Không thể sinh mã khách hàng (trùng nhiều lần). Vui lòng thử lại.");
                }

                // 6) Map entity chính
                // var nowUtc = DateTime.UtcNow;
                var record = new KhDmkhachHang
                {
                    MaKhachHang = maKhachHang,
                    TenKhachHang = model.TenKhachHang,
                    MaDoiTuongKhachHang = model.MaDoiTuongKhachHang,
                    MaNhanVien = nguoiLap?.MaNhanVien,
                    // Khuyến nghị: cột trong DB nên là UTC
                    NgayLap = DateTime.Now
                };

                // 7) Bảng nguồn
                var recordNguon = new KhDmkhachHangNguon
                {
                    MaKhachHang = record.MaKhachHang,
                    MaKhachHangTam = model.MaKhachHangTam
                    // NgayCapNhat nếu có: nowUtc
                };

                // 8) Bảng chi tiết
                var recordCT = new KhDmkhachHangChiTiet
                {
                    MaKhachHang = record.MaKhachHang,
                    IdlanDieuChinh = await TaoMaKhachHangMoiAsync(record.MaKhachHang, context),
                    SoDienThoai = model.SoDienThoai ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                    QuocTich = model.QuocTich ?? string.Empty,
                    NgaySinh = SqlSafe(ParseDateOrNull(model.NgaySinh)),
                    GioiTinh = MapGioiTinh(model.GioiTinh), // map enum → "M"/"F"/"U" (tuỳ chuẩn)
                    MaLoaiIdCard = model.MaLoaiIdCard ?? string.Empty,
                    IdCard = model.IdCard ?? string.Empty,
                    NgayCapIdCard = SqlSafe(ParseDateOrNull(model.NgayCapIdCard)),
                    NoiCapIdCard = model.NoiCapIdCard,
                    DiaChiThuongTru = model.DiaChiThuongTru,
                    DiaChiLienLac = model.DiaChiHienNay,
                    NguoiDaiDien = model.NguoiDaiDien,
                    SoDienThoaiDaiDien = model.SoDienThoaiNguoiDaiDien,
                    NgayCapNhat = record.NgayLap ?? DateTime.Now
                };

                // 9) Add vào DbSet (không cần AddAsync)
                context.KhDmkhachHangs.Add(record);
                context.KhDmkhachHangNguons.Add(recordNguon);
                context.KhDmkhachHangChiTiets.Add(recordCT);

                // 10) Lưu + commit
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ResultModel.SuccessWithId(record.MaKhachHang, "Thêm khách hàng thành công");
            }
            catch (DbUpdateException dbEx)
            {
                // Đoán lỗi unique & trả message dễ hiểu
                _logger.LogError(dbEx, "DbUpdateException khi thêm khách hàng. MaKhachHangTam={MaKhachHangTam}, SDT={SDT}, IdCard={IdCard}",
                    model?.MaKhachHangTam, model?.SoDienThoai, model?.IdCard);

                var msg = "Không thể thêm khách hàng. Có thể dữ liệu bị trùng (mã/CMND/SĐT).";
                return ResultModel.Fail(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống khi thêm khách hàng. MaKhachHangTam={MaKhachHangTam}", model?.MaKhachHangTam);
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        // ---------- Helpers ----------

        private static void NormalizeModel(KhachHangTamModel m)
        {
            m.TenKhachHang = m.TenKhachHang?.Trim();
            m.MaDoiTuongKhachHang = m.MaDoiTuongKhachHang?.Trim();
            m.MaKhachHangTam = m.MaKhachHangTam?.Trim();

            m.SoDienThoai = NormalizePhone(m.SoDienThoai);
            m.Email = m.Email?.Trim();
            m.QuocTich = m.QuocTich?.Trim();
            m.NgaySinh = m.NgaySinh?.Trim();
            m.MaLoaiIdCard = m.MaLoaiIdCard?.Trim();
            m.IdCard = m.IdCard?.Trim();
            m.NgayCapIdCard = m.NgayCapIdCard?.Trim();
            m.NoiCapIdCard = m.NoiCapIdCard?.Trim();
            m.DiaChiThuongTru = m.DiaChiThuongTru?.Trim();
            m.DiaChiHienNay = m.DiaChiHienNay?.Trim();
            m.NguoiDaiDien = m.NguoiDaiDien?.Trim();
            m.SoDienThoaiNguoiDaiDien = NormalizePhone(m.SoDienThoaiNguoiDaiDien);
        }

        private static string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
            var p = new string(phone.Where(char.IsDigit).ToArray()); // đơn giản: giữ số
                                                                     // nếu muốn, đổi +84 → 0: if (p.StartsWith("84")) p = "0" + p[2..];
            return p;
        }

        private static DateTime? ParseDateOrNull(string? ddMMyyyy)
        {
            if (string.IsNullOrWhiteSpace(ddMMyyyy)) return null;

            // chấp nhận "dd/MM/yyyy" hoặc "dd-MM-yyyy"
            var s = ddMMyyyy.Trim().Replace('-', '/');

            // Nếu user gõ linh tinh → null
            if (!DateTime.TryParseExact(
                    s,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var d))
                return null;

            return d;
        }

        // Bọc thêm lớp an toàn với SQL datetime (1753+). 
        // Nếu cột DB là datetime2/date thì vẫn nên giữ để tránh MinValue.
        // Nếu cột DB là datetime2/date thì vẫn nên giữ để tránh MinValue.
        private static DateTime? SqlSafe(DateTime? d)
        {
            if (!d.HasValue) return null;
            var v = d.Value;
            if (v == DateTime.MinValue || v.Year < 1753) return null; // an toàn cho kiểu datetime
            return v;
        }


        private static string MapGioiTinh(object? gioiTinh)
        {
            // tuỳ model của bạn: nếu là enum => cast; nếu đã là string => normalize
            // Ví dụ: 0=Unknown, 1=Male, 2=Female
            if (gioiTinh == null) return "U";
            if (gioiTinh is int i)
            {
                return i switch { 1 => "M", 2 => "F", _ => "U" };
            }
            var s = gioiTinh.ToString()?.Trim().ToUpperInvariant();
            return s switch { "MALE" or "M" => "M", "FEMALE" or "F" => "F", _ => "U" };
        }


        //public async Task<ResultModel> UpdateByIdAsync(KhachHangTamModel model)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var entity = await _context.KhDmkhachHangChiTiets.FirstOrDefaultAsync(d => d.MaKhachHang.ToLower() == model.MaKhachHang.ToLower());
        //        if (entity == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy khách hàng.");
        //        }
        //        var recordCT = new KhDmkhachHangChiTiet
        //        {
        //            MaKhachHang = model.MaKhachHang,
        //            IdlanDieuChinh = await TaoMaKhachHangMoiAsync(model.MaKhachHang, _context),
        //            SoDienThoai = model.SoDienThoai ?? string.Empty,
        //            Email = model.Email ?? string.Empty,
        //            QuocTich = model.QuocTich ?? string.Empty,
        //            NgaySinh = !string.IsNullOrEmpty(model.NgaySinh) ? DateTime.ParseExact(model.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
        //            GioiTinh = model.GioiTinh.ToString(),
        //            MaLoaiIdCard = model.MaLoaiIdCard ?? string.Empty,
        //            IdCard = model.IdCard ?? string.Empty,
        //            NgayCapIdCard = !string.IsNullOrEmpty(model.NgayCapIdCard) ? DateTime.ParseExact(model.NgayCapIdCard, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
        //            NoiCapIdCard = model.NoiCapIdCard,
        //            DiaChiThuongTru = model.DiaChiThuongTru,
        //            DiaChiLienLac = model.DiaChiHienNay,
        //            NguoiDaiDien = model.NguoiDaiDien,
        //            SoDienThoaiDaiDien = model.SoDienThoaiNguoiDaiDien,
        //            NgayCapNhat = DateTime.Now
        //        };
        //        await _context.KhDmkhachHangChiTiets.AddAsync(recordCT);
        //        await _context.SaveChangesAsync();
        //        return ResultModel.Success("Thêm khách hàng thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi Thêm khách hàng");
        //        return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm khách hàng: {ex.Message.ToString()}");
        //    }
        //}

        //public async Task<ResultModel> DeleteKHAsync(string maKhachHang)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var kh = await _context.KhDmkhachHangs.Where(d => d.MaKhachHang == maKhachHang).FirstOrDefaultAsync();
        //        if (kh == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy khách hàng");
        //        }
        //        var khNguon = await _context.KhDmkhachHangNguons.Where(d => d.MaKhachHang == maKhachHang).FirstOrDefaultAsync();
        //        var delKHCT = await _context.KhDmkhachHangChiTiets.Where(d => d.MaKhachHang == maKhachHang).ToListAsync();
        //        _context.KhDmkhachHangs.Remove(kh);
        //        _context.KhDmkhachHangNguons.Remove(khNguon);
        //        _context.KhDmkhachHangChiTiets.RemoveRange(delKHCT);
        //        _context.SaveChanges();
        //        return ResultModel.Success($"Xóa {kh.TenKhachHang} thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[DeleteKHTamAsync] Lỗi khi xóa khách hàng tạm");
        //        return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> UpdateByIdAsync(KhachHangTamModel model, CancellationToken ct = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.MaKhachHang))
                return ResultModel.Fail("Dữ liệu cập nhật không hợp lệ.");

            NormalizeModel(model);

            try
            {
                using var context = _factory.CreateDbContext();

                // 1) Tìm KH (chỉ cần biết là tồn tại)
                var exists = await context.KhDmkhachHangs
                                          .AnyAsync(d => d.MaKhachHang == model.MaKhachHang, ct);
                if (!exists)
                    return ResultModel.Fail("Không tìm thấy khách hàng.");

                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 2) (Tùy schema) nếu có cột hiệu lực, đóng bản ghi hiện hành
                // Ví dụ:
                // await context.KhDmkhachHangChiTiets
                //     .Where(x => x.MaKhachHang == model.MaKhachHang && x.IsHieuLuc)
                //     .ExecuteUpdateAsync(s => s
                //         .SetProperty(p => p.IsHieuLuc, false)
                //         .SetProperty(p => p.DenNgay, DateTime.UtcNow), ct);

                // 3) Tạo bản ghi chi tiết mới (phiên điều chỉnh mới)
                var recordCT = new KhDmkhachHangChiTiet
                {
                    MaKhachHang = model.MaKhachHang,
                    IdlanDieuChinh = await TaoMaKhachHangMoiAsync(model.MaKhachHang, context), // hoặc tự tính max+1
                    SoDienThoai = model.SoDienThoai ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                    QuocTich = model.QuocTich ?? string.Empty,

                    // Parse ngày an toàn + lọc ngoài range SQL datetime
                    NgaySinh = SqlSafe(ParseDateOrNull(model.NgaySinh)),
                    GioiTinh = MapGioiTinh(model.GioiTinh),

                    MaLoaiIdCard = model.MaLoaiIdCard ?? string.Empty,
                    IdCard = model.IdCard ?? string.Empty,
                    NgayCapIdCard = SqlSafe(ParseDateOrNull(model.NgayCapIdCard)),
                    NoiCapIdCard = model.NoiCapIdCard,

                    DiaChiThuongTru = model.DiaChiThuongTru,
                    DiaChiLienLac = model.DiaChiHienNay,
                    NguoiDaiDien = model.NguoiDaiDien,
                    SoDienThoaiDaiDien = model.SoDienThoaiNguoiDaiDien,

                    NgayCapNhat = DateTime.Now, // nên dùng UTC
                                                   // IsHieuLuc    = true, // nếu có cột hiệu lực
                                                   // TuNgay       = DateTime.UtcNow
                };

                context.KhDmkhachHangChiTiets.Add(recordCT);

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ResultModel.Success($"Cập nhật thông tin khách hàng \"{model.MaKhachHang}\" thành công.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi cập nhật KH {MaKhachHang}", model.MaKhachHang);
                return ResultModel.Fail("Không thể cập nhật khách hàng (có thể do ràng buộc dữ liệu).");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi hệ thống KH {MaKhachHang}", model.MaKhachHang);
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }


        public async Task<ResultModel> DeleteKHAsync(string maKhachHang, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maKhachHang))
                return ResultModel.Fail("Thiếu mã khách hàng.");

            try
            {
                using var context = _factory.CreateDbContext();
                var kh = await context.KhDmkhachHangs
                                      .FirstOrDefaultAsync(x => x.MaKhachHang == maKhachHang, ct);
                if (kh == null)
                    return ResultModel.Fail("Không tìm thấy khách hàng.");

                // (Tuỳ nghiệp vụ) Chặn xoá nếu đang được tham chiếu nơi khác
                // ví dụ: hợp đồng, phiếu đặt cọc, công nợ...
                // if (await context.KdHopDongs.AnyAsync(x => x.MaKhachHang == maKhachHang, ct))
                //     return ResultModel.Fail("Khách hàng đã phát sinh nghiệp vụ, không thể xoá.");

                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // Xoá bảng con trước (nếu FK RESTRICT)
                await context.KhDmkhachHangChiTiets
                             .Where(x => x.MaKhachHang == maKhachHang)
                             .ExecuteDeleteAsync(ct);

                await context.KhDmkhachHangNguons
                             .Where(x => x.MaKhachHang == maKhachHang)
                             .ExecuteDeleteAsync(ct);

                // Xoá bảng chính
                context.KhDmkhachHangs.Remove(kh);
                await context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);
                return ResultModel.Success($"Xoá khách hàng \"{kh.TenKhachHang}\" thành công.");
            }
            catch (DbUpdateException ex)
            {
                // thường là vi phạm FK nếu còn tham chiếu chưa xoá
                _logger.LogError(ex, "[DeleteKHAsync] Lỗi FK khi xoá {MaKhachHang}", maKhachHang);
                return ResultModel.Fail("Không thể xoá do đang được tham chiếu ở nghiệp vụ khác.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteKHAsync] Lỗi hệ thống khi xoá {MaKhachHang}", maKhachHang);
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }


        public async Task<ResultModel> DeleteListAsync(List<KhachHangPagingDto> listKHT)
        {
            try
            {
                var ids = listKHT?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaKhachHang))
                    .Select(x => x!.MaKhachHang.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.KhDmkhachHangNguons
                .Where(d => ids.Contains(d.MaKhachHang))
                .ExecuteDeleteAsync();

                var c2 = await _context.KhDmkhachHangChiTiets
               .Where(d => ids.Contains(d.MaKhachHang))
               .ExecuteDeleteAsync();

                var cParent = await _context.KhDmkhachHangs
                    .Where(k => ids.Contains(k.MaKhachHang))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success("Xóa khách hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách khách hàng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin khách hàng
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from kh in _context.KhDmkhachHangChiTiets
                      join khGoc in _context.KhDmkhachHangs on kh.MaKhachHang equals khGoc.MaKhachHang into dtKH
                      from khGoc2 in dtKH.DefaultIfEmpty()

                      join nguon in _context.KhDmkhachHangNguons on kh.MaKhachHang equals nguon.MaKhachHang into dtNguon
                      from nguon2 in dtNguon.DefaultIfEmpty()

                      join dt in _context.KhDmdoiTuongKhachHangs on khGoc2.MaDoiTuongKhachHang equals dt.MaDoiTuongKhachHang into dtDT
                      from dt2 in dtDT.DefaultIfEmpty()

                      join lc in _context.KhDmloaiCards on kh.MaLoaiIdCard equals lc.MaLoaiIdCard into dtCard
                      from lc2 in dtCard.DefaultIfEmpty()

                      join qg in _context.HtDmquocGia on kh.QuocTich equals qg.MaQuocGia into dtQG
                      from qg2 in dtQG.DefaultIfEmpty()

                      where kh.MaKhachHang == id
                      select new KhachHangTamModel
                      {
                          MaKhachHang = kh.MaKhachHang,
                          MaKhachHangTam = nguon2.MaKhachHangTam,
                          TenKhachHang = khGoc2.TenKhachHang,
                          NgayLap = khGoc2.NgayLap ?? DateTime.Now,
                          MaNhanVien = khGoc2.MaNhanVien ?? string.Empty,
                          MaDoiTuongKhachHang = khGoc2.MaDoiTuongKhachHang ?? string.Empty,
                          TenDoiTuongKhachHang = dt2.TenDoiTuongKhachHang,
                          SoDienThoai = kh.SoDienThoai ?? string.Empty,
                          Email = kh.Email ?? string.Empty,
                          QuocTich = kh.QuocTich,
                          NgaySinh = string.Format("{0:dd/MM/yyyy}", kh.NgaySinh),
                          GioiTinh = (kh.GioiTinh ?? string.Empty),
                          MaLoaiIdCard = kh.MaLoaiIdCard ?? string.Empty,
                          TenLoaiIdCard = lc2.TenLoaiIdCard,
                          IdCard = kh.IdCard ?? string.Empty,
                          NgayCapIdCard = string.Format("{0:dd/MM/yyyy}", kh.NgayCapIdCard),
                          NoiCapIdCard = kh.NoiCapIdCard ?? string.Empty,
                          DiaChiThuongTru = kh.DiaChiThuongTru ?? string.Empty,
                          DiaChiHienNay = kh.DiaChiLienLac ?? string.Empty,
                          NguoiDaiDien = kh.NguoiDaiDien ?? string.Empty,
                          SoDienThoaiNguoiDaiDien = kh.SoDienThoaiDaiDien ?? string.Empty,
                          NgayCapNhat = kh.NgayCapNhat,
                          IDLanDieuChinh = kh.IdlanDieuChinh ?? string.Empty
                      }).OrderByDescending(d => d.NgayCapNhat).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new KhachHangTamModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaKhachHang = await SinhMaKhachHangTuDongAsync("KH-", _context, 5);
                    entity.MaKhachHangTam = string.Empty;
                    entity.NgaySinh = string.Empty;
                    entity.NgayCapIdCard = string.Empty;
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một sản phẩm trong dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<(List<KhachHangTamPagingDto> Data, int TotalCount)> GetPagingPopupAsync(
       string? loaiHinh, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@LoaiHinh", !string.IsNullOrEmpty(loaiHinh) ? loaiHinh : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<KhachHangTamPagingDto>(
                "Proc_KhachHangTamPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<List<KhachHangTamModel>> GetLichSuChiTietAsync(string maKhachHang)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var list = await (
                    from kh in _context.KhDmkhachHangChiTiets
                    join lc in _context.KhDmloaiCards on kh.MaLoaiIdCard equals lc.MaLoaiIdCard into dtCard
                    from lc2 in dtCard.DefaultIfEmpty()
                    join qg in _context.HtDmquocGia on kh.QuocTich equals qg.MaQuocGia into dtQG
                    from qg2 in dtQG.DefaultIfEmpty()
                    where kh.MaKhachHang == maKhachHang
                    orderby kh.NgayCapNhat descending
                    select new KhachHangTamModel
                    {
                        IDLanDieuChinh = kh.IdlanDieuChinh ?? string.Empty,
                        GioiTinh = kh.GioiTinh ?? string.Empty,
                        NgaySinh = kh.NgaySinh.HasValue ? string.Format("{0:dd/MM/yyyy}", kh.NgaySinh) : string.Empty,
                        QuocTich = kh.QuocTich ?? string.Empty,
                        TenQuocTich = qg2.TenQuocGia ?? string.Empty,
                        MaLoaiIdCard = kh.MaLoaiIdCard ?? string.Empty,
                        TenLoaiIdCard = lc2.TenLoaiIdCard ?? string.Empty,
                        IdCard = kh.IdCard ?? string.Empty,
                        NgayCapIdCard = kh.NgayCapIdCard.HasValue ? string.Format("{0:dd/MM/yyyy}", kh.NgayCapIdCard) : string.Empty,
                        NoiCapIdCard = kh.NoiCapIdCard ?? string.Empty,
                        DiaChiThuongTru = kh.DiaChiThuongTru ?? string.Empty,
                        DiaChiHienNay = kh.DiaChiLienLac ?? string.Empty,
                        SoDienThoai = kh.SoDienThoai ?? string.Empty,
                        Email = kh.Email ?? string.Empty,
                        NgayCapNhat = kh.NgayCapNhat,
                    }).ToListAsync();

                return list;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetLichSuChiTietAsync] Lỗi khi lấy lịch sử khách hàng {MaKhachHang}", maKhachHang);
                return new List<KhachHangTamModel>();
            }
        }
        #endregion

        #region Load combobox
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
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sàn giao dịch");
            }
            return entity;
        }
        public async Task<List<KhDmnguonKhachHang>> GetNguonKhachHangAsync()
        {
            var entity = new List<KhDmnguonKhachHang>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.KhDmnguonKhachHangs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<KhDmnguonKhachHang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh đối tượng nguồn khách hàng");
            }
            return entity;
        }

        public async Task<List<KhDmdoiTuongKhachHang>> GetByLoaiKhachHangAsync()
        {
            var entity = new List<KhDmdoiTuongKhachHang>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.KhDmdoiTuongKhachHangs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<KhDmdoiTuongKhachHang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại khách hàng");
            }
            return entity;
        }
        public async Task<List<KhDmdoiTuongKhachHang>> GetByLoaiKhachHangPopupAsync()
        {
            var entity = new List<KhDmdoiTuongKhachHang>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.KhDmdoiTuongKhachHangs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<KhDmdoiTuongKhachHang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại khách hàng");
            }
            return entity;
        }
        public async Task<List<KhDmloaiCard>> GetByCardKhachHangAsync()
        {
            var entity = new List<KhDmloaiCard>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.KhDmloaiCards.ToListAsync();
                if (entity == null)
                {
                    entity = new List<KhDmloaiCard>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại card");
            }
            return entity;
        }

        public async Task<List<HtDmquocGium>> GetByQuocGiaAsync()
        {
            var entity = new List<HtDmquocGium>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.HtDmquocGia.ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtDmquocGium>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách quốc gia");
            }
            return entity;
        }
        #endregion

        #region Hàm tăng tự động của mã khách hàng     
        public async Task<string> SinhMaKhachHangTuDongAsync(string prefix, AppDbContext _context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KhDmkhachHangs
                .Where(kh => kh.MaKhachHang.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaKhachHang)
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

        #region Hàm tăng tự động của mã khách hàng khi mỗi lần cập nhật
        public async Task<string> TaoMaKhachHangMoiAsync(string maHienTai, AppDbContext _context)
        {
            var lastId = await _context.KhDmkhachHangChiTiets
                      .Where(x => x.MaKhachHang == maHienTai && x.IdlanDieuChinh.StartsWith(maHienTai + "_"))
                      .OrderByDescending(x => x.NgayCapNhat)
                      .Select(x => x.IdlanDieuChinh)
                      .FirstOrDefaultAsync();

            int nextIndex = 1;

            // Bước 2: Nếu có bản ghi trước đó, tách số ra và +1
            if (!string.IsNullOrEmpty(lastId))
            {
                var parts = lastId.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int currentIndex))
                {
                    nextIndex = currentIndex + 1;
                }
            }

            // Bước 3: Trả về mã mới, padding 3 chữ số
            return $"{maHienTai}_{nextIndex.ToString("D3")}";
        }
        #endregion
    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using VTTGROUP.Domain.Helpers;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.InPhieu;
using VTTGROUP.Domain.Model.KhachHangTam;
using VTTGROUP.Domain.Model.PhieuDatCoc;
using VTTGROUP.Infrastructure.Database;
using VTTGROUP.Infrastructure.Services.Email;
using Xceed.Words.NET;
using DocxAlignment = Xceed.Document.NET.Alignment;
using DocxCell = Xceed.Document.NET.Cell;
namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuDatCocService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<PhieuDatCocService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;
        public PhieuDatCocService(IDbContextFactory<AppDbContext> factory, ILogger<PhieuDatCocService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService, IEmailService emailService, IEmailTemplateService templateService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
            _emailService = emailService;
            _templateService = templateService;
        }

        #region Hiển thị danh sách phiếu đặt cọc
        public async Task<(List<PhieuDatCocPaginDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, int page, int pageSize, string? qSearch, string? trangThai, string fromDate, string toDate)
        {
            try
            {
                qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaDuAn", maDuAn);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                param.Add("@TrangThai", trangThai);
                param.Add("@NgayLapFrom", fromDate);
                param.Add("@NgayLapTo", toDate);

                var result = (await connection.QueryAsync<PhieuDatCocPaginDto>(
                    "Proc_PhieuDatCoc_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;

                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sanh sách phiếu đặt cọc");
                var result = new List<PhieuDatCocPaginDto>();
                return (result, 0);
            }
        }
        #endregion

        #region Thêm, xóa, sửa phiếu đặt cọc        
        public async Task<ResultModel> SavePhieuDatCocAsync(
    PhieuDatCocModel model,
    List<PhieuDatCocTienDoThanhToanModel>? listTDTT, string _env,
    CancellationToken ct = default)
        {
            if (model is null)
                return ResultModel.Fail("Dữ liệu phiếu đặt cọc không hợp lệ.");

            // Track các file đã lưu để cleanup nếu DB fail/rollback
            var savedRelativePaths = new List<string>();

            try
            {
                await using var context = _factory.CreateDbContext();
                var strategy = context.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await context.Database.BeginTransactionAsync(ct);

                    // Người lập
                    var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    var maNhanVienLap = (nguoiLap?.MaNhanVien ?? string.Empty).Trim();

                    //// Sinh mã phiếu
                    var maPhieuDc = await SinhMaPhieuDCTuDongAsync("PDC-", context, 5);
                    if (string.IsNullOrWhiteSpace(maPhieuDc))
                        return ResultModel.Fail("Không thể sinh mã phiếu đặt cọc.");

                    // Parse ngày ký an toàn
                    DateTime? ngayKi = null;
                    if (!string.IsNullOrWhiteSpace(model.NgayKi)
                        && DateTime.TryParseExact(model.NgayKi.Trim(), "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedNgayKi))
                    {
                        ngayKi = parsedNgayKi;
                    }

                    // ===== Parent: BH_PhieuDatCoc =====
                    var record = new BhPhieuDatCoc
                    {
                        MaPhieuDc = maPhieuDc,
                        SoPhieuDc = model.SoPhieuDC,

                        MaKhachHang = (model.MaKhachHang ?? string.Empty).Trim(),
                        IdkhachHangCt = model.IDKhachHangCT,

                        MaDuAn = model.MaDuAn,
                        MaDotMoBan = model.DotMoBan,
                        MaPhieuDangKy = model.MaPhieuDK,
                        MaCanHo = model.MaCanHo,
                        MaChinhSachTt = model.MaChinhSachTT,

                        NgayKi = ngayKi,
                        GhiChu = model.GhiChu,
                        MaPhieuDatCocKyLai = model.MaDatCocChoKyLai,
                        MaMauIn = model.MaMauIn,

                        DienTichTimTuong = model.DienTichTimTuong,
                        DienTichLotLong = model.DienTichLotLong,
                        DienTichSanVuon = model.DienTichSanVuon,
                        TyLeThueVat = model.TyLeThueVAT,

                        GiaCanHoTruocThue = model.GiaCanHoTruocThue,
                        GiaCanHoSauThue = model.GiaCanHoSauThue,
                        DonGiaDat = model.DonGiaDat,
                        GiaDat = model.GiaDat,
                        TyLeCk = model.TyLeCK,
                        GiaTriCk = model.GiaTriCK,
                        GiaBanTruocThue = model.GiaBanTruocThue,
                        GiaBanTienThue = model.GiaBanTienThue,
                        GiaBanSauThue = model.GiaBanSauThue,
                        TyLeQuyBaoTri = model.TyLeQuyBaoTri,
                        TienQuyBaoTri = model.TienQuyBaoTri,

                        NguoiLap = maNhanVienLap,
                        NgayLap = DateTime.Now
                    };

                    await context.BhPhieuDatCocs.AddAsync(record, ct);

                    // ===== Child: Tiến độ thanh toán =====
                    if (listTDTT?.Any() == true) // ✅ sửa & -> &&
                    {
                        var listCT = listTDTT
                            .Where(x => x != null)
                            .Select(x => new BhPhieuDatCocTienDoThanhToan
                            {
                                MaPhieuDc = record.MaPhieuDc!,
                                MaCstt = x.MaCSTT,
                                DotTt = x.DotTT,
                                NoiDungTt = x.NoiDungTT,
                                MaKyTt = x.MaKyTT,
                                DotThamChieu = x.DotThamChieu,
                                SoKhoangCachNgay = x.SoKhoangCachNgay,
                                TyLeTt = x.TyLeThanhToan,
                                TyLeTtvat = x.TyLeThanhToanVAT,
                                SoTienThanhToan = x.SoTien,
                                SoTienCanTruDaTt = x.SoTienCanTruDaTT,
                                SoTienChuyenDoiBooking = x.SoTienChuyenDoiBooking,
                                SoTienPhaiThanhToan = x.SoTienPhaiThanhToan,
                                NgayThanhToan = x.NgayThanhToan,
                            })
                            .ToList();

                        if (listCT.Count > 0)
                            await context.BhPhieuDatCocTienDoThanhToans.AddRangeAsync(listCT, ct);
                    }

                    // ===== Child: File đính kèm =====
                    if (model.Files?.Any() == true)
                    {
                        var listFiles = new List<HtFileDinhKem>();

                        foreach (var file in model.Files)
                        {
                            if (file == null) continue;
                            if (string.IsNullOrWhiteSpace(file.FileName)) continue;

                            // Save vật lý vào wwwroot/uploads, trả về relative path: uploads/xxx.ext
                            var relativePath = await SaveFileWithTickAsync(file);
                            if (string.IsNullOrWhiteSpace(relativePath)) continue;

                            savedRelativePaths.Add(relativePath);

                            listFiles.Add(new HtFileDinhKem
                            {
                                MaPhieu = record.MaPhieuDc ?? string.Empty,
                                TenFileDinhKem = file.FileName,
                                TenFileDinhKemLuu = relativePath,
                                TaiLieuUrl = relativePath,

                                Controller = "PhieuDatCoc",
                                AcTion = "Create",

                                NgayLap = DateTime.Now,
                                MaNhanVien = maNhanVienLap,
                                TenNhanVien = string.Empty,
                                FileSize = file.FileSize,
                                FileType = file.ContentType,
                                FullDomain = file.FullDomain,
                            });
                        }

                        if (listFiles.Count > 0)
                            await context.HtFileDinhKems.AddRangeAsync(listFiles, ct);
                    }

                    // ===== Save DB =====
                    await context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return ResultModel.SuccessWithId(record.MaPhieuDc, "Thêm phiếu đặt cọc thành công");
                });
            }
            catch (OperationCanceledException)
            {
                // Cleanup nếu đã lưu file mà bị cancel
                foreach (var p in savedRelativePaths)
                    DeleteUploadedFileIfExists(p, _env);

                return ResultModel.Fail("Thao tác đã bị huỷ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi thêm phiếu đặt cọc. MaPhieuDK={MaPhieuDK}, MaCanHo={MaCanHo}, MaDuAn={MaDuAn}",
                    model?.MaPhieuDK, model?.MaCanHo, model?.MaDuAn);

                // Cleanup file rác nếu DB fail/rollback
                foreach (var p in savedRelativePaths)
                {
                    try { DeleteUploadedFileIfExists(p, _env); }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Cleanup file failed: {Path}", p);
                    }
                }

                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu đặt cọc: {ex.Message}");
            }
        }

        //    public async Task<ResultModel> UpdateByIdAsync(PhieuDatCocModel model, List<PhieuDatCocTienDoThanhToanModel> listTDTT, string _env,
        //CancellationToken ct = default)
        //    {
        //        try
        //        {
        //            using var _context = _factory.CreateDbContext();
        //            var entity = await _context.BhPhieuDatCocs.FirstOrDefaultAsync(d => d.MaPhieuDc.ToLower() == model.MaPhieuDC.ToLower());
        //            if (entity == null)
        //            {
        //                return ResultModel.Fail("Không tìm thấy phiếu đặt cọc.");
        //            }
        //            entity.NgayKi = !string.IsNullOrEmpty(model.NgayKi) ? DateTime.ParseExact(model.NgayKi, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
        //            entity.SoPhieuDc = model.SoPhieuDC;
        //            entity.GhiChu = model.GhiChu;
        //            //Insert chính sách thanh toán
        //            var delTDTT = _context.BhPhieuDatCocTienDoThanhToans.Where(d => d.MaPhieuDc == entity.MaPhieuDc);
        //            if (listTDTT != null & listTDTT.Any() == true)
        //            {
        //                List<BhPhieuDatCocTienDoThanhToan> listCT = new List<BhPhieuDatCocTienDoThanhToan>();
        //                foreach (var item in listTDTT)
        //                {
        //                    var r = new BhPhieuDatCocTienDoThanhToan();
        //                    r.MaPhieuDc = entity.MaPhieuDc;
        //                    r.MaCstt = item.MaCSTT;
        //                    r.DotTt = item.DotTT;
        //                    r.NoiDungTt = item.NoiDungTT;
        //                    r.MaKyTt = item.MaKyTT;
        //                    r.DotThamChieu = item.DotThamChieu;
        //                    r.SoKhoangCachNgay = item.SoKhoangCachNgay;
        //                    r.TyLeTt = item.TyLeThanhToan;
        //                    r.TyLeTtvat = item.TyLeThanhToanVAT;
        //                    r.SoTienThanhToan = item.SoTien;
        //                    r.SoTienCanTruDaTt = item.SoTienCanTruDaTT;
        //                    r.SoTienChuyenDoiBooking = item.SoTienChuyenDoiBooking;
        //                    r.SoTienPhaiThanhToan = item.SoTienPhaiThanhToan;
        //                    listCT.Add(r);
        //                }
        //                await _context.BhPhieuDatCocTienDoThanhToans.AddRangeAsync(listCT);
        //            }
        //            _context.BhPhieuDatCocTienDoThanhToans.RemoveRange(delTDTT);
        //            List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
        //            var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == entity.MaPhieuDc && d.Controller == "PhieuDatCoc").ToListAsync();

        //            if (model.Files != null && model.Files.Any())
        //            {
        //                foreach (var file in model.Files)
        //                {
        //                    if (string.IsNullOrEmpty(file.FileName)) continue;

        //                    bool exists = UploadedFiles.Any(f =>
        //                        f.TenFileDinhKem == file.FileName &&
        //                        f.FileSize == file.FileSize
        //                    );
        //                    if (exists)
        //                        continue;

        //                    var savedPath = await SaveFileWithTickAsync(file);
        //                    var f = new HtFileDinhKem
        //                    {
        //                        MaPhieu = model.MaPhieuDC,
        //                        TenFileDinhKem = file.FileName,
        //                        TenFileDinhKemLuu = savedPath,
        //                        TaiLieuUrl = savedPath,
        //                        Controller = "PhieuDatCoc",
        //                        AcTion = "Edit",
        //                        NgayLap = DateTime.Now,
        //                        MaNhanVien = string.Empty,
        //                        TenNhanVien = string.Empty,
        //                        FileSize = file.FileSize,
        //                        FileType = file.ContentType,
        //                        FullDomain = file.FullDomain,
        //                    };
        //                    listFiles.Add(f);
        //                }
        //                await _context.HtFileDinhKems.AddRangeAsync(listFiles);
        //            }
        //            await _context.SaveChangesAsync();
        //            return ResultModel.SuccessWithId(entity.MaPhieuDc, "Cập nhật phiếu đặt cọc thành công");
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Lỗi khi cập nhật phiếu giữ chỗ");
        //            return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật phiếu giữ chỗ: {ex.Message.ToString()}");
        //        }
        //    }

        public async Task<ResultModel> UpdateByIdAsync(
    PhieuDatCocModel model,
    List<PhieuDatCocTienDoThanhToanModel>? listTDTT, string _env,
    CancellationToken ct = default)
        {
            if (model is null || string.IsNullOrWhiteSpace(model.MaPhieuDC))
                return ResultModel.Fail("Dữ liệu cập nhật không hợp lệ.");

            // Track file mới lưu để rollback nếu lỗi
            var savedRelativePaths = new List<string>();

            try
            {
                await using var context = _factory.CreateDbContext();
                var strategy = context.Database.CreateExecutionStrategy();

                return await strategy.ExecuteAsync(async () =>
                {
                    await using var tx = await context.Database.BeginTransactionAsync(ct);

                    var maPhieu = model.MaPhieuDC.Trim();

                    var entity = await context.BhPhieuDatCocs
                        .FirstOrDefaultAsync(d => d.MaPhieuDc == maPhieu, ct);

                    if (entity is null)
                        return ResultModel.Fail("Không tìm thấy phiếu đặt cọc.");

                    // Parse ngày ký an toàn
                    DateTime? ngayKi = null;
                    if (!string.IsNullOrWhiteSpace(model.NgayKi)
                        && DateTime.TryParseExact(model.NgayKi.Trim(), "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedNgayKi))
                    {
                        ngayKi = parsedNgayKi;
                    }

                    // ===== Update parent =====
                    entity.NgayKi = ngayKi;
                    entity.SoPhieuDc = model.SoPhieuDC;
                    entity.GhiChu = model.GhiChu;

                    // ===== Replace TDTT: Remove trước, Add sau =====
                    var oldTDTT = context.BhPhieuDatCocTienDoThanhToans
                        .Where(d => d.MaPhieuDc == entity.MaPhieuDc);

                    context.BhPhieuDatCocTienDoThanhToans.RemoveRange(oldTDTT);

                    if (listTDTT?.Any() == true)
                    {
                        var newTDTT = listTDTT
                            .Where(x => x != null)
                            .Select(x => new BhPhieuDatCocTienDoThanhToan
                            {
                                MaPhieuDc = entity.MaPhieuDc,
                                MaCstt = x.MaCSTT,
                                DotTt = x.DotTT,
                                NoiDungTt = x.NoiDungTT,
                                MaKyTt = x.MaKyTT,
                                DotThamChieu = x.DotThamChieu,
                                SoKhoangCachNgay = x.SoKhoangCachNgay,
                                TyLeTt = x.TyLeThanhToan,
                                TyLeTtvat = x.TyLeThanhToanVAT,
                                SoTienThanhToan = x.SoTien,
                                SoTienCanTruDaTt = x.SoTienCanTruDaTT,
                                SoTienChuyenDoiBooking = x.SoTienChuyenDoiBooking,
                                SoTienPhaiThanhToan = x.SoTienPhaiThanhToan,
                                NgayThanhToan = x.NgayThanhToan,
                            })
                            .ToList();

                        if (newTDTT.Count > 0)
                            await context.BhPhieuDatCocTienDoThanhToans.AddRangeAsync(newTDTT, ct);
                    }

                    // ===== Add file mới (không xóa file cũ) =====
                    var uploadedFiles = await context.HtFileDinhKems
                        .Where(d => d.MaPhieu == entity.MaPhieuDc && d.Controller == "PhieuDatCoc")
                        .ToListAsync(ct);

                    if (model.Files?.Any() == true)
                    {
                        var newFiles = new List<HtFileDinhKem>();

                        foreach (var file in model.Files)
                        {
                            if (file == null) continue;
                            if (string.IsNullOrWhiteSpace(file.FileName)) continue;

                            var exists = uploadedFiles.Any(f =>
                                string.Equals(f.TenFileDinhKem, file.FileName, StringComparison.OrdinalIgnoreCase)
                                && f.FileSize == file.FileSize);

                            if (exists) continue;

                            var savedPath = await SaveFileWithTickAsync(file);
                            if (string.IsNullOrWhiteSpace(savedPath)) continue;

                            // ✅ track để rollback khi lỗi
                            savedRelativePaths.Add(savedPath);

                            newFiles.Add(new HtFileDinhKem
                            {
                                MaPhieu = entity.MaPhieuDc ?? string.Empty,
                                TenFileDinhKem = file.FileName,
                                TenFileDinhKemLuu = savedPath,
                                TaiLieuUrl = savedPath,

                                Controller = "PhieuDatCoc",
                                AcTion = "Edit",

                                NgayLap = DateTime.Now,
                                MaNhanVien = string.Empty,
                                TenNhanVien = string.Empty,

                                FileSize = file.FileSize,
                                FileType = file.ContentType,
                                FullDomain = file.FullDomain,
                            });
                        }

                        if (newFiles.Count > 0)
                            await context.HtFileDinhKems.AddRangeAsync(newFiles, ct);
                    }

                    await context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    return ResultModel.SuccessWithId(entity.MaPhieuDc, "Cập nhật phiếu đặt cọc thành công");
                });
            }
            catch (OperationCanceledException)
            {
                // ✅ Cleanup file mới upload nếu bị cancel
                foreach (var p in savedRelativePaths)
                {
                    try { DeleteUploadedFileIfExists(p, _env); }
                    catch { /* ignore cleanup errors */ }
                }

                return ResultModel.Fail("Thao tác đã bị huỷ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi cập nhật phiếu đặt cọc. MaPhieuDC={MaPhieuDC}, MaCanHo={MaCanHo}, MaDuAn={MaDuAn}",
                    model?.MaPhieuDC, model?.MaCanHo, model?.MaDuAn);

                // ✅ Cleanup file mới upload nếu DB fail/rollback
                foreach (var p in savedRelativePaths)
                {
                    try { DeleteUploadedFileIfExists(p, _env); }
                    catch (Exception cleanupEx)
                    {
                        _logger.LogWarning(cleanupEx, "Cleanup file failed: {Path}", p);
                    }
                }

                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật phiếu đặt cọc: {ex.Message}");
            }
        }

        private void DeleteUploadedFileIfExists(string relativePath, string _env)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            // relativePath: uploads/abc.pdf
            var normalized = relativePath
                .Replace("/", Path.DirectorySeparatorChar.ToString())
                .TrimStart(Path.DirectorySeparatorChar);

            var physicalPath = Path.GetFullPath(Path.Combine(_env, normalized));
            var uploadsRoot = Path.GetFullPath(Path.Combine(_env, "uploads"));

            // chặn xóa ra ngoài uploads
            if (!physicalPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
                return;

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);
        }

        public async Task<ResultModel> DeletePDCAsync(string maPhieu, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var pdc = await _context.BhPhieuDatCocs.Where(d => d.MaPhieuDc == maPhieu).FirstOrDefaultAsync();
                if (pdc == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu đặt cọc");
                }
                var delPTTT = _context.BhPhieuDatCocTienDoThanhToans.Where(d => d.MaPhieuDc == maPhieu);
                _context.BhPhieuDatCocs.Remove(pdc);
                _context.BhPhieuDatCocTienDoThanhToans.RemoveRange(delPTTT);
                var listFiles = _context.HtFileDinhKems.Where(d => d.Controller == "PhieuDatCoc" && d.MaPhieu == pdc.MaPhieuDc);
                if (listFiles != null && listFiles.Any())
                {
                    foreach (var file in listFiles)
                    {
                        var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }

                    _context.HtFileDinhKems.RemoveRange(listFiles);
                }
                var delND = _context.HtDmnguoiDuyets.Where(d => d.MaPhieu == maPhieu);
                _context.HtDmnguoiDuyets.RemoveRange(delND);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {pdc.MaPhieuDc} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePGCAsync] Lỗi khi xóa phiếu đặt cọc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteListAsync(List<PhieuDatCocPaginDto> listPDC, string webRootPath)
        {
            try
            {
                var ids = listPDC?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieuDC))
                    .Select(x => x!.MaPhieuDC.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B1: Lấy trước danh sách file cần xóa (vật lý) ---
                var filePaths = await _context.HtFileDinhKems
                    .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "PhieuDatCoc")
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.BhPhieuDatCocTienDoThanhToans
                    .Where(d => ids.Contains(d.MaPhieuDc))
                    .ExecuteDeleteAsync();

                var c2 = await _context.HtFileDinhKems
                    .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "PhieuDatCoc")
                    .ExecuteDeleteAsync();

                var c3 = await _context.HtDmnguoiDuyets
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var cParent = await _context.BhPhieuDatCocs
                    .Where(k => ids.Contains(k.MaPhieuDc))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                // --- B3: Xóa file vật lý ngoài transaction ---
                int cFile = 0;
                foreach (var relPath in filePaths.Distinct().Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    try
                    {
                        // chuẩn hóa path
                        var clean = relPath!.Trim().TrimStart('/', '\\');
                        var fullPath = Path.Combine(webRootPath, clean);

                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            cFile++;
                        }
                    }
                    catch (Exception exDel)
                    {
                        _logger.LogWarning(exDel, "[DeleteListAsync] Không xóa được file: {RelPath}", relPath);
                        // không throw – tránh rollback DB sau khi đã commit
                    }
                }

                return ResultModel.Success(
                    $"Đã xóa {cParent} phiếu, {c1} tiến độ, {c2} file đính kèm (DB), {c3} người duyệt. " +
                    $"File vật lý xóa: {cFile}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách phiếu đặt cọc");
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
                    return ResultModel.Fail("Không tìm thấy phiếu giữ chỗ");
                }
                pgc.IsxacNhan = true;
                _context.SaveChanges();
                return ResultModel.Success($"Xác nhận {pgc.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[XacNhanPhieuGiuCho] Lỗi khi xác nhận phiếu giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin phiếu đặt cọc
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await ThongTinChungPDC(string.Empty, id, string.Empty, string.Empty);
                if (string.IsNullOrEmpty(id))
                {
                    entity = new PhieuDatCocModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaPhieuDC = await SinhMaPhieuDCTuDongAsync("PDC-", _context, 5);
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("PhieuDatCoc", entity.MaPhieuDC);
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
                    var files = await _context.HtFileDinhKems.Where(d => d.Controller == "PhieuDatCoc" && d.MaPhieu == id).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType
                    }).ToListAsync();
                    entity.Files = files;
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu đặc cọc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<PhieuDatCocModel> ThongTinChungPDC(string maPhieuDangKy, string maPhieuDatCoc, string maDuAn, string maChinhSachTT)
        {
            var entity = new PhieuDatCocModel();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaPhieuDangKy", maPhieuDangKy);
                param.Add("@MaPhieuDatCoc", maPhieuDatCoc);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaChinhSacTT", maChinhSachTT);
                entity = (await connection.QueryAsync<PhieuDatCocModel>(
                "Proc_PhieuDatCoc_ThongTinChung",
                param,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            catch
            {
                entity = new PhieuDatCocModel();
            }
            return entity;
        }

        public async Task<KhachHangPhieuDatCoc> ThongTinKhachHang(string maPhieuDangKy, string maPhieuDatCoc, string maDuAn, string maChinhSachTT)
        {
            var khachHang = new KhachHangPhieuDatCoc();
            var entity = new PhieuDatCocModel();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaPhieuDangKy", maPhieuDangKy);
                param.Add("@MaPhieuDatCoc", maPhieuDatCoc);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaChinhSacTT", maChinhSachTT);
                entity = (await connection.QueryAsync<PhieuDatCocModel>(
                "Proc_PhieuDatCoc_ThongTinChung",
                param,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
                if (entity != null)
                {
                    khachHang.MaKhachHang = entity.MaKhachHang;
                    khachHang.TenKhachHang = entity.TenKhachHang;
                    khachHang.MaDoiTuongKH = entity.MaDoiTuongKH;
                    khachHang.NguoiDaiDien = entity.NguoiDaiDien;
                    khachHang.ChucVuNguoiDaiDien = entity.ChucVuNguoiDaiDien;
                    khachHang.NguoiLienHe = entity.NguoiLienHe;
                    khachHang.SoDienThoaiNguoiLienHe = entity.SoDienThoaiNguoiLienHe;
                    khachHang.NgaySinh = entity.NgaySinh == null ? string.Empty : string.Format("{0:dd/MM/yyyy}", entity.NgaySinh);
                    khachHang.SoDienThoai = entity.SoDienThoai;
                    khachHang.IdCard = entity.IDCard;
                    khachHang.NgayCapIdCard = entity.NgayCap == null ? string.Empty : string.Format("{0:dd/MM/yyyy}", entity.NgayCap);
                    khachHang.NoiCapIdCard = entity.NoiCap;
                    khachHang.DiaChiThuongTru = entity.DiaChiThuongTru;
                    khachHang.DiaChiHienNay = entity.DiaChiHienNay;
                    khachHang.MaDotMoBan = entity.DotMoBan;
                    khachHang.TenDotMoBan = entity.TenDotMoBan;
                    khachHang.MaCanHo = entity.MaCanHo;
                    khachHang.TenCanHo = entity.TenCanHo;
                    khachHang.MaChinhSachTT = entity.MaChinhSachTT;
                    khachHang.TenCSTT = entity.TenChinhSachTT;
                    khachHang.IDKhachHangCT = entity.IDKhachHangCT;

                    khachHang.DienTichTimTuong = entity.DienTichTimTuong;
                    khachHang.DienTichSanVuon = entity.DienTichSanVuon;
                    khachHang.DienTichLotLong = entity.DienTichLotLong;
                    khachHang.GiaDat = entity.GiaDat;
                    khachHang.DonGiaDat = entity.DonGiaDat;
                    khachHang.GiaCanHoTruocThue = entity.GiaCanHoTruocThue;
                    khachHang.TyLeThueVAT = entity.TyLeThueVAT;
                    khachHang.GiaCanHoSauThue = entity.GiaCanHoSauThue;
                    khachHang.TyLeCK = entity.TyLeCK;
                    khachHang.GiaTriCK = entity.GiaTriCK;
                    khachHang.GiaBanTruocThue = entity.GiaBanTruocThue;
                    khachHang.GiaBanTienThue = entity.GiaBanTienThue;
                    khachHang.GiaBanSauThue = entity.GiaBanSauThue;
                    khachHang.TyLeQuyBaoTri = entity.TyLeQuyBaoTri;
                    khachHang.TienQuyBaoTri = entity.TienQuyBaoTri;

                }
            }
            catch
            {
                khachHang = new KhachHangPhieuDatCoc();
            }
            return khachHang;
        }

        public async Task<List<PhieuDatCocTienDoThanhToanModel>> GetByTienDoThanhToanAsync(string maCSTT, string maPhieuDC, decimal giaBanTruocThue, decimal giaBanTienThue, string maPhieuDK)
        {
            var entity = new List<PhieuDatCocTienDoThanhToanModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaChinhSachTT", maCSTT);
                param.Add("@MaPhieuDC", maPhieuDC);
                param.Add("@GiaBanTruocThue", giaBanTruocThue);
                param.Add("@GiaBanTienThue", giaBanTienThue);
                param.Add("@MaPhieuDangKy", maPhieuDK);
                entity = (await connection.QueryAsync<PhieuDatCocTienDoThanhToanModel>(
                    "Proc_PhieuDatCoc_TienDoThanhToan",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<PhieuDatCocTienDoThanhToanModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách tiến độ thanh toán phiếu đặt cọc");
            }
            return entity;
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu giữ chỗ   
        public async Task<string> SinhMaPhieuDCTuDongAsync(string prefix, AppDbContext _context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.BhPhieuDatCocs
                .Where(kh => kh.MaPhieuDc.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaPhieuDc)
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

        public async Task<string> SinhTuDonSoPhieuAsync(string maDuAn, string maDot, AppDbContext _context)
        {
            try
            {
                int coutDot = (await _context.BhPhieuGiuChos.Where(d => d.MaDuAn == maDuAn && d.DotMoBan == maDot).CountAsync()) + 1;
                string maMoi = maDuAn + "-" + maDot + "-" + coutDot.ToString();
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
            using var _context = _factory.CreateDbContext();
            var entity = new List<DaDanhMucDuAn>();
            try
            {
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
        public async Task<List<PhieuDangKyDatCocCSTTModel>> GetChinhSachThanhToanAsync(string maDuAn, string maPhieuDK)
        {
            var entity = new List<PhieuDangKyDatCocCSTTModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaPhieu", maPhieuDK);

                entity = (await connection.QueryAsync<PhieuDangKyDatCocCSTTModel>(
                    "Proc_PhieuDatCoc_ChinhSachThanhToan",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<PhieuDangKyDatCocCSTTModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách chính sách thanh toán");
            }
            return entity;
        }

        public async Task<List<PhieuDangKyDatCocModel>> GetByPhieuDangKyAsync(string maDuAn)
        {
            var entity = new List<PhieuDangKyDatCocModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);

                entity = (await connection.QueryAsync<PhieuDangKyDatCocModel>(
                    "Proc_PhieuDatCoc_ChonPhieuDangKy",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<PhieuDangKyDatCocModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách phiếu đăn ký chưa lên phiếu đặt cọc");
            }
            return entity;
        }
        public async Task<List<HtMauIn>> GetByMauInAsync(string maDuAn)
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<HtMauIn>();
            try
            {
                entity = await _context.HtMauIns.Where(d => d.MaDuAn == maDuAn && d.LoaiMauIn == "DC").ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtMauIn>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách mẫu in");
            }
            return entity;
        }

        public async Task<PhieuDangKyDatCocModel> GetByPhieuDangKyDCAsync(string maPhieuDK)
        {
            var entity2 = new PhieuDangKyDatCocModel();
            try
            {
                using var _context = _factory.CreateDbContext();
                string maDuAn = await _context.BhPhieuDangKiChonCans.Where(d => d.MaPhieu == maPhieuDK).Select(d => d.MaDuAn).FirstOrDefaultAsync() ?? string.Empty;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);

                var entity = (await connection.QueryAsync<PhieuDangKyDatCocModel>(
                       "Proc_PhieuDatCoc_ChonPhieuDangKy",
                       param,
                       commandType: CommandType.StoredProcedure
                   )).ToList();
                entity2 = entity.Where(d => d.MaPhieuDangKy == maPhieuDK).FirstOrDefault();
                if (entity2 == null)
                {
                    entity2 = new PhieuDangKyDatCocModel();
                }
                entity2.MaDuAn = maDuAn;
                entity2.TenDuAn = await _context.DaDanhMucDuAns.Where(d => d.MaDuAn == maDuAn).Select(d => d.TenDuAn).FirstOrDefaultAsync() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách phiếu đăn ký chưa lên phiếu đặt cọc");
            }
            return entity2;
        }

        /// <summary>
        /// Lấy MaCSTT mặc định được chọn (IsChon = 1, LoaiCS = 'CSTT') từ bảng BH_PhieuDangKiChonCan_CSBH
        /// theo mã phiếu đăng ký.
        /// </summary>
        public async Task<string> GetCSTTMacDinhTuPhieuDKAsync(string maPhieuDK)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maPhieuDK))
                { 
                    return string.Empty; 
                }

                using var _context = _factory.CreateDbContext();
                var maCSTT = await _context.BhPhieuDangKiChonCanCsbhs
                    .AsNoTracking()
                    .Where(x => x.MaPhieuDangKy == maPhieuDK
                             && x.LoaiCs == "CSTT"
                             && x.IsChon == true)
                    .Select(x => x.MaCsbh)
                    .FirstOrDefaultAsync();

                return maCSTT ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetCSTTMacDinhTuPhieuDKAsync] Lỗi khi lấy CSTT mặc định từ phiếu đăng ký {MaPhieuDK}", maPhieuDK);
                return string.Empty;
            }
        }
        #endregion

        #region Thông tin mẫu in phiếu đặt cọc
        public async Task<ResultModel> GetThongTinInPDCAsync(string id)
        {
            try
            {
                var entity = await ThongTinInPDC(id);
                var thongTin = new ThongTinDangKyNguyenVong
                {
                    MaPhieu = id,
                    TenCongTy = entity.TenCongTy,
                    TenKhachHang = entity.TenKhachHang,
                    TenNguoiDaiDien = entity.TenNguoiDaiDien,
                    NgaySinh = string.Format("{0:dd/MM/yyyy}", entity.NgaySinh),
                    QT = entity.QT,
                    SoDienThoai = entity.SoDienThoai,
                    Email = entity.Email,
                    SoCMND = entity.IdCard,
                    NgayCap = string.Format("{0:dd/MM/yyyy}", entity.NgayCapIdCard),
                    NoiCap = entity.NoiCapIdCard,
                    DiaChiThuongTru = entity.DiaChiThuongTru,
                    DiaChiHienTai = entity.DiaChiLienLac,
                    TenDuAn = entity.TenDuAn,
                    DiaChiCongTy = entity.DiaChiCongTy,
                    MaCanHo = entity.MaCanHo,
                    TangBlock = entity.Tang + " " + entity.Block,
                    Tang = entity.Tang,
                    Block = entity.Block,
                    DienTichThongThuy = string.Format("{0:N2}", entity.DienTichLotLong),
                    DienTichTimTuong = string.Format("{0:N2}", entity.DienTichTimTuong),
                    GiaBanCanHo = string.Format("{0:N2}", entity.GiaBanCanHo),
                    GiaBanCanHoBangChu = string.Empty,
                    PhiBaoTri = "0",
                    NgayHienThai = DateTime.Now.Day.ToString(),
                    ThangHienTai = DateTime.Now.Month.ToString(),
                    NamHienTai = DateTime.Now.Year.ToString(),
                    DanhSachChinhSach = await GetByChinhSachThanhToanAsync(id),
                    MaMauIn = entity.MaMauIn,
                };
                var template = new TemplatePhieuDatCoc
                {
                    MaPhieu = id,
                    ThongTin = thongTin
                };
                return ResultModel.SuccessWithData(template, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetThongTinInPDCAsync] Lỗi khi lấy thông tin in phiếu đặc cọc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<PhieuDatCocInDto> ThongTinInPDC(string maPhieu)
        {
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaPhieuDC", maPhieu);

            var entity = (await connection.QueryAsync<PhieuDatCocInDto>(
                  "Proc_PhieuDatCoc_InThongTinChung",
                  param,
                  commandType: CommandType.StoredProcedure
              )).FirstOrDefault();

            if (entity == null)
            {
                entity = new PhieuDatCocInDto();
            }
            return entity;
        }

        public async Task<List<ChinhSachModel>> GetByChinhSachThanhToanAsync(string maPhieuDC)
        {
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaPhieuDC", maPhieuDC);

            var entity = (await connection.QueryAsync<ChinhSachModel>(
                  "Proc_PhieuDatCoc_InCSTT",
                  param,
                  commandType: CommandType.StoredProcedure
              )).ToList();

            if (entity == null)
            {
                entity = new List<ChinhSachModel>();
            }
            return entity;
        }
        #endregion

        #region In phiếu đặt cọc 
        public async Task<(string FileName, byte[] ZipBytes)> GenerateAllDocsZipAsync(string maPhieu)
        {
            using var _context = _factory.CreateDbContext();
            // 1) Dữ liệu in
            var e = await ThongTinInPDC(maPhieu) ?? throw new InvalidOperationException($"Không có dữ liệu {maPhieu}");

            var data = new Dictionary<string, string>
            {
                ["MaPhieu"] = e.MaPhieu ?? maPhieu,
                ["TenCongTy"] = e.TenCongTy,
                ["TenKhachHang"] = e.TenKhachHang,
                ["TenNguoiDaiDien"] = e.TenNguoiDaiDien,
                ["NgaySinh"] = string.Format("{0:dd/MM/yyyy}", e.NgaySinh),
                ["QT"] = e.QT,
                ["SoDienThoai"] = e.SoDienThoai,
                ["Email"] = e.Email,
                ["SoCMND"] = e.IdCard,
                ["NgayCap"] = string.Format("{0:dd/MM/yyyy}", e.NgayCapIdCard),
                ["NoiCap"] = e.NoiCapIdCard,
                ["DiaChiThuongTru"] = e.DiaChiThuongTru,
                ["DiaChiHienTai"] = e.DiaChiLienLac,
                ["TenDuAn"] = e.TenDuAn,
                ["DiaChiCongTy"] = e.DiaChiCongTy,
                ["MaCanHo"] = e.MaCanHo,
                ["TangBlock"] = $"{e.Tang}.{e.Block}",
                ["Tang"] = e.Tang,
                ["Block"] = e.Block,
                ["DienTichThongThuy"] = string.Format("{0:N2}", e.DienTichLotLong),
                ["DienTichTimTuong"] = string.Format("{0:N2}", e.DienTichTimTuong),
                ["GiaBanCanHo"] = string.Format("{0:N0}", e.GiaBanCanHo),
                ["GiaBanCanHoBangChu"] = FormatHelper.DocSoTienBangChu(e.GiaBanCanHo),
                ["PhiBaoTri"] = "0",
                ["NgayHienTai"] = DateTime.Now.Day.ToString("00"),
                ["ThangHienTai"] = DateTime.Now.Month.ToString("00"),
                ["NamHienTai"] = DateTime.Now.Year.ToString()
            };

            // 2) KM
            var promos = await GetByChinhSachThanhToanAsync(maPhieu);

            // 3) Lấy danh sách file từ DB
            // LƯU Ý: nếu cột lọc đúng là MaPhieu thì dùng maPhieu; nếu là "mã mẫu in" thì e.MaMauIn. Bạn kiểm tra lại.
            var files = await _context.HtFileDinhKems
                .Where(f => f.MaPhieu == e.MaMauIn && (f.TenFileDinhKemLuu ?? f.TaiLieuUrl).EndsWith(".docx"))
                .Select(f => f.TenFileDinhKemLuu ?? f.TaiLieuUrl)
                .ToListAsync();

            var docxPaths = files
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(NormalizePhysicalPath)
                .Where(File.Exists)
                .ToList();

            if (docxPaths.Count == 0)
                throw new FileNotFoundException("Không tìm thấy file .docx nào cho mã phiếu trong HT_FileDinhKem");

            // 4) Nén ZIP + ghi errors.txt nếu có file lỗi
            using var zipMs = new MemoryStream();
            using (var zip = new ZipArchive(zipMs, ZipArchiveMode.Create, true))
            {
                StreamWriter? errorWriter = null;

                foreach (var path in docxPaths)
                {
                    var tplBytes = await File.ReadAllBytesAsync(path);

                    // lọc nhanh .docx hợp lệ
                    if (!IsValidDocx(tplBytes))
                    {
                        errorWriter ??= new StreamWriter(zip.CreateEntry("errors.txt").Open());
                        errorWriter.WriteLine($"{Path.GetFileName(path)} -> Không phải .docx hợp lệ (ZIP ‘PK’/thiếu document.xml).");
                        continue;
                    }

                    var docBytes = TryGenerateFromTemplate(
                        tplBytes, data,
                        doc => FillPromotionTable(doc, promos),
                        out var err
                    );

                    if (docBytes == null)
                    {
                        errorWriter ??= new StreamWriter(zip.CreateEntry("errors.txt").Open());
                        errorWriter.WriteLine($"{Path.GetFileName(path)} -> {err}");
                        continue;
                    }

                    var entryName = $"{Path.GetFileNameWithoutExtension(path)}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                    using var es = zip.CreateEntry(entryName, CompressionLevel.Fastest).Open();
                    await es.WriteAsync(docBytes, 0, docBytes.Length);
                }

                if (errorWriter != null)
                    errorWriter.Dispose(); // flush errors.txt
            }

            var outName = $"Phieu_Dat_Coc_{maPhieu}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            return (outName, zipMs.ToArray());
        }

        public async Task<(string FileName, byte[] ZipBytes)> GenerateAllDocsZipProgressAsync(
    string maPhieu,
    IProgress<ExportProgress>? progress = null,
    CancellationToken ct = default)
        {
            using var _context = _factory.CreateDbContext();
            // 1) Dữ liệu in
            var e = await ThongTinInPDC(maPhieu) ?? throw new InvalidOperationException($"Không có dữ liệu {maPhieu}");

            var data = new Dictionary<string, string>
            {
                ["MaPhieu"] = e.MaPhieu ?? maPhieu,
                ["TenCongTy"] = e.TenCongTy,
                ["TenKhachHang"] = e.TenKhachHang,
                ["TenNguoiDaiDien"] = e.TenNguoiDaiDien,
                ["NgaySinh"] = string.Format("{0:dd/MM/yyyy}", e.NgaySinh),
                ["QT"] = e.QT,
                ["SoDienThoai"] = e.SoDienThoai,
                ["Email"] = e.Email,
                ["SoCMND"] = e.IdCard,
                ["NgayCap"] = string.Format("{0:dd/MM/yyyy}", e.NgayCapIdCard),
                ["NoiCap"] = e.NoiCapIdCard,
                ["DiaChiThuongTru"] = e.DiaChiThuongTru,
                ["DiaChiHienTai"] = e.DiaChiLienLac,
                ["TenDuAn"] = e.TenDuAn,
                ["DiaChiCongTy"] = e.DiaChiCongTy,
                ["MaCanHo"] = e.MaCanHo,
                ["TangBlock"] = $"{e.Tang}.{e.Block}",
                ["Tang"] = e.Tang,
                ["Block"] = e.Block,
                ["DienTichThongThuy"] = string.Format("{0:N2}", e.DienTichLotLong),
                ["DienTichTimTuong"] = string.Format("{0:N2}", e.DienTichTimTuong),
                ["GiaBanCanHo"] = string.Format("{0:N0}", e.GiaBanCanHo),
                ["GiaBanCanHoBangChu"] = ToTienBangChu(e.GiaBanCanHo),
                ["PhiBaoTri"] = "0",
                ["NgayHienTai"] = DateTime.Now.Day.ToString("00"),
                ["ThangHienTai"] = DateTime.Now.Month.ToString("00"),
                ["NamHienTai"] = DateTime.Now.Year.ToString()
            };

            // 2) KM
            var promos = await GetByChinhSachThanhToanAsync(maPhieu);

            // 3) Lấy danh sách file từ DB
            var files = await _context.HtFileDinhKems
                .Where(f => f.MaPhieu == e.MaMauIn && (f.TenFileDinhKemLuu ?? f.TaiLieuUrl).EndsWith(".docx"))
                .Select(f => f.TenFileDinhKemLuu ?? f.TaiLieuUrl)
                .ToListAsync(ct);

            var docxPaths = files
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(NormalizePhysicalPath)
                .Where(File.Exists)
                .ToList();

            if (docxPaths.Count == 0)
                throw new FileNotFoundException("Không tìm thấy file .docx nào cho mã phiếu trong HT_FileDinhKem");

            int total = docxPaths.Count;
            int done = 0;

            // 4) Nén ZIP + ghi errors.txt nếu có file lỗi
            using var zipMs = new MemoryStream();
            using (var zip = new ZipArchive(zipMs, ZipArchiveMode.Create, true))
            {
                StreamWriter? errorWriter = null;

                foreach (var path in docxPaths)
                {
                    ct.ThrowIfCancellationRequested();

                    var shortName = Path.GetFileName(path);
                    progress?.Report(new ExportProgress(
                        (int)(done * 100.0 / Math.Max(total, 1)),
                        $"Đang xử lý: {shortName}",
                        done + 1,
                        total,
                        shortName
                    ));

                    var tplBytes = await File.ReadAllBytesAsync(path, ct);

                    // lọc nhanh .docx hợp lệ
                    if (!IsValidDocx(tplBytes))
                    {
                        errorWriter ??= new StreamWriter(zip.CreateEntry("errors.txt").Open());
                        errorWriter.WriteLine($"{shortName} -> Không phải .docx hợp lệ (ZIP ‘PK’/thiếu document.xml).");
                        done++;
                        progress?.Report(new ExportProgress(
                            (int)(done * 100.0 / Math.Max(total, 1)),
                            $"Bỏ qua (file lỗi): {shortName}",
                            done,
                            total,
                            shortName
                        ));
                        continue;
                    }

                    var docBytes = TryGenerateFromTemplate(
                        tplBytes, data,
                        doc => FillPromotionTable(doc, promos),
                        out var err
                    );

                    if (docBytes == null)
                    {
                        errorWriter ??= new StreamWriter(zip.CreateEntry("errors.txt").Open());
                        errorWriter.WriteLine($"{shortName} -> {err}");
                    }
                    else
                    {
                        var entryName = $"{Path.GetFileNameWithoutExtension(path)}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                        using var es = zip.CreateEntry(entryName, CompressionLevel.Fastest).Open();
                        await es.WriteAsync(docBytes, 0, docBytes.Length, ct);
                    }

                    done++;
                    progress?.Report(new ExportProgress(
                       (int)(done * 100.0 / Math.Max(total, 1)),
                       $"Đã xong: {shortName}",
                        done,
                        total,
                        shortName
                    ));
                }

                if (errorWriter != null)
                    errorWriter.Dispose(); // flush errors.txt
            }

            var outName = $"Phieu_Dat_Coc_{maPhieu}_{DateTime.Now:yyyyMMdd_HHmmss}.zip";
            progress?.Report(new ExportProgress(100, "Đóng gói ZIP xong.", total, total));
            return (outName, zipMs.ToArray());
        }


        private static bool IsValidDocx(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 4) return false;
            // header ZIP 'PK'
            if (!(bytes[0] == 0x50 && bytes[1] == 0x4B)) return false;

            try
            {
                using var ms = new MemoryStream(bytes, writable: false);
                using var zip = new ZipArchive(ms, ZipArchiveMode.Read, leaveOpen: true);
                return zip.GetEntry("word/document.xml") != null && zip.GetEntry("[Content_Types].xml") != null;
            }
            catch { return false; }
        }

        private static byte[]? TryGenerateFromTemplate(
            byte[] templateBytes,
            IDictionary<string, string> data,
            Action<DocX>? afterReplace,
            out string error)
        {
            error = "";
            try
            {
                using var tpl = new MemoryStream(templateBytes, writable: false);
                if (tpl.CanSeek) tpl.Position = 0;

                using var doc = DocX.Load(tpl);

                foreach (var kv in data)
                    doc.ReplaceText($"<<{kv.Key}>>", kv.Value ?? string.Empty, false, RegexOptions.None);

                afterReplace?.Invoke(doc);

                using var outMs = new MemoryStream();
                doc.SaveAs(outMs);
                return outMs.ToArray();
            }
            catch (Exception ex)
            {
                error = $"{ex.GetType().Name}: {ex.Message}" +
                        (ex.InnerException != null ? $" | {ex.InnerException.Message}" : "");
                return null;
            }
        }


        // --- Helpers ---

        /// Map đường dẫn trong DB -> đường dẫn vật lý
        private static string NormalizePhysicalPath(string dbPath)
        {
            // Các case thường gặp: "uploads/...", "/uploads/...", "~/uploads/..."
            var p = dbPath.Replace('\\', '/').Trim();

            // Nếu là đường dẫn tuyệt đối (C:\... hoặc //server/...), trả nguyên
            if (Path.IsPathRooted(p)) return p;

            // Bỏ prefix ~/
            if (p.StartsWith("~/")) p = p[2..];
            if (p.StartsWith("/")) p = p[1..];

            // Ghép với wwwroot
            var webroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            return Path.Combine(webroot, p.Replace('/', Path.DirectorySeparatorChar));
        }

        private static byte[] GenerateFromTemplate(
            byte[] templateBytes,
            IDictionary<string, string> data,
            Action<DocX>? afterReplace = null)
        {
            using var tpl = new MemoryStream(templateBytes);
            using var doc = DocX.Load(tpl);

            foreach (var kv in data)
            {
                var ph = $"<<{kv.Key}>>";
                doc.ReplaceText(ph, kv.Value ?? string.Empty, false, RegexOptions.None);
            }

            afterReplace?.Invoke(doc);

            using var outMs = new MemoryStream();
            doc.SaveAs(outMs);
            return outMs.ToArray();
        }

        private static void FillPromotionTable(DocX doc, IList<ChinhSachModel> items)
        {
            if (items == null || items.Count == 0) return;

            // 1) Tìm bảng có header: STT | Nội dung | Chính sách lựa chọn
            var table = doc.Tables.FirstOrDefault(t =>
                t.Rows.Any(r =>
                {
                    var line = string.Join("|", r.Cells.SelectMany(c => c.Paragraphs).Select(p => p.Text))
                                    .ToLowerInvariant();
                    return line.Contains("stt") && line.Contains("nội dung")
                           && (line.Contains("chính sách") || line.Contains("lua chon") || line.Contains("lựa chọn"));
                }));

            if (table == null) return;

            // 2) Xác định hàng header
            int headerIndex = 0;
            for (int i = 0; i < table.Rows.Count; i++)
            {
                var line = string.Join("|", table.Rows[i].Cells.SelectMany(c => c.Paragraphs).Select(p => p.Text))
                                .ToLowerInvariant();
                if (line.Contains("stt") && line.Contains("nội dung")
                    && (line.Contains("chính sách") || line.Contains("lua chon") || line.Contains("lựa chọn")))
                {
                    headerIndex = i;
                    break;
                }
            }

            // 3) Chèn từng dòng ngay dưới header (clone header để giữ style)
            int insertIndex = headerIndex;
            int stt = 1;

            foreach (var it in items)
            {
                var src = table.Rows[headerIndex];                // hàng mẫu = header
                var row = table.InsertRow(src, insertIndex + 1);  // chèn ngay dưới

                if (row.Cells.Count < 3)
                    throw new InvalidOperationException("Header không đủ 3 cột.");

                SetCellText(row.Cells[0], stt.ToString());                 // STT
                SetCellText(row.Cells[1], it.NoiDung ?? string.Empty);     // Nội dung
                SetCellText(row.Cells[2], it.DuocChon ? "☑" : "☐");        // Chính sách lựa chọn

                // Căn lề (tuỳ chọn)
                row.Cells[0].Paragraphs[0].Alignment = DocxAlignment.center;
                row.Cells[1].Paragraphs[0].Alignment = DocxAlignment.left;
                row.Cells[2].Paragraphs[0].Alignment = DocxAlignment.center;

                insertIndex++;
                stt++;
            }
        }

        // Helper: ghi text an toàn vào một ô, giữ style sẵn có
        private static void SetCellText(DocxCell cell, string text)
        {
            var p = cell.Paragraphs.FirstOrDefault() ?? cell.InsertParagraph();
            try { p.RemoveText(0); } catch { /* không có text cũ */ }
            p.Append(text);
        }

        private static string ToTienBangChu(decimal? soTien)
        {
            // TODO: Việt hoá nếu cần
            return soTien?.ToString("0,0.##") ?? "";
        }
        #endregion

        #region Create file
        private async Task<string> SaveFileWithTickAsync(UploadedFileModel file)
        {
            if (string.IsNullOrEmpty(file.FileName)) return "";

            var absolutePath = Path.Combine(file.FolderUrl, file.FileNameSave);

            // 5. Trả về tên file lưu (để lưu DB)
            return absolutePath.Replace("\\", "/"); // ex: uploads/abc_637xxxx.pdf
        }
        #endregion

        #region Xác nhận ngày ký khi đã duyệt phiếu
        //public async Task<ResultModel> XacNhanNgayKyAsync(PhieuDatCocModel model)
        //{

        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var entity = await _context.BhPhieuDatCocs.FirstOrDefaultAsync(d => d.MaPhieuDc.ToLower() == model.MaPhieuDC.ToLower());
        //        if (entity == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy phiếu đặt cọc.");
        //        }
        //        var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
        //        entity.NgayKi = !string.IsNullOrEmpty(model.NgayKi) ? DateTime.ParseExact(model.NgayKi, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
        //        entity.IsDaKy = model.IsDaKy;
        //        entity.NgayXacNhan = DateTime.Now;
        //        entity.NguoiXacNhan = NguoiLap.MaNhanVien;
        //        List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
        //        var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == entity.MaPhieuDc && d.Controller == "PhieuDatCoc").ToListAsync();

        //        if (model.Files != null && model.Files.Any())
        //        {
        //            foreach (var file in model.Files)
        //            {
        //                if (string.IsNullOrEmpty(file.FileName)) continue;

        //                bool exists = UploadedFiles.Any(f =>
        //                    f.TenFileDinhKem == file.FileName &&
        //                    f.FileSize == file.FileSize
        //                );
        //                if (exists)
        //                    continue;

        //                var savedPath = await SaveFileWithTickAsync(file);
        //                var f = new HtFileDinhKem
        //                {
        //                    MaPhieu = entity.MaPhieuDc,
        //                    TenFileDinhKem = file.FileName,
        //                    TenFileDinhKemLuu = savedPath,
        //                    TaiLieuUrl = savedPath,
        //                    Controller = "PhieuDatCoc",
        //                    AcTion = "Edit",
        //                    NgayLap = DateTime.Now,
        //                    MaNhanVien = string.Empty,
        //                    TenNhanVien = string.Empty,
        //                    FileSize = file.FileSize,
        //                    FileType = file.ContentType,
        //                    FullDomain = file.FullDomain,
        //                };
        //                listFiles.Add(f);
        //            }
        //            await _context.HtFileDinhKems.AddRangeAsync(listFiles);
        //        }
        //        if (entity.IsDaKy == true)
        //        {
        //            string flagCNPT = await TaoPhieuCongNoPhaiThuBDSAsync(entity, _context);
        //            if (string.IsNullOrEmpty(flagCNPT))
        //            {
        //                await _baseService.TaoCongNoPTERP(entity.MaPhieuDc, entity.NgayKy, "", entity.MaDuAn);
        //            }
        //            else//Roll back lại khi bị lỗi
        //            {
        //                var delPKBL = _context.KtPhieuCongNoPhaiThus.Where(d => d.MaChungTu == model.MaPhieuDC);
        //                var entityCN = await _context.BhPhieuDatCocs.FirstOrDefaultAsync(d => d.MaPhieuDc.ToLower() == model.MaPhieuDC.ToLower());
        //                entityCN.IsDaKy = false;
        //                _context.KtPhieuCongNoPhaiThus.RemoveRange(delPKBL);
        //                _context.SaveChanges();
        //                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật ngày ký phiếu đặt cọc: {flagCNPT}");
        //            }
        //            await _context.SaveChangesAsync();
        //            return ResultModel.SuccessWithId(entity.MaPhieuDc, "Cập nhật ngày ký phiếu đặt cọc thành công");
        //        }
        //        await _context.SaveChangesAsync();
        //        return ResultModel.SuccessWithId(entity.MaPhieuDc, "Cập nhật ngày ký phiếu đặt cọc thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        using var _context2 = _factory.CreateDbContext();
        //        var delPKBL = _context2.KtPhieuCongNoPhaiThus.Where(d => d.MaChungTu == model.MaPhieuDC);
        //        var entityCN = await _context2.BhPhieuDatCocs.FirstOrDefaultAsync(d => d.MaPhieuDc.ToLower() == model.MaPhieuDC.ToLower());
        //        entityCN.IsDaKy = false;
        //        _context2.KtPhieuCongNoPhaiThus.RemoveRange(delPKBL);
        //        _context2.SaveChanges();
        //        _logger.LogError(ex, "Lỗi khi cập nhật ngày ký phiếu đặt cọc thành công");
        //        return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật ngày ký phiếu đặt cọc thành công: {ex.Message.ToString()}");
        //    }
        //}

        public async Task<ResultModel> XacNhanNgayKyAsync(PhieuDatCocModel model, CancellationToken ct = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.MaPhieuDC))
                return ResultModel.Fail("Thiếu thông tin phiếu.");

            // Lưu các file vừa stage để có thể dọn nếu transaction fail
            var stagedFiles = new List<(string SavedPath, string OriginalName, string Size, string ContentType, string? FullDomain)>();

            await using var context = _factory.CreateDbContext();
            await using var tran = await context.Database.BeginTransactionAsync(ct);

            try
            {
                context.ChangeTracker.Clear();

                // 1) Load entity
                var maPhieu = model.MaPhieuDC.Trim();
                var entity = await context.BhPhieuDatCocs
                    .FirstOrDefaultAsync(d => d.MaPhieuDc.ToUpper() == maPhieu.ToUpper(), ct);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy phiếu đặt cọc.");

                // 2) Parse ngày ký (nullable) an toàn
                DateTime? ngayKi = null;
                if (!string.IsNullOrWhiteSpace(model.NgayKi))
                {
                    if (DateTime.TryParseExact(model.NgayKi.Trim(), "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    {
                        ngayKi = parsed;
                    }
                    else
                    {
                        return ResultModel.Fail("Định dạng ngày ký không hợp lệ (dd/MM/yyyy).");
                    }
                }

                // 3) Lấy file đã upload trước đó (để chống trùng theo tên + size)
                var uploadedFiles = await context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.MaPhieu == entity.MaPhieuDc && d.Controller == "PhieuDatCoc")
                    .Select(d => new { d.TenFileDinhKem, d.FileSize }) // FileSize là string trong entity
                    .ToListAsync(ct);

                // 4) Stage file mới lên đĩa (nếu có)
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (file == null || string.IsNullOrWhiteSpace(file.FileName))
                            continue;

                        bool exists = uploadedFiles.Any(f =>
                            string.Equals(f.TenFileDinhKem, file.FileName, StringComparison.OrdinalIgnoreCase)
                            && f.FileSize == file.FileSize // so sánh string-string
                        );
                        if (exists) continue;

                        // Lưu vật lý (trả về đường dẫn đã lưu)
                        var savedPath = await SaveFileWithTickAsync(file);

                        // ⚠️ stagedFiles gồm 5 phần tử (đúng thứ tự)
                        stagedFiles.Add((savedPath,
                                         file.FileName,
                                         file.FileSize,                    // string (khớp entity)
                                         file.ContentType ?? string.Empty, // tránh null
                                         file.FullDomain));
                    }
                }

                // 5) Update entity
                var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                entity.NgayKi = ngayKi;
                entity.IsDaKy = model.IsDaKy;
                entity.NgayXacNhan = DateTime.Now;
                entity.NguoiXacNhan = nguoiLap?.MaNhanVien;

                // 6) Lưu metadata file vào DB (cùng transaction) nếu có stage
                if (stagedFiles.Count > 0)
                {
                    var newFileEntities = stagedFiles.Select(sf => new HtFileDinhKem
                    {
                        MaPhieu = entity.MaPhieuDc,
                        TenFileDinhKem = sf.OriginalName,
                        TenFileDinhKemLuu = sf.SavedPath,
                        TaiLieuUrl = sf.SavedPath,
                        Controller = "PhieuDatCoc",
                        AcTion = "Edit",
                        NgayLap = DateTime.Now,
                        MaNhanVien = nguoiLap?.MaNhanVien ?? string.Empty,
                        TenNhanVien = nguoiLap?.HoVaTen ?? string.Empty,
                        FileSize = sf.Size,             // string -> OK, hết CS0029
                        FileType = sf.ContentType,
                        FullDomain = sf.FullDomain
                    }).ToList();

                    await context.HtFileDinhKems.AddRangeAsync(newFileEntities, ct);
                }

                // 7) Nếu đã ký -> tạo công nợ nội bộ (cùng transaction)
                if (entity.IsDaKy == true)
                {
                    string flagCNPT = await TaoPhieuCongNoPhaiThuBDSAsync(entity, context);
                    if (!string.IsNullOrEmpty(flagCNPT))
                    {
                        await tran.RollbackAsync(ct);
                        CleanupStagedFilesSafe(stagedFiles);
                        return ResultModel.Fail($"Không thể cập nhật ngày ký PĐC: {flagCNPT}");
                    }
                }

                // 8) Commit dữ liệu nội bộ
                await context.SaveChangesAsync(ct);
                await tran.CommitAsync(ct);

                // 9) Sau COMMIT: đồng bộ ERP (không ảnh hưởng dữ liệu nội bộ nếu lỗi)
                if (entity.IsDaKy == true)
                {
                    try
                    {
                        await _baseService.TaoCongNoPTERP(entity.MaPhieuDc, entity.NgayKi, "", entity.MaDuAn);
                    }
                    catch (Exception exErp)
                    {
                        _logger.LogWarning(exErp, "[XacNhanNgayKyAsync] ERP sync failed for {MaPhieu}", entity.MaPhieuDc);
                        return ResultModel.SuccessWithId(entity.MaPhieuDc,
                            "Cập nhật ngày ký thành công, nhưng đồng bộ ERP thất bại. Vui lòng thử lại sau.");
                    }
                }

                return ResultModel.SuccessWithId(entity.MaPhieuDc, "Cập nhật ngày ký phiếu đặt cọc thành công");
            }
            catch (Exception ex)
            {
                try { await tran.RollbackAsync(ct); } catch { /* ignore */ }
                CleanupStagedFilesSafe(stagedFiles);
                _logger.LogError(ex, "[XacNhanNgayKyAsync] Lỗi khi cập nhật ngày ký PĐC");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật ngày ký phiếu đặt cọc: {ex.Message}");
            }

            // ===== LOCAL HELPERS =====
            static void CleanupStagedFilesSafe(IEnumerable<(string SavedPath, string OriginalName, string Size, string ContentType, string? FullDomain)> files)
            {
                foreach (var f in files)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(f.SavedPath) && System.IO.File.Exists(f.SavedPath))
                            System.IO.File.Delete(f.SavedPath);
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }
            }
        }


        #endregion

        #region Send email
        public async Task<ResultModel> SendEmailAsync(PhieuDatCocModel model, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var e = await ThongTinInPDC(model.MaPhieuDC) ?? throw new InvalidOperationException($"Không có dữ liệu {model.MaPhieuDC}");
                var data = new Dictionary<string, string>
                {
                    ["MaPhieu"] = e.MaPhieu ?? model.MaPhieuDC,
                    ["TenCongTy"] = e.TenCongTy,
                    ["TenKhachHang"] = e.TenKhachHang,
                    ["TenNguoiDaiDien"] = e.TenNguoiDaiDien,
                    ["NgaySinh"] = string.Format("{0:dd/MM/yyyy}", e.NgaySinh),
                    ["QT"] = e.QT,
                    ["SoDienThoai"] = e.SoDienThoai,
                    ["Email"] = e.Email,
                    ["SoCMND"] = e.IdCard,
                    ["NgayCap"] = string.Format("{0:dd/MM/yyyy}", e.NgayCapIdCard),
                    ["NoiCap"] = e.NoiCapIdCard,
                    ["DiaChiThuongTru"] = e.DiaChiThuongTru,
                    ["DiaChiHienTai"] = e.DiaChiLienLac,
                    ["TenDuAn"] = e.TenDuAn,
                    ["DiaChiCongTy"] = e.DiaChiCongTy,
                    ["MaCanHo"] = e.MaCanHo,
                    ["TangBlock"] = $"{e.Tang}.{e.Block}",
                    ["Tang"] = e.Tang,
                    ["Block"] = e.Block,
                    ["DienTichThongThuy"] = string.Format("{0:N2}", e.DienTichLotLong),
                    ["DienTichTimTuong"] = string.Format("{0:N2}", e.DienTichTimTuong),
                    ["GiaBanCanHo"] = string.Format("{0:N0}", e.GiaBanCanHo),
                    ["GiaBanCanHoBangChu"] = FormatHelper.DocSoTienBangChu(e.GiaBanCanHo),
                    ["PhiBaoTri"] = "0",
                    ["NgayHienTai"] = DateTime.Now.Day.ToString("00"),
                    ["ThangHienTai"] = DateTime.Now.Month.ToString("00"),
                    ["NamHienTai"] = DateTime.Now.Year.ToString()
                };

                var email = new HtSendEmail();

                var template = await _context.HtTemplates.Where(d => d.SoTemplate == "XAC_NHAN_THANH_TOAN").FirstOrDefaultAsync();

                if (template == null)
                    return ResultModel.Fail("Không tìm thấy template email.");

                var thongTinCongTy = await _baseService.ThongTinCongTyAsync();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                email.IdEmail = Guid.NewGuid().ToString();
                email.Email = e.Email;
                email.TieuDe = template?.TieuDe;
                email.NgayLap = DateTime.Now;
                email.TrangThai = false;
                email.NguoiLap = NguoiLap.MaNhanVien;

                var body = _templateService.Render(template?.NoiDung, new
                {
                    urlLogo = thongTinCongTy.Logo.Select(d => d.FullDomain).FirstOrDefault(),
                    tenCongTy = thongTinCongTy.TenCongTy,
                    diaChiCongTy = thongTinCongTy.DiaChiCongTy,
                    maPhieu = e?.MaPhieu,
                    ngayLapPhieu = string.Format("{0:dd/MM/yyyy}", e?.NgayLap ?? string.Empty),
                    ngayHienTai = string.Format("{0:dd/MM/yyyy}", DateTime.Now),
                    namHienTai = DateTime.Now.Year,
                    hoTenKhachHang = e?.TenKhachHang,
                    tenSanPham = e?.TenSanPham,
                    noiDungChiTiet = e?.GhiChu ?? string.Empty,
                    tongTien = string.Format("{0:N0}", e?.GiaBanCanHo),
                    tongTienBangChu = FormatHelper.DocSoTienBangChu(e.GiaBanCanHo)

                }, htmlEncode: false);

                email.NoiDung = body;

                var fileAttachments = new List<HtSendEmailAttachment>();
                var fileDinhkems = await _context.HtFileDinhKems.Where(d => d.MaPhieu == template.MaTemplate && d.Controller == "Template").ToListAsync();

                if (fileDinhkems.Any())
                {
                    foreach (var f in fileDinhkems)
                    {
                        var downloaded = await _templateService.DownloadFileFromUrlAsync(f.TenFileDinhKemLuu, webRootPath);
                        if (downloaded == null)
                        {
                            _logger.LogError($"[SendEmailAsync] - downloaded: null");
                            continue;
                        }


                        var fileBytes = downloaded.Value.Bytes;
                        var fileName = f.TenFileDinhKem;

                        if (Path.GetExtension(fileName).Equals(".docx", StringComparison.OrdinalIgnoreCase))
                        {
                            // Replace trước khi gửi
                            fileBytes = _templateService.ReplacePlaceholders(fileBytes, data);
                            var pdfBytes = _templateService.TryDocxToPdfAsync(fileBytes);
                            if (pdfBytes != null)
                            {
                                fileBytes = pdfBytes;
                                fileName = Path.ChangeExtension(fileName, ".pdf");
                            }

                        }

                        var att = new HtSendEmailAttachment
                        {
                            EmailId = email.IdEmail,
                            FileName = fileName,
                            FileBytes = fileBytes
                        };

                        fileAttachments.Add(att);
                    }
                    await _context.HtSendEmailAttachments.AddRangeAsync(fileAttachments);
                    await _context.HtSendEmails.AddAsync(email);
                    await _context.SaveChangesAsync();
                }

                return ResultModel.Success("Gửi email thành công");
            }
            catch
            {
                return ResultModel.Fail("Gửi email không thành công");
            }
        }
        #endregion

        #region Tạo phiếu công nợ phải thu sau khi đã xác nhận ngày ký phiếu đặt cọc
        public async Task<string> TaoPhieuCongNoPhaiThuERPAsync(BhPhieuDatCoc pdc, AppDbContext context)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaPhieu", pdc.MaPhieuDc);
                param.Add("@NgayKy", pdc.NgayKi);
                param.Add("@MaCongViec", "PhieuDatCoc");

                var result = (await connection.QueryAsync<CongNoPhaiThuModel>(
                    "Proc_TaoPhieuCongNoPhaiThu",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                KtPhieuCongNoPhaiThu r;
                foreach (var item in result)
                {
                    r = new KtPhieuCongNoPhaiThu();//Trường hợp ở đây MaPhieu đã tự tăng trong Seq_PCNPT rùi nha
                    r.MaPhieu = await SinhMaPhieuCNPTTuDongAsync("PCNPT-", context, 5);
                    r.NgayLap = DateTime.Now;
                    r.DuAn = item.DuAn;
                    r.MaChungTu = item.MaChungTu;
                    r.IdChungTu = item.IdChungTu;
                    r.NoiDung = item.NoiDung;
                    r.HanThanhToan = item.HanThanhToan;
                    r.MaKhachHang = item.MaKhachHang;
                    r.TenKhachHang = item.TenKhachHang;
                    r.SoTien = item.SoTien;
                    r.MaCongViec = item.MaCongViec;
                    r.MaDoiTuong = item.MaKhachHang;
                    await context.KtPhieuCongNoPhaiThus.AddAsync(r);
                    context.SaveChanges();
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public async Task<string> TaoPhieuCongNoPhaiThuBDSAsync(BhPhieuDatCoc pdc, AppDbContext context)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaPhieu", pdc.MaPhieuDc);
                param.Add("@NgayKy", pdc.NgayKi);
                param.Add("@MaCongViec", "PhieuDatCoc");

                var result = (await connection.QueryAsync<CongNoPhaiThuModel>(
                    "Proc_TaoPhieuCongNoBDS",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                //KtPhieuCongNoPhaiThu r;
                //foreach (var item in result)
                //{
                //    r = new KtPhieuCongNoPhaiThu();//Trường hợp ở đây MaPhieu đã tự tăng trong Seq_PCNPT rùi nha
                //    r.MaPhieu = await SinhMaPhieuCNPTTuDongAsync("PCNPT-", context, 5);
                //    r.NgayLap = DateTime.Now;
                //    r.DuAn = item.DuAn;
                //    r.MaChungTu = item.MaChungTu;
                //    r.IdChungTu = item.IdChungTu;
                //    r.NoiDung = item.NoiDung;
                //    r.HanThanhToan = item.HanThanhToan;
                //    r.MaKhachHang = item.MaKhachHang;
                //    r.TenKhachHang = item.TenKhachHang;
                //    r.SoTien = item.SoTien;
                //    r.MaCongViec = item.MaCongViec;
                //    r.MaDoiTuong = item.MaKhachHang;
                //    await context.KtPhieuCongNoPhaiThus.AddAsync(r);
                //    context.SaveChanges();
                //}
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public async Task<string> SinhMaPhieuCNPTTuDongAsync(string prefix, AppDbContext context, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KtPhieuCongNoPhaiThus
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
    }
}

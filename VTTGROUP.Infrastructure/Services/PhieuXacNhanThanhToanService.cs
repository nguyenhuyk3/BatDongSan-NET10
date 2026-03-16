using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.PhieuXacNhanThanhToan;
using VTTGROUP.Domain.Model.TongHopCongNoPhaiThu;
using VTTGROUP.Infrastructure.Database;


namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuXacNhanThanhToanService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<TongHopBookingService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        private readonly IMemoryCache _cache;
        public PhieuXacNhanThanhToanService(IDbContextFactory<AppDbContext> factory, ILogger<TongHopBookingService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService, IMemoryCache cache)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
            _cache = cache;
        }

        #region Danh sách phiếu xác nhận thanh toán
        public async Task<(List<PhieuXacNhanThanhToanPagingDto> Data, int TotalCount)> GetPagingAsync(
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

                var result = (await connection.QueryAsync<PhieuXacNhanThanhToanPagingDto>(
                    "Proc_PhieuXacNhanThanhToan_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;

                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sanh sách phiếu phiếu xác nhận booking");
                var result = new List<PhieuXacNhanThanhToanPagingDto>();
                return (result, 0);
            }
        }
        #endregion

        #region Thêm, xóa, sử phiếu xác nhận thanh toán
        public async Task<ResultModel> SavePhieuAsync(
      PhieuXacNhanThanhToanModel model,
      List<PhieuXacNhanThanhToanPhieuCongNoModel> listCN,
      List<PhieuXacNhanThanhToanNguonChuyenDoiModel> listNCD)
        {
            try
            {
                using var _context = _factory.CreateDbContext();

                var oldAdc = _context.ChangeTracker.AutoDetectChangesEnabled;
                _context.ChangeTracker.AutoDetectChangesEnabled = false;

                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();

                DateTime? ngayHachToan = null;
                if (!string.IsNullOrWhiteSpace(model.NgayHachToan)
                    && DateTime.TryParseExact(model.NgayHachToan, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                              DateTimeStyles.None, out var d))
                    ngayHachToan = d;

                listCN = (listCN ?? new()).Where(x => x != null).ToList();
                listNCD = (listNCD ?? new()).Where(x => x != null).ToList();

                // ---- BEGIN TRANSACTION + APPLOCK ----
                await using var tx = await _context.Database.BeginTransactionAsync();

                try
                {
                    var maPhieu = await SinhMaPhieuDCTuDongAsync("XNTT-", _context, 4);
                    // 3) Insert header
                    var record = new KtPhieuXacNhanThanhToan
                    {
                        MaPhieu = maPhieu,
                        MaDuAn = model.MaDuAn,
                        LoaiPhieu = model.LoaiPhieu,
                        HinhThucThanhToan = model.MaHinhThucTT,
                        MaKhachHang = model.MaKhachHang,
                        TenKhachHang = model.TenKhachHang,
                        NgayHachToan = ngayHachToan,
                        NoiDung = model.NoiDung,
                        NguoiLap = NguoiLap?.MaNhanVien,
                        NgayLap = DateTime.Now,
                        SoChungTu = model.SoChungTu,
                        IsXacNhan = false
                    };
                    await _context.KtPhieuXacNhanThanhToans.AddAsync(record);

                    // 4) Insert chi tiết công nợ
                    if (listCN.Any())
                    {
                        var listCT = listCN.Select(item => new KtPhieuXacNhanThanhToanChiTiet
                        {
                            MaPhieu = maPhieu,
                            MaPhieuCongNo = item.MaPhieuCongNo,
                            IdChungTu = item.IdChungTu,
                            NoiDung = item.NoiDung,
                            SoTien = item.SoTien
                        }).ToList();
                        await _context.KtPhieuXacNhanThanhToanChiTiets.AddRangeAsync(listCT);
                    }


                    // 5) Insert chi tiết nguồn chuyển đổi (lọc trùng tuỳ nghiệp vụ)
                    if (listNCD.Any() && model.IsCanTruNguonKhac)
                    {
                        var listCT = listNCD
                            .GroupBy(x => new { x.MaPhieu, x.SoTien })
                            .Select(g => new KtPhieuXacNhanThanhToanPhieuChuyenDoi
                            {
                                MaPhieu = maPhieu,
                                MaPhieuNguon = g.Key.MaPhieu,
                                SoTienChuyenDoi = g.Key.SoTien
                            }).ToList();

                        await _context.KtPhieuXacNhanThanhToanPhieuChuyenDois.AddRangeAsync(listCT);
                    }

                    // 6) Insert file đính kèm
                    if (model.Files != null && model.Files.Any())
                    {
                        var listFiles = new List<HtFileDinhKem>();
                        foreach (var file in model.Files)
                        {
                            if (string.IsNullOrEmpty(file.FileName)) continue;

                            var savedPath = await SaveFileWithTickAsync(file);
                            listFiles.Add(new HtFileDinhKem
                            {
                                MaPhieu = maPhieu,
                                TenFileDinhKem = file.FileName,
                                TenFileDinhKemLuu = savedPath,
                                TaiLieuUrl = savedPath,
                                Controller = "XacNhanThanhToan",
                                AcTion = "Create",
                                NgayLap = DateTime.Now,
                                MaNhanVien = NguoiLap?.MaNhanVien ?? string.Empty,
                                TenNhanVien = NguoiLap?.HoVaTen ?? string.Empty,
                                FileSize = file.FileSize,
                                FileType = file.ContentType,
                                FullDomain = file.FullDomain
                            });
                        }
                        if (listFiles.Count > 0)
                            await _context.HtFileDinhKems.AddRangeAsync(listFiles);
                    }

                    // 7) Lưu & commit (nhả lock)
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    _context.ChangeTracker.AutoDetectChangesEnabled = oldAdc;
                    return ResultModel.SuccessWithId(maPhieu, "Thêm phiếu xác nhận thanh toán thành công");
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    _context.ChangeTracker.AutoDetectChangesEnabled = oldAdc;

                    // Nếu là lỗi timeout khi lấy applock
                    if (ex is Microsoft.Data.SqlClient.SqlException sqlEx &&
                        (sqlEx.Message?.Contains("FAILED_TO_GET_APPLOCK") ?? false))
                    {
                        return ResultModel.Fail("Hệ thống đang bận cấp số chứng từ. Vui lòng thử lại trong giây lát.");
                    }

                    _logger.LogError(ex, "Lỗi khi Thêm phiếu xác nhận");
                    return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu xác nhận thanh toán: {ex.Message}");
                }
                // ---- END TRANSACTION ----
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi ngoài khi Thêm phiếu xác nhận");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateByIdAsync(
    PhieuXacNhanThanhToanModel model,
    List<PhieuXacNhanThanhToanPhieuCongNoModel> listCN,
    List<PhieuXacNhanThanhToanNguonChuyenDoiModel> listNCD,
    CancellationToken ct = default)
        {
            try
            {
                await using var _context = _factory.CreateDbContext();

                // 1) Tìm entity (giữ index, tránh ToLower)
                var entity = await _context.KtPhieuXacNhanThanhToans
                    .FirstOrDefaultAsync(d => d.MaPhieu == model.MaPhieu, ct);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy Phiếu xác nhận thanh toán.");

                // 2) Chuẩn hoá input list
                listCN = (listCN ?? new()).Where(x => x != null).ToList();
                listNCD = (listNCD ?? new()).Where(x => x != null).ToList();

                // 3) Parse ngày hạch toán an toàn
                DateTime? ngayHachToan = null;
                if (!string.IsNullOrWhiteSpace(model.NgayHachToan) &&
                    DateTime.TryParseExact(model.NgayHachToan, "dd/MM/yyyy",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                {
                    ngayHachToan = d;
                }

                // 4) Cập nhật header
                entity.NgayHachToan = ngayHachToan;
                entity.SoChungTu = model.SoChungTu;
                entity.NoiDung = model.NoiDung;
                // TODO: entity.NgaySua = DateTime.Now; entity.NguoiSua = ...

                // 5) Transaction để đảm bảo tính nguyên tử
                await using var tx = await _context.Database.BeginTransactionAsync(ct);

                // 5.1) Xoá chi tiết công nợ cũ
                await _context.KtPhieuXacNhanThanhToanChiTiets
                    .Where(x => x.MaPhieu == entity.MaPhieu)
                    .ExecuteDeleteAsync(ct);

                // 5.2) Thêm chi tiết công nợ mới (nếu có)
                if (listCN.Count > 0)
                {
                    var listCtCn = listCN.Select(item => new KtPhieuXacNhanThanhToanChiTiet
                    {
                        MaPhieu = entity.MaPhieu,
                        MaPhieuCongNo = item.MaPhieuCongNo,
                        IdChungTu = item.IdChungTu,
                        NoiDung = item.NoiDung,
                        SoTien = item.SoTien
                    }).ToList();

                    await _context.KtPhieuXacNhanThanhToanChiTiets.AddRangeAsync(listCtCn, ct);
                }

                // 5.3) Xoá chi tiết nguồn chuyển đổi cũ
                await _context.KtPhieuXacNhanThanhToanPhieuChuyenDois
                    .Where(x => x.MaPhieu == entity.MaPhieu)
                    .ExecuteDeleteAsync(ct);

                // 5.4) Thêm nguồn chuyển đổi (nếu bật cấn trừ)
                if (model.IsCanTruNguonKhac && listNCD.Count > 0)
                {
                    // Gộp đúng theo "mã nguồn" (đặt tên theo model của bạn)
                    // Ví dụ: group theo MaPhieuNguon/IdChungTuNguon (TUỲ THUỘC FIELD THỰC)
                    var grouped = listNCD
                        .GroupBy(x => new { x.MaPhieu /* hoặc x.MaPhieuNguon / x.IdChungTuNguon */ })
                        .Select(g => new KtPhieuXacNhanThanhToanPhieuChuyenDoi
                        {
                            MaPhieu = entity.MaPhieu,
                            MaPhieuNguon = g.Key.MaPhieu,  // sửa theo field thực
                            SoTienChuyenDoi = g.Sum(z => z.SoTien) // CỘNG DỒN số tiền
                        })
                        .ToList();

                    await _context.KtPhieuXacNhanThanhToanPhieuChuyenDois.AddRangeAsync(grouped, ct);
                }

                // 5.5) File đính kèm
                var uploadedFiles = await _context.HtFileDinhKems
                    .Where(d => d.MaPhieu == entity.MaPhieu && d.Controller == "XacNhanThanhToan")
                    .ToListAsync(ct);

                if (model.Files != null && model.Files.Any())
                {
                    var toInsert = new List<HtFileDinhKem>();

                    foreach (var file in model.Files)
                    {
                        if (string.IsNullOrWhiteSpace(file.FileName))
                            continue;

                        // Kiểm tra trùng cơ bản (có thể nâng cấp bằng hash)
                        bool exists = uploadedFiles.Any(f =>
                            f.TenFileDinhKem == file.FileName &&
                            f.FileSize == file.FileSize &&
                            f.FileType == file.ContentType
                        );
                        if (exists) continue;

                        string savedPath;
                        try
                        {
                            savedPath = await SaveFileWithTickAsync(file /*, ct nếu hỗ trợ */);
                        }
                        catch (Exception ioEx)
                        {
                            _logger.LogWarning(ioEx, "Không lưu được file {FileName}", file.FileName);
                            // Tuỳ nghiệp vụ: continue hoặc throw để rollback
                            continue;
                        }

                        toInsert.Add(new HtFileDinhKem
                        {
                            MaPhieu = entity.MaPhieu,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "XacNhanThanhToan",
                            AcTion = "Edit",
                            NgayLap = DateTime.Now,
                            MaNhanVien = string.Empty, // TODO: lấy từ CurrentUser
                            TenNhanVien = string.Empty, // TODO
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain
                        });
                    }

                    if (toInsert.Count > 0)
                        await _context.HtFileDinhKems.AddRangeAsync(toInsert, ct);
                }

                // 6) Lưu + commit
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật Phiếu xác nhận thanh toán thành công.");
            }
            catch (OperationCanceledException)
            {
                return ResultModel.Fail("Yêu cầu đã bị huỷ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật Phiếu xác nhận thanh toán.");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        //  public async Task<ResultModel> UpdateByIdAsync(PhieuXacNhanThanhToanModel model, List<PhieuXacNhanThanhToanPhieuCongNoModel> listCN,
        //List<PhieuXacNhanThanhToanNguonChuyenDoiModel> listNCD, CancellationToken ct = default)
        //  {
        //      try
        //      {
        //          using var _context = _factory.CreateDbContext();
        //          var entity = await _context.KtPhieuXacNhanThanhToans.FirstOrDefaultAsync(d => d.MaPhieu.ToLower() == model.MaPhieu.ToLower());
        //          if (entity == null)
        //          {
        //              return ResultModel.Fail("Không tìm thấy phiếu xác nhận thanh toán mua bán.");
        //          }
        //          entity.NgayHachToan = !string.IsNullOrEmpty(model.NgayHachToan) ? DateTime.ParseExact(model.NgayHachToan, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
        //          entity.SoChungTu = model.SoChungTu;
        //          entity.NoiDung = model.NoiDung;
        //          // 4) Insert chi tiết công nợ
        //          // 2) Xoá hết chi tiết hiện tại rồi thêm lại
        //          await _context.KtPhieuXacNhanThanhToanChiTiets
        //              .Where(x => x.MaPhieu == model.MaPhieu)
        //              .ExecuteDeleteAsync(ct);

        //          if (listCN.Any())
        //          {
        //              var listCT = listCN.Select(item => new KtPhieuXacNhanThanhToanChiTiet
        //              {
        //                  MaPhieu = entity.MaPhieu,
        //                  MaPhieuCongNo = item.MaPhieuCongNo,
        //                  IdChungTu = item.IdChungTu,
        //                  NoiDung = item.NoiDung,
        //                  SoTien = item.SoTien
        //              }).ToList();
        //              await _context.KtPhieuXacNhanThanhToanChiTiets.AddRangeAsync(listCT);
        //          }


        //          // 5) Insert chi tiết nguồn chuyển đổi (lọc trùng tuỳ nghiệp vụ)
        //          await _context.KtPhieuXacNhanThanhToanPhieuChuyenDois
        //         .Where(x => x.MaPhieu == model.MaPhieu)
        //         .ExecuteDeleteAsync(ct);

        //          if (listNCD.Any() && model.IsCanTruNguonKhac)
        //          {
        //              var listCT = listNCD
        //                  .GroupBy(x => new { x.MaPhieu, x.SoTien })
        //                  .Select(g => new KtPhieuXacNhanThanhToanPhieuChuyenDoi
        //                  {
        //                      MaPhieu = entity.MaPhieu,
        //                      MaPhieuNguon = g.Key.MaPhieu,
        //                      SoTienChuyenDoi = g.Key.SoTien
        //                  }).ToList();

        //              await _context.KtPhieuXacNhanThanhToanPhieuChuyenDois.AddRangeAsync(listCT);
        //          }

        //          List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
        //          var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == entity.MaPhieu && d.Controller == "XacNhanThanhToan").ToListAsync();

        //          if (model.Files != null && model.Files.Any())
        //          {
        //              foreach (var file in model.Files)
        //              {
        //                  if (string.IsNullOrEmpty(file.FileName)) continue;

        //                  bool exists = UploadedFiles.Any(f =>
        //                      f.TenFileDinhKem == file.FileName &&
        //                      f.FileSize == file.FileSize
        //                  );
        //                  if (exists)
        //                      continue;

        //                  var savedPath = await SaveFileWithTickAsync(file);
        //                  var f = new HtFileDinhKem
        //                  {
        //                      MaPhieu = model.MaPhieu,
        //                      TenFileDinhKem = file.FileName,
        //                      TenFileDinhKemLuu = savedPath,
        //                      TaiLieuUrl = savedPath,
        //                      Controller = "XacNhanThanhToan",
        //                      AcTion = "Edit",
        //                      NgayLap = DateTime.Now,
        //                      MaNhanVien = string.Empty,
        //                      TenNhanVien = string.Empty,
        //                      FileSize = file.FileSize,
        //                      FileType = file.ContentType,
        //                      FullDomain = file.FullDomain,
        //                  };
        //                  listFiles.Add(f);
        //              }
        //              await _context.HtFileDinhKems.AddRangeAsync(listFiles);
        //          }
        //          await _context.SaveChangesAsync();
        //          return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật phiếu xác nhận thanh toán thành công");
        //      }
        //      catch (Exception ex)
        //      {
        //          _logger.LogError(ex, "Lỗi khi cập nhật hợp đồng mua bán");
        //          return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật hợp đồng mua bán: {ex.Message.ToString()}");
        //      }
        //  }
        public async Task<ResultModel> DeletePTHAsync(
      string maPhieu,
      string webRootPath,
      CancellationToken ct = default)
        {
            try
            {
                await using var _context = await _factory.CreateDbContextAsync(ct);

                // 1) Kiểm tra tồn tại (nhẹ, không tracking)
                var exists = await _context.KtPhieuXacNhanThanhToans
                    .AsNoTracking()
                    .AnyAsync(x => x.MaPhieu == maPhieu, ct);

                if (!exists)
                    return ResultModel.Fail("Không tìm thấy phiếu xác nhận thanh toán");

                // 2) Lấy danh sách file (để còn xóa ngoài ổ đĩa sau khi commit)
                var files = await _context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.Controller == "XacNhanThanhToan" && d.MaPhieu == maPhieu)
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync(ct);

                // 3) Transaction: xóa DB + file trong 1 flow
                await using var tx = await _context.Database.BeginTransactionAsync(ct);

                // 3.1) Xóa bảng con liên kết
                await _context.KtPhieuXacNhanThanhToanPhieuChuyenDois
                    .Where(d => d.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct);

                await _context.KtPhieuXacNhanThanhToanChiTiets
                    .Where(d => d.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct);

                // 3.2) Xóa bản ghi file (sau khi đã copy path ra biến `files`)
                await _context.HtFileDinhKems
                    .Where(d => d.Controller == "XacNhanThanhToan" && d.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct);

                // 3.3) Xóa record chính
                await _context.KtPhieuXacNhanThanhToans
                    .Where(d => d.MaPhieu == maPhieu)
                    .ExecuteDeleteAsync(ct);

                // 3.4) Commit DB trước rồi mới xóa file vật lý (tránh rủi ro rollback mà file đã mất)
                await tx.CommitAsync(ct);

                // --- B3: Xóa file vật lý ngoài transaction ---
                int cFile = 0;
                foreach (var relPath in files.Distinct().Where(p => !string.IsNullOrWhiteSpace(p)))
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
                        _logger.LogWarning(exDel, "[DeletePTHAsync] Không xóa được file: {RelPath}", relPath);
                        // không throw – tránh rollback DB sau khi đã commit
                    }
                }
                return ResultModel.Success($"Xóa {maPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePTHAsync] Lỗi khi xóa phiếu xác nhận thanh toán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<PhieuXacNhanThanhToanPagingDto> listTHBK, string webRootPath)
        {
            try
            {
                var ids = listTHBK?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
                    .Select(x => x!.MaPhieu.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B1: Lấy trước danh sách file cần xóa (vật lý) ---
                var filePaths = await _context.HtFileDinhKems
                    .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "XacNhanThanhToan")
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c2 = await _context.HtFileDinhKems
                .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "XacNhanThanhToan")
                .ExecuteDeleteAsync();

                var c1 = await _context.KtPhieuXacNhanThanhToanPhieuChuyenDois
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var c3 = await _context.KtPhieuXacNhanThanhToanChiTiets
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var cParent = await _context.KtPhieuXacNhanThanhToans
                    .Where(k => ids.Contains(k.MaPhieu))
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

                return ResultModel.Success("Đã xóa danh sách phiếu xác nhận thanh toán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách phiếu xác nhận thanh toán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin phiếu xác nhận thanh toán      
        public async Task<ResultModel> GetByIdAsync(string id, CancellationToken ct = default)
        {
            try
            {
                await using var _context = await _factory.CreateDbContextAsync(ct);

                // Lấy entity chính
                var entity = await (
                    from cn in _context.KtPhieuXacNhanThanhToans.AsNoTracking()
                    join da in _context.DaDanhMucDuAns.AsNoTracking()
                        on cn.MaDuAn equals da.MaDuAn into dtDuAn
                    from da2 in dtDuAn.DefaultIfEmpty()

                    join httt in _context.HtDanhMucHinhThucThanhToans.AsNoTracking()
                        on cn.HinhThucThanhToan equals httt.MaHttt into dtHTTT
                    from httt2 in dtHTTT.DefaultIfEmpty()

                    where cn.MaPhieu == id
                    select new PhieuXacNhanThanhToanModel
                    {
                        MaPhieu = cn.MaPhieu,
                        NgayLap = cn.NgayLap ?? DateTime.Now,
                        NgayHachToanDt = cn.NgayHachToan,                 // <-- giữ DateTime? để format ngoài DB
                        MaDuAn = cn.MaDuAn ?? string.Empty,
                        TenDuAn = da2 != null ? da2.TenDuAn : null,
                        NoiDung = cn.NoiDung,
                        IsXacNhan = cn.IsXacNhan ?? false,
                        MaKhachHang = cn.MaKhachHang,
                        TenKhachHang = cn.TenKhachHang,
                        MaNhanVien = cn.NguoiLap,
                        SoChungTu = cn.SoChungTu,
                        LoaiPhieu = cn.LoaiPhieu,
                        MaHinhThucTT = cn.HinhThucThanhToan,
                        TenHinhThucTT = httt2 != null ? httt2.TenHttt : null,
                        IsCanTruNguonKhac = httt2.IsCanTruNguonKhac ?? false
                    })
                    .FirstOrDefaultAsync(ct);

                if (entity == null)
                {
                    // Phiếu mới
                    var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity = new PhieuXacNhanThanhToanModel
                    {
                        MaPhieu = await SinhMaPhieuDCTuDongAsync("XNTT-", _context, 4),
                        NgayLap = DateTime.Now,
                        NgayHachToanDt = DateTime.Now,
                        NguoiLap = nguoiLap
                    };
                }
                else
                {
                    // Bổ sung thông tin người lập & Flag
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    if (!entity.IsXacNhan && entity.NguoiLap?.MaNhanVien == _currentUser.MaNhanVien)
                    {
                        entity.FlagTong = true;
                    }

                    // Load file đính kèm
                    entity.Files = await _context.HtFileDinhKems.AsNoTracking()
                        .Where(d => d.Controller == "XacNhanThanhToan" && d.MaPhieu == id)
                        .Select(d => new UploadedFileModel
                        {
                            Id = d.Id,
                            FileName = d.TenFileDinhKem,
                            FileNameSave = d.TenFileDinhKemLuu,
                            FileSize = d.FileSize,
                            ContentType = d.FileType
                        })
                        .ToListAsync(ct);
                }

                // Format ngày cho UI (ngoài DB)
                entity.NgayHachToan = entity.NgayHachToanDt.HasValue
                    ? entity.NgayHachToanDt.Value.ToString("dd/MM/yyyy")
                    : string.Empty;

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu xác nhận thanh toán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        #endregion

        #region Hàm tăng tự động của phiếu xác nhận thanh toán        
        public async Task<string> SinhMaPhieuDCTuDongAsync(string prefix, AppDbContext _context, int padding = 4)
        {
            // await using var tx = await _context.Database.BeginTransactionAsync();

            var period = DateTime.Now.ToString("yyMM");              // 2509
            var basePrefix = $"{prefix.TrimEnd('-')}{period}-";      // "XNTT2509-"
                                                                     //        var lockName = $"XNTT_SEQ_{basePrefix}";                 // "XNTT_SEQ_XNTT2509-"

            //        // Lấy app lock để serialize cấp số trong transaction này
            //        await _context.Database.ExecuteSqlRawAsync(@"
            //    DECLARE @ret int;
            //    EXEC @ret = sp_getapplock 
            //        @Resource = {0},
            //        @LockMode = 'Exclusive',
            //        @LockOwner = 'Transaction',
            //        @LockTimeout = 10000; -- 10s
            //    IF (@ret < 0)
            //        THROW 51000, 'FAILED_TO_GET_APPLOCK', 1;
            //", lockName);

            // Sau khi giữ lock, đọc mã lớn nhất (đã padding nên OrderByDescending theo string OK)
            var lastCode = await _context.KtPhieuXacNhanThanhToans
                .Where(x => x.MaPhieu.StartsWith(basePrefix))
                .OrderByDescending(x => x.MaPhieu)
                .Select(x => x.MaPhieu)
                .FirstOrDefaultAsync();

            int lastSeq = 0;
            if (!string.IsNullOrEmpty(lastCode))
            {
                var seqPart = lastCode.Substring(basePrefix.Length);
                int.TryParse(seqPart, out lastSeq);
            }

            var next = $"{basePrefix}{(lastSeq + 1).ToString($"D{padding}")}";

            // Commit để nhả lock
            //  await tx.CommitAsync();
            return next;
        }

        #endregion

        #region Load combobox
        public async Task<List<HtDanhMucHinhThucThanhToan>> GetByHinhThucTTAsync()
        {
            var entity = new List<HtDanhMucHinhThucThanhToan>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.HtDanhMucHinhThucThanhToans.ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtDanhMucHinhThucThanhToan>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách hình thức thanh toán");
            }
            return entity;
        }
        #endregion

        #region Thông tin khách hàng công nợ
        public async Task<(List<PhieuXacNhanThanhToanPhieuCongNoModel> Data, int TotalCount)> GetPagingKhachHangPopupAsync(string maDuAn, string maPhieu, string loaiPhieu, string maKhachHang, int page, int pageSize, string? qSearch)
        {
            var result = new List<PhieuXacNhanThanhToanPhieuCongNoModel>();
            try
            {
                qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaPhieu", maPhieu);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@LoaiPhieu", loaiPhieu);
                param.Add("@MaKhachHang", maKhachHang);
                param.Add("@Page", null);
                param.Add("@PageSize", null);
                param.Add("@QSearch", qSearch);
                result = (await connection.QueryAsync<PhieuXacNhanThanhToanPhieuCongNoModel>(
                    "Proc_PhieuXacNhanThanhToan_PhieuCongNo",
                   param,
                   commandType: CommandType.StoredProcedure
               )).ToList();

                // DISTINCT theo MaKhachHang (ưu tiên tên khác rỗng, trim để tránh lệch do khoảng trắng)
                var distinct = result
                    .Where(r => !string.IsNullOrWhiteSpace(r.MaKhachHang))
                    .GroupBy(r => r.MaKhachHang.Trim())
                    .Select(g => new PhieuXacNhanThanhToanPhieuCongNoModel
                    {
                        MaKhachHang = g.Key,
                        TenKhachHang = g.Select(x => (x.TenKhachHang ?? "").Trim())
                                        .FirstOrDefault(t => !string.IsNullOrEmpty(t)) ?? ""
                    })
                    .OrderBy(x => x.MaKhachHang) // tuỳ ý
                    .ToList();
                return (distinct, distinct.Count);
            }
            catch (Exception ex)
            {
                result = new List<PhieuXacNhanThanhToanPhieuCongNoModel>();
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu hợp đồng chuyển nhượng");
                return (result, 0);
            }
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

        #region  Lấy danh sách chi tiết công nợ  
        public async Task<(List<PhieuXacNhanThanhToanPhieuCongNoModel> Data, int TotalCount)> GetDanhSachCongNoAsync(string maPhieu, string maDuAn, string loaiPhieu, string maKhachHang, int? page, int? pageSize, string? qSearch)
        {
            var entity = new List<PhieuXacNhanThanhToanPhieuCongNoModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaPhieu", maPhieu);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@LoaiPhieu", loaiPhieu);
                param.Add("@MaKhachHang", maKhachHang);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                entity = (await connection.QueryAsync<PhieuXacNhanThanhToanPhieuCongNoModel>(
                    "Proc_PhieuXacNhanThanhToan_PhieuCongNo",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<PhieuXacNhanThanhToanPhieuCongNoModel>();
                }
                int total = entity.FirstOrDefault()?.TotalCount ?? 0;

                return (entity, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách chi tiết công nợ");
                entity = new List<PhieuXacNhanThanhToanPhieuCongNoModel>();
                return (entity, 0);
            }

        }
        #endregion

        #region  Lấy danh sách chi tiết nguồn chuyển đổi
        public async Task<(List<PhieuXacNhanThanhToanNguonChuyenDoiModel> Data, int TotalCount)> GetDanhSachNguonChuyenDoiAsync(string maPhieu, string maDuAn, string loaiPhieu, string maKhachHang, int? page, int? pageSize, string? qSearch)
        {
            var entity = new List<PhieuXacNhanThanhToanNguonChuyenDoiModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaPhieu", maPhieu);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@LoaiPhieu", loaiPhieu);
                param.Add("@MaKhachHang", maKhachHang);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                entity = (await connection.QueryAsync<PhieuXacNhanThanhToanNguonChuyenDoiModel>(
                    "Proc_PhieuXacNhanThanhToan_PhieuNguonChuyenDoi",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<PhieuXacNhanThanhToanNguonChuyenDoiModel>();
                }
                int total = entity.FirstOrDefault()?.TotalCount ?? 0;

                return (entity, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách chi tiết nguồn chuyển đổi");
                entity = new List<PhieuXacNhanThanhToanNguonChuyenDoiModel>();
                return (entity, 0);
            }

        }
        #endregion

        #region Xác nhận phiếu thanh toán
        public async Task<ResultModel> XacNhanPhieuTTAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.KtPhieuXacNhanThanhToans.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy phiếu xác nhận thanh toán");
                entity.IsXacNhan = true;
                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xác nhận phiếu thanh toán {entity.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[XacNhanPhieuDKAsync] Lỗi khi xác nhận phiếu thanh toán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Tạo phiếu xác nhận thanh toán từ phiếu công nợ phải thu
        public Task<string> CreateDraftFromCNPTAsync(XNTTDraftDto draft, CancellationToken ct = default)
        {
            var draftId = Guid.NewGuid().ToString("N");

            var opts = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            _cache.Set($"XNTT_DRAFT_{draftId}", draft, opts);
            return Task.FromResult(draftId);
        }

        public Task<XNTTDraftDto?> GetDraftAsync(string draftId, CancellationToken ct = default)
        {
            _cache.TryGetValue($"XNTT_DRAFT_{draftId}", out XNTTDraftDto? draft);
            return Task.FromResult(draft);
        }

        public Task DeleteDraftAsync(string draftId, CancellationToken ct = default)
        {
            _cache.Remove($"XNTT_DRAFT_{draftId}");
            return Task.CompletedTask;
        }
        #endregion

    }
}

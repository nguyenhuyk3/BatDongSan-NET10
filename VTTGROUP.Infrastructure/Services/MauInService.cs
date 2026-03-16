using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.LoaiCanHo;
using VTTGROUP.Domain.Model.MauIn;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class MauInService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<MauInService> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ICurrentUserService _currentUser;
        private readonly string _connectionString;
        private readonly IBaseService _baseService;
        public MauInService(IDbContextFactory<AppDbContext> factory, ILogger<MauInService> logger, IConfiguration config, IMemoryCache cache, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _cache = cache;
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị toàn bộ danh sách danh mục thiết kế
        public async Task<(List<MauInPagingDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, int page, int pageSize, string? qSearch, string? trangThai, string fromDate, string toDate)
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

            var result = (await connection.QueryAsync<MauInPagingDto>(
                "Proc_MauIn_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa mẫu in
        public async Task<ResultModel> SaveMauInAsync(MauInModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var record = new HtMauIn
                {
                    MaMauIn = await SinhMaPhieuTuDongAsync("MIVB-", 5),
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    LoaiMauIn = model.LoaiMauIn ?? string.Empty,
                    TenMauIn = model.TenMauIn ?? string.Empty,
                    NoiDung = model.NoiDung
                };
                record.NgayLap = DateTime.Now;
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = model.MaMauIn ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "MauIn",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = string.Empty,
                            TenNhanVien = string.Empty,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }
                    await context.HtFileDinhKems.AddRangeAsync(listFiles);
                }
                await context.HtMauIns.AddAsync(record);
                await context.SaveChangesAsync();
                return ResultModel.Success("Thêm mẫu in thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm mẫu in");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm mẫu in: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(MauInModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await context.HtMauIns.FirstOrDefaultAsync(d => d.MaMauIn == model.MaMauIn);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy mẫu in.");
                }
                entity.TenMauIn = model.TenMauIn ?? string.Empty;
                entity.NoiDung = model.NoiDung ?? string.Empty;
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaMauIn && d.Controller == "MauIn").ToListAsync();

                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;

                        bool exists = UploadedFiles.Any(f =>
                            f.TenFileDinhKem == file.FileName &&
                            f.FileSize == file.FileSize
                        );
                        if (exists)
                            continue;

                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = model.MaMauIn,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "MauIn",
                            AcTion = "Edit",
                            NgayLap = DateTime.Now,
                            MaNhanVien = string.Empty,
                            TenNhanVien = string.Empty,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }
                    await context.HtFileDinhKems.AddRangeAsync(listFiles);
                }
                await context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenMauIn} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật mẫu in");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteMauInAsync(
     string maMauIn,
     string webRootPath,
     CancellationToken ct = default)
        {
            const string ControllerMauIn = "MauIn";

            try
            {
                if (string.IsNullOrWhiteSpace(maMauIn))
                    return ResultModel.Fail("Thiếu mã mẫu in.");

                if (string.IsNullOrWhiteSpace(webRootPath))
                    return ResultModel.Fail("Thiếu đường dẫn webRootPath.");

                var webRootFullPath = Path.GetFullPath(webRootPath);
                if (!Directory.Exists(webRootFullPath))
                    return ResultModel.Fail($"Thư mục webRootPath không tồn tại: {webRootFullPath}");

                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // ===== B1: lấy mẫu in =====
                var mauIn = await context.HtMauIns
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MaMauIn == maMauIn, ct);

                if (mauIn == null)
                    return ResultModel.Fail("Không tìm thấy mẫu in.");

                // ===== B2: kiểm tra ràng buộc =====
                // 2.1 Phiếu đặt cọc
                var dcCount = await context.BhPhieuDatCocs
                    .AsNoTracking()
                    .Where(x => x.MaMauIn == maMauIn)
                    .CountAsync(ct);

                // 2.2 Hợp đồng
                var hdCount = await context.KdHopDongs
                    .AsNoTracking()
                    .Where(x => x.MaMauIn == maMauIn)
                    .CountAsync(ct);

                if (dcCount > 0 || hdCount > 0)
                {
                    return ResultModel.Fail(
                        $"Không thể xoá vì mẫu in đang được sử dụng: {dcCount} phiếu đặt cọc, {hdCount} hợp đồng.");
                }

                // ===== B3: lấy danh sách file đính kèm (để lát nữa xóa vật lý) =====
                var fileRecords = await context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.Controller == ControllerMauIn && d.MaPhieu == maMauIn)
                    .ToListAsync(ct);

                // ===== B4: xóa DB trong transaction =====
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 4.1 xóa file đính kèm trong DB
                _ = await context.HtFileDinhKems
                    .Where(d => d.Controller == ControllerMauIn && d.MaPhieu == maMauIn)
                    .ExecuteDeleteAsync(ct);

                // 4.2 xóa mẫu in
                _ = await context.HtMauIns
                    .Where(d => d.MaMauIn == maMauIn)
                    .ExecuteDeleteAsync(ct);

                await tx.CommitAsync(ct);

                // ===== B5: xóa file vật lý sau khi DB đã commit =====
                var deletedFiles = 0;
                var failedFiles = new List<string>();

                foreach (var file in fileRecords)
                {
                    var rel = (file.TenFileDinhKemLuu ?? string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(rel))
                        continue;

                    var clean = rel.TrimStart('/', '\\');
                    var fullPath = Path.GetFullPath(Path.Combine(webRootFullPath, clean));

                    // chống path traversal
                    if (!fullPath.StartsWith(webRootFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Bỏ qua file ngoài webroot: {Path}", fullPath);
                        failedFiles.Add($"{rel} (ngoài webroot)");
                        continue;
                    }

                    try
                    {
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            deletedFiles++;
                        }
                    }
                    catch (Exception exDel)
                    {
                        _logger.LogWarning(exDel, "[DeleteMauInAsync] Không xoá được file: {Rel}", rel);
                        failedFiles.Add(rel);
                    }
                }

                var msg =
                    $"Xoá mẫu in \"{mauIn.TenMauIn}\" thành công. " +
                    (fileRecords.Count == 0
                        ? "Không có file đính kèm."
                        : $"Xoá file đính kèm: {deletedFiles}/{fileRecords.Count}.") +
                    (failedFiles.Count > 0
                        ? " File không xoá được: " + string.Join(", ", failedFiles.Take(5)) + (failedFiles.Count > 5 ? ", ..." : "")
                        : "");

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteMauInAsync] Lỗi khi xóa mẫu in {MaMauIn}", maMauIn);
                return ResultModel.Fail("Lỗi hệ thống, vui lòng thử lại sau.");
            }
        }


        public async Task<ResultModel> DeleteListAsync(
      List<MauInPagingDto> listMI,
      string webRootPath,
      CancellationToken ct = default)
        {
            const int BatchSize = 1800;
            const string ControllerMauIn = "MauIn";

            try
            {
                // B0) Normalize & guard
                var ids = (listMI ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaMauIn))
                    .Select(x => x!.MaMauIn.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                if (string.IsNullOrWhiteSpace(webRootPath))
                    return ResultModel.Fail("Thiếu đường dẫn webRootPath.");

                var webRootFullPath = Path.GetFullPath(webRootPath);
                if (!Directory.Exists(webRootFullPath))
                    return ResultModel.Fail($"Thư mục webRootPath không tồn tại: {webRootFullPath}");

                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // ===================================================
                // B1) KIỂM TRA RÀNG BUỘC: BH_PhieuDatCoc + KD_HopDong
                // map: MaMauIn -> (datCocCount, hopDongCount)
                // ===================================================
                var blockedMap = new Dictionary<string, (int datCoc, int hopDong)>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    // 1. Phiếu đặt cọc
                    var datCocCounts = await context.BhPhieuDatCocs
                        .AsNoTracking()
                        .Where(x => x.MaMauIn != null && chunk.Contains(x.MaMauIn!))
                        .GroupBy(x => x.MaMauIn!)
                        .Select(g => new { MaMauIn = g.Key, Count = g.Count() })
                        .TagWith("MauIn.DeleteListAsync: check BH_PhieuDatCoc by MaMauIn")
                        .ToListAsync(ct);

                    foreach (var row in datCocCounts)
                    {
                        if (blockedMap.TryGetValue(row.MaMauIn, out var cur))
                            blockedMap[row.MaMauIn] = (cur.datCoc + row.Count, cur.hopDong);
                        else
                            blockedMap[row.MaMauIn] = (row.Count, 0);
                    }

                    // 2. Hợp đồng
                    var hopDongCounts = await context.KdHopDongs
                        .AsNoTracking()
                        .Where(x => x.MaMauIn != null && chunk.Contains(x.MaMauIn!))
                        .GroupBy(x => x.MaMauIn!)
                        .Select(g => new { MaMauIn = g.Key, Count = g.Count() })
                        .TagWith("MauIn.DeleteListAsync: check KD_HopDong by MaMauIn")
                        .ToListAsync(ct);

                    foreach (var row in hopDongCounts)
                    {
                        if (blockedMap.TryGetValue(row.MaMauIn, out var cur))
                            blockedMap[row.MaMauIn] = (cur.datCoc, cur.hopDong + row.Count);
                        else
                            blockedMap[row.MaMauIn] = (0, row.Count);
                    }
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key).Select(kv =>
                        {
                            var (dc, hd) = kv.Value;
                            return $"- Mẫu in [{kv.Key}] đang được sử dụng: {dc} phiếu đặt cọc, {hd} hợp đồng.";
                        }));

                    return ResultModel.Fail("Không thể xoá: tất cả mẫu in được chọn đều đang được sử dụng.\n" + detail);
                }

                // ===================================================
                // B2) LẤY DANH SÁCH FILE ĐÍNH KÈM CỦA NHỮNG MẪU SẼ XOÁ
                // ===================================================
                var filePaths = new List<string>();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var files = await context.HtFileDinhKems
                        .AsNoTracking()
                        .Where(d => d.Controller == ControllerMauIn && chunk.Contains(d.MaPhieu))
                        .Select(d => d.TenFileDinhKemLuu ?? string.Empty)
                        .TagWith("MauIn.DeleteListAsync: load HtFileDinhKems by MaMauIn")
                        .ToListAsync(ct);

                    filePaths.AddRange(files);
                }

                filePaths = filePaths
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // ===================================================
                // B3) XOÁ TRONG DB (TRANSACTION)
                // ===================================================
                await using var tx = await context.Database.BeginTransactionAsync(ct);
                var totalDeleted = 0;

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    // 3.1) xoá file đính kèm trong DB
                    _ = await context.HtFileDinhKems
                        .Where(d => d.Controller == ControllerMauIn && chunk.Contains(d.MaPhieu))
                        .TagWith("MauIn.DeleteListAsync: delete HtFileDinhKems")
                        .ExecuteDeleteAsync(ct);

                    // 3.2) xoá mẫu in
                    var affected = await context.HtMauIns
                        .Where(m => chunk.Contains(m.MaMauIn))
                        .TagWith("MauIn.DeleteListAsync: delete HtMauIns")
                        .ExecuteDeleteAsync(ct);

                    totalDeleted += affected;
                }

                await tx.CommitAsync(ct);

                // ===================================================
                // B4) XOÁ FILE VẬT LÝ
                // ===================================================
                var deletedFiles = 0;
                var failedFiles = new List<string>();

                foreach (var relPath in filePaths)
                {
                    try
                    {
                        var clean = relPath.TrimStart('/', '\\');
                        var fullPath = Path.GetFullPath(Path.Combine(webRootFullPath, clean));

                        // chống path traversal
                        if (!fullPath.StartsWith(webRootFullPath, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Bỏ qua file ngoài webroot: {Path}", fullPath);
                            failedFiles.Add($"{relPath} (ngoài webroot)");
                            continue;
                        }

                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            deletedFiles++;
                        }
                    }
                    catch (Exception exDel)
                    {
                        _logger.LogWarning(exDel, "Không xoá được file: {RelPath}", relPath);
                        failedFiles.Add(relPath);
                    }
                }

                // ===================================================
                // B5) BUILD MESSAGE
                // ===================================================
                var skipped = ids.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} mẫu in. " +
                    (skipped > 0 ? $"{skipped} mẫu không xoá được (đang được sử dụng hoặc không tồn tại). " : "");

                var fileMsg = filePaths.Count == 0
                    ? "Không có file đính kèm."
                    : $"Xoá file đính kèm: {deletedFiles}/{filePaths.Count}.";

                var blockedDetail = "";
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv =>
                        {
                            var (dc, hd) = kv.Value;
                            return $"- Mẫu in [{kv.Key}] đang được sử dụng: {dc} phiếu đặt cọc, {hd} hợp đồng.";
                        });

                    blockedDetail = "\nChi tiết ràng buộc:\n" + string.Join(Environment.NewLine, top) +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} mẫu khác." : "");
                }

                var failedDetail = "";
                if (failedFiles.Count > 0)
                {
                    failedDetail = " Một số file không xoá được: " +
                                   string.Join(", ", failedFiles.Take(5)) +
                                   (failedFiles.Count > 5 ? ", ..." : "") + ".";
                }

                return ResultModel.Success(baseMsg + fileMsg + failedDetail + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[HtMauIns.DeleteListAsync] Lỗi khi xoá danh sách mẫu in");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách mẫu in.");
            }
        }

        // helper giữ nguyên
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }    
        #endregion

        #region Thông tin mẫu in 
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await (
                      from m in context.HtMauIns
                      join l in context.HtLoaiMauIns on m.LoaiMauIn equals l.MaLoaiMauIn
                      join da in context.DaDanhMucDuAns on m.MaDuAn equals da.MaDuAn
                      where m.MaMauIn == id
                      select new MauInModel
                      {
                          MaMauIn = m.MaMauIn,
                          LoaiMauIn = m.LoaiMauIn,
                          TenLoaiMauIn = l.TenLoaiMauIn,
                          TenMauIn = m.TenMauIn,
                          NoiDung = m.NoiDung,
                          TrangThaiDuyet = m.TrangThaiDuyet ?? 0,
                          MaQuiTrinhDuyet = m.MaQuiTrinhDuyet ?? 0,
                          NgayLap = m.NgayLap ?? DateTime.Now,
                          MaNhanVien = m.NguoiLap,
                          MaDuAn = m.MaDuAn,
                          TenDuAn = da.TenDuAn,
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new MauInModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaMauIn = await SinhMaPhieuTuDongAsync("MIVB-", 5);
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("MauIn", entity.MaMauIn);
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
                var files = await context.HtFileDinhKems.Where(d => d.Controller == "MauIn" && d.MaPhieu == id).Select(d => new UploadedFileModel
                {
                    Id = d.Id,
                    FileName = d.TenFileDinhKem,
                    FileNameSave = d.TenFileDinhKemLuu,
                    FileSize = d.FileSize,
                    ContentType = d.FileType
                }).ToListAsync();
                entity.Files = files;
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một loại thiết kế");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<List<DaDanhMucDuAn>> GetByDuAnAsync()
        {
            var entity = new List<DaDanhMucDuAn>();
            try
            {
                using var context = _factory.CreateDbContext();
                entity = await context.DaDanhMucDuAns.ToListAsync();
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

        public async Task<List<HtLoaiMauIn>> GetByLoaiMauInAsync()
        {
            var entity = new List<HtLoaiMauIn>();
            try
            {
                using var context = _factory.CreateDbContext();
                entity = await context.HtLoaiMauIns.ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtLoaiMauIn>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại mẫu in");
            }
            return entity;
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

        #region Hàm tăng tự động của mã mẫu in văn bảng
        public async Task<string> SinhMaPhieuTuDongAsync(string prefix, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.HtMauIns
                .Where(kh => kh.MaMauIn.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaMauIn)
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

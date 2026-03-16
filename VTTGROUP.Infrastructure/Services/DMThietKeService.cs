using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.DMLoaiThietKe;
using VTTGROUP.Domain.Model.Tang;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class DMThietKeService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<DMThietKeService> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ICurrentUserService _currentUser;
        public DMThietKeService(IDbContextFactory<AppDbContext> factory, ILogger<DMThietKeService> logger, IConfiguration config, IMemoryCache cache, ICurrentUserService currentUser)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _cache = cache;
            _currentUser = currentUser;
        }

        #region Hiển thị toàn bộ danh sách danh mục thiết kế
        public async Task<List<LoaiThietKeModel>> GetDanhMucThietKeAsync(string maDuAn, string qSearch = null)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var query = await (
                                 from ltk in context.DaDanhMucLoaiThietKes
                                 join duan in context.DaDanhMucDuAns on ltk.MaDuAn equals duan.MaDuAn into dtDong
                                 from duan2 in dtDong.DefaultIfEmpty()
                                 join nl in context.TblNhanviens on ltk.NguoiLap equals nl.MaNhanVien into dtNL
                                 from nl2 in dtNL.DefaultIfEmpty()
                                 where (string.IsNullOrEmpty(maDuAn) || ltk.MaDuAn == maDuAn)
                                 &&
                        (
                            string.IsNullOrEmpty(qSearch) ||
                            EF.Functions.Like(nl2.HoVaTen, $"%{qSearch}%") ||
                             EF.Functions.Like(ltk.NguoiLap, $"%{qSearch}%") ||
                            EF.Functions.Like(ltk.MaLoaiThietKe, $"%{qSearch}%") ||
                            EF.Functions.Like(ltk.TenLoaiThietKe, $"%{qSearch}%") ||
                            EF.Functions.Like(ltk.MaDuAn, $"%{qSearch}%") ||
                            EF.Functions.Like(duan2.TenDuAn, $"%{qSearch}%")
                        )
                                 select new LoaiThietKeModel
                                 {
                                     MaLoaiThietKe = ltk.MaLoaiThietKe,
                                     TenLoaiThietKe = ltk.TenLoaiThietKe,
                                     MaDuAn = ltk.MaDuAn,
                                     TenDuAn = duan2.TenDuAn,
                                     MoTa = ltk.MoTa,
                                     NgayLap = ltk.NgayLap,
                                     MaNhanVien = ltk.NguoiLap
                                 }).ToListAsync();
                return query;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách danh mục loại thiết kế");
            }
            return new List<LoaiThietKeModel>();
        }
        #endregion

        #region Thêm, xóa, sửa loại thiết kế
        public async Task<ResultModel> SaveThietKeAsync(LoaiThietKeModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var ltk = await context.DaDanhMucLoaiThietKes.FirstOrDefaultAsync(d => d.MaLoaiThietKe.ToLower() == model.MaLoaiThietKe.ToLower());
                if (ltk != null)
                    return ResultModel.Fail("DM thiết kế đã tồn tại.");

                var record = new DaDanhMucLoaiThietKe
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaLoaiThietKe = model.MaLoaiThietKe ?? string.Empty,
                    TenLoaiThietKe = model.TenLoaiThietKe ?? string.Empty,
                    MoTa = model.MoTa
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
                            MaPhieu = model.MaLoaiThietKe ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "DMThietKe",
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
                await context.DaDanhMucLoaiThietKes.AddAsync(record);
                await context.SaveChangesAsync();
                return ResultModel.Success("Thêm loại thiết kế thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm dự án");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm dự án: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(LoaiThietKeModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await context.DaDanhMucLoaiThietKes.FirstOrDefaultAsync(d => d.MaLoaiThietKe == model.MaLoaiThietKe);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy loại thiết kế.");
                }
                entity.MaDuAn = model.MaDuAn ?? string.Empty;
                entity.TenLoaiThietKe = model.TenLoaiThietKe ?? string.Empty;
                entity.MoTa = model.MoTa ?? string.Empty;
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaLoaiThietKe && d.Controller == "DMThietKe").ToListAsync();

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
                            MaPhieu = model.MaLoaiThietKe,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "DMThietKe",
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

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenLoaiThietKe} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật loại thiết kế");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        //public async Task<ResultModel> DeleteLoaiTKAsync(string maLoaiThietKe, string webRootPath)
        //{
        //    try
        //    {
        //        using var context = _factory.CreateDbContext();
        //        context.ChangeTracker.Clear();
        //        var ltk = await context.DaDanhMucLoaiThietKes.Where(d => d.MaLoaiThietKe == maLoaiThietKe).FirstOrDefaultAsync();
        //        if (ltk == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy loại thiết kế");
        //        }
        //        var listFiles = context.HtFileDinhKems.Where(d => d.Controller == "DMThietKe" && d.MaPhieu == ltk.MaLoaiThietKe);
        //        if (listFiles != null && listFiles.Any())
        //        {
        //            foreach (var file in listFiles)
        //            {
        //                var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
        //                if (File.Exists(fullPath)) File.Delete(fullPath);
        //            }

        //            context.HtFileDinhKems.RemoveRange(listFiles);
        //        }
        //        context.DaDanhMucLoaiThietKes.Remove(ltk);
        //        context.SaveChanges();
        //        return ResultModel.Success($"Xóa {ltk.TenLoaiThietKe} thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[DeleteLoaiTKAsync] Lỗi khi xóa loại thiết kế");
        //        return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> DeleteLoaiTKAsync(
    string maLoaiThietKe,
    string webRootPath,
    CancellationToken ct = default)
        {
            try
            {
                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // 1) Lấy thông tin tối thiểu + danh sách file (AsNoTracking để nhẹ hơn)
                var ltk = await context.DaDanhMucLoaiThietKes
                    .AsNoTracking()
                    .Where(d => d.MaLoaiThietKe == maLoaiThietKe)
                    .Select(d => new { d.MaLoaiThietKe, d.TenLoaiThietKe })
                    .SingleOrDefaultAsync(ct);

                if (ltk == null)
                    return ResultModel.Fail("Không tìm thấy loại thiết kế");

                var fileRelPaths = await context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.Controller == "DMThietKe" && d.MaPhieu == ltk.MaLoaiThietKe)
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync(ct);

                // (Tuỳ chọn) Chặn xoá nếu đang được tham chiếu ở nơi khác
                // var inUse = await context.DaThietKes.AsNoTracking()
                //     .AnyAsync(x => x.MaLoaiThietKe == maLoaiThietKe, ct);
                // if (inUse) return ResultModel.Fail("Loại thiết kế đang được sử dụng, không thể xoá.");

                // 2) Xoá DB trong transaction (EF Core 7/8: ExecuteDeleteAsync rất nhanh, không cần materialize entity)
                await using var tran = await context.Database.BeginTransactionAsync(ct);

                // Xoá các dòng file đính kèm
                await context.HtFileDinhKems
                    .Where(d => d.Controller == "DMThietKe" && d.MaPhieu == maLoaiThietKe)
                    .ExecuteDeleteAsync(ct);

                // Xoá loại thiết kế
                var affected = await context.DaDanhMucLoaiThietKes
                    .Where(d => d.MaLoaiThietKe == maLoaiThietKe)
                    .ExecuteDeleteAsync(ct);

                if (affected == 0)
                {
                    await tran.RollbackAsync(ct);
                    return ResultModel.Fail("Không xoá được loại thiết kế (có thể đã bị thay đổi).");
                }

                await tran.CommitAsync(ct);

                // 3) Sau khi DB commit thành công mới xoá file trên đĩa (best-effort)
                int deletedCount = 0, notFoundCount = 0, errorCount = 0;

                foreach (var relPath in fileRelPaths.Distinct())
                {
                    try
                    {
                        var fullPath = SafeCombineUnderRoot(webRootPath, relPath);
                        if (fullPath == null)
                        {
                            // Path không hợp lệ (traversal), bỏ qua nhưng log lại
                            _logger.LogWarning("[DeleteLoaiTKAsync] Bỏ qua path không an toàn: {Path}", relPath);
                            continue;
                        }

                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                            deletedCount++;
                        }
                        else
                        {
                            notFoundCount++;
                        }
                    }
                    catch (Exception exFile)
                    {
                        errorCount++;
                        _logger.LogWarning(exFile, "[DeleteLoaiTKAsync] Lỗi khi xoá file: {RelPath}", relPath);
                    }
                }

                var msg = $"Xoá \"{ltk.TenLoaiThietKe}\" thành công. "
                        + (fileRelPaths.Count > 0
                            ? $"Tệp: xoá {deletedCount}, không tìm thấy {notFoundCount}, lỗi {errorCount}."
                            : "Không có tệp đính kèm.");

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteLoaiTKAsync] Lỗi khi xoá loại thiết kế");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }

            // ===== Helper: chống path traversal & chuẩn hoá đường dẫn =====
            static string? SafeCombineUnderRoot(string root, string rel)
            {
                if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(rel))
                    return null;

                // Chuẩn hoá: loại bỏ leading '/' '\' để Path.Combine không coi như path tuyệt đối
                var trimmed = rel.TrimStart('/', '\\');

                // Nếu app đang lưu dạng "/uploads/abc.png" -> sau khi trim còn "uploads/abc.png"
                var combined = Path.GetFullPath(Path.Combine(root, trimmed));

                // Chỉ cho phép xoá trong webRootPath
                var rootFull = Path.GetFullPath(root);
                if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                    return null;

                return combined;
            }
        }

        public async Task<ResultModel> DeleteListAsync(
            List<LoaiThietKeModel> listLTK,
            string webRootPath,
            CancellationToken ct = default)
        {
            const int BatchSize = 1800;                   // an toàn < 2100 tham số
            const string ControllerLoaiThietKe = "DMThietKe";

            // --- Normalize & guard ---
            var ids = (listLTK ?? new())
                .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaLoaiThietKe))
                .Select(x => x!.MaLoaiThietKe.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
                return ResultModel.Success("Không có dòng nào được chọn để xoá.");

            if (string.IsNullOrWhiteSpace(webRootPath))
                return ResultModel.Fail("Thiếu đường dẫn webRootPath.");

            // Chuẩn hoá webroot và thêm dấu phân cách để so sánh StartsWith an toàn
            var webRootFullPath = Path.GetFullPath(webRootPath);
            if (!Directory.Exists(webRootFullPath))
                return ResultModel.Fail($"Thư mục webRootPath không tồn tại: {webRootFullPath}");

            if (!webRootFullPath.EndsWith(Path.DirectorySeparatorChar))
                webRootFullPath += Path.DirectorySeparatorChar;

            try
            {
                await using var context = _factory.CreateDbContext();

                // ============ B1) KIỂM TRA RÀNG BUỘC: Loại thiết kế -> Loại căn hộ ============
                // map: MaLoaiThietKe -> (loaiCanHoCount)
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var counts = await context.DaDanhMucLoaiCanHos.AsNoTracking()
                        .Where(x => chunk.Contains(x.MaLoaiThietKe))
                        .GroupBy(x => x.MaLoaiThietKe)
                        .Select(g => new { MaLoaiThietKe = g.Key!, Count = g.Count() })
                        .TagWith("DeleteListAsync: Check DaDanhMucLoaiCanHos by MaLoaiThietKe")
                        .ToListAsync(ct);

                    foreach (var row in counts)
                        blockedMap[row.MaLoaiThietKe] = (blockedMap.TryGetValue(row.MaLoaiThietKe, out var cur) ? cur : 0) + row.Count;
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- Loại thiết kế [{kv.Key}] đang được sử dụng: {kv.Value} loại căn hộ.")
                    );
                    return ResultModel.Fail("Không thể xoá: tất cả Loại thiết kế được chọn đều đang được sử dụng.\n" + detail);
                }

                // ============ B2) LẤY DANH SÁCH FILE ĐÍNH KÈM (lấy trước khi xoá DB) ============
                var filesToDelete = new List<string>();
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var files = await context.HtFileDinhKems.AsNoTracking()
                        .Where(d => d.Controller == ControllerLoaiThietKe && chunk.Contains(d.MaPhieu))
                        .Select(d => d.TenFileDinhKemLuu ?? "")
                        .TagWith("DeleteListAsync: Load HtFileDinhKems for deletable MaLoaiThietKe")
                        .ToListAsync(ct);

                    filesToDelete.AddRange(files
                        .Where(p => !string.IsNullOrWhiteSpace(p))
                        .Select(p => p.Trim()));
                }

                // Loại trùng để không xoá 2 lần
                filesToDelete = filesToDelete.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

                // ============ B3) XOÁ DB TRONG TRANSACTION ============
                await using var tx = await context.Database.BeginTransactionAsync(ct);
                var totalDeleted = 0;

                // 3.1) Xoá bản ghi file đính kèm
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await context.HtFileDinhKems
                        .Where(d => d.Controller == ControllerLoaiThietKe && chunk.Contains(d.MaPhieu))
                        .TagWith("DeleteListAsync: ExecuteDelete HtFileDinhKems by MaPhieu")
                        .ExecuteDeleteAsync(ct);
                }

                // 3.2) Xoá loại thiết kế
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var affected = await context.DaDanhMucLoaiThietKes
                        .Where(t => chunk.Contains(t.MaLoaiThietKe))
                        .TagWith("DeleteListAsync: ExecuteDelete DaDanhMucLoaiThietKes")
                        .ExecuteDeleteAsync(ct);

                    totalDeleted += affected;
                }

                await tx.CommitAsync(ct);

                // ============ B4) XOÁ FILE TRÊN Ổ ĐĨA (I/O sau khi DB đã chắc chắn) ============
                int deletedFiles = 0;
                var failedFiles = new List<string>();

                foreach (var rel in filesToDelete)
                {
                    var safeRel = rel.TrimStart('/', '\\');
                    var fullPath = Path.GetFullPath(Path.Combine(webRootFullPath, safeRel));

                    // Chống path traversal
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
                        else
                        {
                            _logger.LogInformation("File không tồn tại trên đĩa: {Path}", fullPath);
                        }
                    }
                    catch (Exception ioEx)
                    {
                        _logger.LogError(ioEx, "Lỗi xoá file: {Path}", fullPath);
                        failedFiles.Add(rel);
                    }
                }

                // ============ B5) THÔNG ĐIỆP ============
                var skipped = ids.Count - totalDeleted; // vướng ràng buộc hoặc không tồn tại
                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} Loại thiết kế. " +
                    (skipped == 0 ? "" : $"{skipped} Loại thiết kế không xoá (đang được sử dụng hoặc không tồn tại). ");

                var fileMsg = filesToDelete.Count == 0
                    ? "Không có file đính kèm."
                    : $"Xoá file đính kèm: {deletedFiles}/{filesToDelete.Count}.";

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var topDetail = blockedMap
                        .OrderBy(kv => kv.Key)
                        .Take(10)
                        .Select(kv => $"- Loại thiết kế [{kv.Key}] đang được sử dụng: {kv.Value} loại căn hộ.");

                    blockedDetail = $"\nChi tiết Loại thiết kế bị ràng buộc:\n{string.Join(Environment.NewLine, topDetail)}" +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} loại khác." : "");
                }

                string failedFileDetail = string.Empty;
                if (failedFiles.Count > 0)
                {
                    var preview = string.Join(", ", failedFiles.Take(5));
                    failedFileDetail = $" Một số file không xoá được: {preview}" +
                                       (failedFiles.Count > 5 ? ", ..." : "") + ".";
                }

                return ResultModel.Success(baseMsg + fileMsg + failedFileDetail + blockedDetail);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteListAsync bị huỷ. Count={Count}", ids.Count);
                return ResultModel.Fail("Tác vụ đã bị huỷ.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteListAsync] Lỗi ràng buộc khi xoá danh sách Loại thiết kế. Count={Count}", ids.Count);
                return ResultModel.Fail("Không thể xoá vì Loại thiết kế đang được sử dụng (ràng buộc dữ liệu). Vui lòng gỡ liên kết/loại căn hộ trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách Loại thiết kế. Count={Count}", ids.Count);
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách Loại thiết kế.");
            }
        }

        // Helper chunk nếu bạn chưa có
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin loại thiết kế
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await (
                      from ltk in context.DaDanhMucLoaiThietKes
                      join duan in context.DaDanhMucDuAns on ltk.MaDuAn equals duan.MaDuAn
                      where ltk.MaLoaiThietKe == id
                      select new LoaiThietKeModel
                      {
                          MaDuAn = ltk.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaLoaiThietKe = ltk.MaLoaiThietKe,
                          TenLoaiThietKe = ltk.TenLoaiThietKe,
                          MoTa = ltk.MoTa,
                          MaNhanVien = ltk.NguoiLap,
                          NgayLap = ltk.NgayLap
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new LoaiThietKeModel();
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                }
                var files = await context.HtFileDinhKems.Where(d => d.Controller == "DMThietKe" && d.MaPhieu == id).Select(d => new UploadedFileModel
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
    }
}

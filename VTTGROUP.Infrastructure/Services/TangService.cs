using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.Block;
using VTTGROUP.Domain.Model.Tang;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class TangService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<TangService> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ICurrentUserService _currentUser;
        public TangService(IDbContextFactory<AppDbContext> factory, ILogger<TangService> logger, IConfiguration config, IMemoryCache cache, ICurrentUserService currentUser)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _cache = cache;
            _currentUser = currentUser;
        }

        #region Hiển thị toàn bộ tầng     
        public async Task<List<TangModel>> GetTangAsync(string maDuAn, string maBlock, string qSearch = null)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                qSearch = qSearch?.Trim();

                var query =
                    from tang in context.DaDanhMucTangs.AsNoTracking()
                    join block in context.DaDanhMucBlocks.AsNoTracking() on tang.MaBlock equals block.MaBlock into dtDong
                    from block2 in dtDong.DefaultIfEmpty()
                    where
                        (string.IsNullOrEmpty(maBlock) || tang.MaBlock == maBlock) &&
                        (string.IsNullOrEmpty(maDuAn) || tang.MaDuAn == maDuAn) &&
                        (
                            string.IsNullOrEmpty(qSearch) ||
                            EF.Functions.Like(tang.MaTang, $"%{qSearch}%") ||
                            EF.Functions.Like(tang.TenTang, $"%{qSearch}%") ||
                            EF.Functions.Like(tang.MaBlock, $"%{qSearch}%") ||
                            EF.Functions.Like(block2.TenBlock, $"%{qSearch}%")
                        )
                    select new TangModel
                    {
                        MaTang = tang.MaTang,
                        TenTang = tang.TenTang,
                        MaBlock = tang.MaBlock,
                        TenBlock = block2.TenBlock,
                        HeSo = tang.HeSoTang??1,
                        STTTang = tang.Stttang??1
                    };

                // Sắp xếp nếu cần
                query = query.OrderBy(x => x.MaBlock).ThenBy(x => x.STTTang);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Tầng");
                return new List<TangModel>();
            }
        }

        #endregion

        #region Thêm, xóa, sửa tầng
        public async Task<ResultModel> SaveTangAsync(TangModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var tang = await context.DaDanhMucTangs.FirstOrDefaultAsync(d => d.MaTang.ToLower() == model.MaTang.ToLower());
                if (tang != null)
                    return ResultModel.Fail("Tầng đã tồn tại.");

                var record = new DaDanhMucTang
                {
                    MaTang = model.MaTang ?? string.Empty,
                    TenTang = model.TenTang ?? string.Empty,
                    MaDuAn = model.MaDuAn,
                    MaBlock = model.MaBlock ?? string.Empty,
                    HeSoTang = model.HeSo,
                    Stttang = model.STTTang,
                    HinhAnh = model.HinhAnh != null && model.HinhAnh.Any() ? model.HinhAnh[0].FullDomain : string.Empty,
                };

                await context.DaDanhMucTangs.AddAsync(record);


                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                if (model.HinhAnh != null && model.HinhAnh.Any())
                {
                    foreach (var file in model.HinhAnh)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = record.MaTang,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "LoTang",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                    if (listFiles.Any())
                        await context.HtFileDinhKems.AddRangeAsync(listFiles);
                }

                await context.SaveChangesAsync();
                return ResultModel.Success("Thêm tầng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm tầng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm tầng: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(TangModel model, string webRootPath)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await context.DaDanhMucTangs.FirstOrDefaultAsync(d => d.MaTang == model.MaTang);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy tầng.");
                }
                // entity.MaBlock = model.MaBlock ?? string.Empty;
                entity.TenTang = model.TenTang ?? string.Empty;
                entity.HeSoTang = model.HeSo;
                entity.Stttang = model.STTTang;

                if (!string.IsNullOrEmpty(entity.HinhAnh) && model.HinhAnh.Any())
                {
                    if (entity.HinhAnh != model.HinhAnh[0].FullDomain) //Có sự thay đổi file mới cần xóa file cũ
                    {
                        var f = context.HtFileDinhKems.Where(d => d.Controller == "LoTang" && d.MaPhieu == entity.MaTang && d.FullDomain == entity.HinhAnh);
                        if (f != null && f.Any())
                        {
                            foreach (var file in f)
                            {
                                var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                                if (File.Exists(fullPath)) File.Delete(fullPath);
                            }

                            context.HtFileDinhKems.RemoveRange(f);
                        }
                    }
                }
                entity.HinhAnh = model.HinhAnh != null && model.HinhAnh.Any() ? model.HinhAnh[0].FullDomain : string.Empty;

                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaTang && d.Controller == "LoTang"
                && d.FullDomain == entity.HinhAnh
                ).ToListAsync();

                if (model.HinhAnh != null && model.HinhAnh.Any())
                {
                    foreach (var file in model.HinhAnh)
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
                            MaPhieu = model.MaTang,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "LoTang",
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

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenTang} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteTangAsync(
     string maTang,
     string webRootPath,
     CancellationToken ct = default)
        {
            const string ControllerLoTang = "LoTang";

            try
            {
                // --- Guard clauses ---
                if (string.IsNullOrWhiteSpace(maTang))
                    return ResultModel.Fail("Thiếu mã tầng.");

                if (string.IsNullOrWhiteSpace(webRootPath))
                    return ResultModel.Fail("Thiếu đường dẫn webRootPath.");

                maTang = maTang.Trim();
                var webRootFullPath = Path.GetFullPath(webRootPath);

                if (!Directory.Exists(webRootFullPath))
                    return ResultModel.Fail($"Thư mục webRootPath không tồn tại: {webRootFullPath}");

                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // --- Tìm entity ---
                var tang = await context.DaDanhMucTangs
                    .FirstOrDefaultAsync(d => d.MaTang == maTang, ct);

                if (tang == null)
                    return ResultModel.Fail("Không tìm thấy tầng.");

                // (Tùy chọn) Kiểm tra ràng buộc tham chiếu trước khi xoá để tránh lỗi FK.
                // Ví dụ:
                bool hasRefProducts = await context.DaDanhMucSanPhams.AnyAsync(x => x.MaTang == maTang && x.MaDuAn == tang.MaDuAn, ct);
                if (hasRefProducts) return ResultModel.Fail("Tầng đang được tham chiếu bởi sản phẩm, không thể xóa.");

                // --- Lấy danh sách file đính kèm theo đúng controller ---
                var files = await context.HtFileDinhKems
                    .Where(d => d.Controller == ControllerLoTang && d.MaPhieu == maTang)
                    .ToListAsync(ct);

                var deletedFiles = 0;
                var failedFiles = new List<string>();

                // --- Xoá file trên ổ đĩa (không giao dịch được, nên làm trước & ghi nhận cảnh báo) ---
                foreach (var f in files)
                {
                    var relative = (f.TenFileDinhKemLuu ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(relative))
                        continue;

                    // Chuẩn hoá và chống path traversal
                    var safeRelative = relative.TrimStart('/', '\\');
                    var fullPath = Path.GetFullPath(Path.Combine(webRootFullPath, safeRelative));

                    // Chỉ cho phép xóa trong webroot
                    if (!fullPath.StartsWith(webRootFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Bỏ qua file ngoài webroot: {Path}", fullPath);
                        failedFiles.Add($"{relative} (ngoài webroot)");
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
                            // Không xem là lỗi nghiêm trọng – chỉ log “không tìm thấy”
                            _logger.LogInformation("File không tồn tại trên đĩa: {Path}", fullPath);
                        }
                    }
                    catch (Exception ioEx)
                    {
                        _logger.LogError(ioEx, "Lỗi xoá file: {Path}", fullPath);
                        failedFiles.Add(relative);
                    }
                }

                // --- Giao dịch DB: xoá bản ghi file + tầng ---
                await using var tx = await context.Database.BeginTransactionAsync(ct);
                try
                {
                    if (files.Count > 0)
                        context.HtFileDinhKems.RemoveRange(files);

                    context.DaDanhMucTangs.Remove(tang);

                    var affected = await context.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    // --- Kết quả thân thiện ---
                    var okMsg = $"Đã xoá tầng \"{tang.TenTang}\" (MaTang={tang.MaTang}). " +
                                $"Xoá {deletedFiles}/{files.Count} file đính kèm.";

                    if (failedFiles.Count > 0)
                    {
                        var preview = string.Join(", ", failedFiles.Take(5));
                        okMsg += $" Một số file không xoá được: {preview}" +
                                 (failedFiles.Count > 5 ? ", ..." : "") + ".";
                    }

                    return ResultModel.Success(okMsg);
                }
                catch (DbUpdateException dbEx)
                {
                    await tx.RollbackAsync(ct);
                    _logger.LogError(dbEx, "[DeleteTangAsync] Lỗi DB khi xoá tầng {MaTang}", maTang);

                    // Gợi ý nguyên nhân thường gặp: khoá ngoại
                    return ResultModel.Fail("Không thể xoá tầng do ràng buộc dữ liệu (ví dụ đang được tham chiếu).");
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync(ct);
                    _logger.LogError(ex, "[DeleteTangAsync] Lỗi khi xoá tầng {MaTang}", maTang);
                    return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                return ResultModel.Fail("Tác vụ đã bị huỷ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteTangAsync] Lỗi không xác định");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(
     List<TangModel> listTang,
     string webRootPath,
     CancellationToken ct = default)
        {
            const int BatchSize = 1800;            // an toàn < 2100 tham số
            const string ControllerLoTang = "LoTang";

            // --- Normalize & guard ---
            var ids = (listTang ?? new())
                .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaTang))
                .Select(x => x!.MaTang.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
                return ResultModel.Success("Không có dòng nào được chọn để xoá.");

            if (string.IsNullOrWhiteSpace(webRootPath))
                return ResultModel.Fail("Thiếu đường dẫn webRootPath.");

            var webRootFullPath = Path.GetFullPath(webRootPath);
            if (!Directory.Exists(webRootFullPath))
                return ResultModel.Fail($"Thư mục webRootPath không tồn tại: {webRootFullPath}");

            try
            {
                await using var context = _factory.CreateDbContext();

                // ============ B1) KIỂM TRA RÀNG BUỘC SẢN PHẨM/CĂN HỘ ============
                // map: MaTang -> (tangCount, canHoCount). Ở đây chỉ đếm căn hộ, tangCount = 0 (để tương thích format cũ).
                var blockedMap = new Dictionary<string, (int tang, int canHo)>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var canHoCounts = await context.DaDanhMucSanPhams.AsNoTracking()
                        .Where(sp => chunk.Contains(sp.MaTang))
                        .GroupBy(sp => sp.MaTang)
                        .Select(g => new { MaTang = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync: Check DaDanhMucSanPhams by MaTang")
                        .ToListAsync(ct);

                    foreach (var row in canHoCounts)
                    {
                        if (!blockedMap.TryGetValue(row.MaTang, out var cur))
                            blockedMap[row.MaTang] = (0, row.Count);
                        else
                            blockedMap[row.MaTang] = (cur.tang, cur.canHo + row.Count);
                    }
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var lines = blockedMap
                        .OrderBy(kv => kv.Key)
                        .Select(kv =>
                        {
                            var (tang, canHo) = kv.Value;
                            return $"- Tầng [{kv.Key}] đang được sử dụng: {tang} tầng, {canHo} căn hộ.";
                        });

                    var detail = string.Join(Environment.NewLine, lines);
                    return ResultModel.Fail(
                        "Không thể xoá: tất cả Tầng được chọn đều đang được sử dụng.\n" + detail);
                }

                // ============ B2) XOÁ FILE TRÊN Ổ ĐĨA (I/O – làm TRƯỚC, không giao dịch) ============
                // Chỉ cần lưu relative path (đủ để xoá). Tránh lỗi tuple type mismatch.
                var filesToDelete = new List<string>();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var files = await context.HtFileDinhKems.AsNoTracking()
                        .Where(d => d.Controller == ControllerLoTang && chunk.Contains(d.MaPhieu))
                        .Select(d => d.TenFileDinhKemLuu)
                        .TagWith("DeleteListAsync: Load HtFileDinhKems for deletable MaTang")
                        .ToListAsync(ct);

                    filesToDelete.AddRange(
                        files.Select(p => (p ?? string.Empty).Trim())
                    );
                }

                int deletedFiles = 0;
                var failedFiles = new List<string>();

                foreach (var relRaw in filesToDelete)
                {
                    if (string.IsNullOrWhiteSpace(relRaw))
                        continue;

                    // Chống path traversal
                    var safeRelative = relRaw.TrimStart('/', '\\');
                    var fullPath = Path.GetFullPath(Path.Combine(webRootFullPath, safeRelative));

                    if (!fullPath.StartsWith(webRootFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("Bỏ qua file ngoài webroot: {Path}", fullPath);
                        failedFiles.Add($"{relRaw} (ngoài webroot)");
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
                        failedFiles.Add(relRaw);
                    }
                }

                // ============ B3) XOÁ DB TRONG TRANSACTION ============
                await using var tx = await context.Database.BeginTransactionAsync(ct);
                var totalDeleted = 0;

                // 3.1) Xoá bản ghi file đính kèm
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await context.HtFileDinhKems
                        .Where(d => d.Controller == ControllerLoTang && chunk.Contains(d.MaPhieu))
                        .TagWith("DeleteListAsync: ExecuteDelete HtFileDinhKems by MaPhieu")
                        .ExecuteDeleteAsync(ct);
                }

                // 3.2) Xoá tầng
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var affected = await context.DaDanhMucTangs
                        .Where(t => chunk.Contains(t.MaTang))
                        .TagWith("DeleteListAsync: ExecuteDelete DaDanhMucTangs")
                        .ExecuteDeleteAsync(ct);

                    totalDeleted += affected;
                }

                await tx.CommitAsync(ct);

                // ============ B4) THÔNG ĐIỆP ============
                var skipped = ids.Count - totalDeleted; // vướng ràng buộc hoặc không tồn tại
                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} Tầng. " +
                    (skipped == 0
                        ? ""
                        : $"{skipped} Tầng không xoá (đang được sử dụng hoặc không tồn tại). ");

                var fileMsg =
                    filesToDelete.Count == 0
                        ? "Không có file đính kèm."
                        : $"Xoá file đính kèm: {deletedFiles}/{filesToDelete.Count}.";

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var topDetail = blockedMap
                        .OrderBy(kv => kv.Key)
                        .Take(10)
                        .Select(kv =>
                        {
                            var (tang, canHo) = kv.Value;
                            return $"- Tầng [{kv.Key}] đang được sử dụng: {tang} tầng, {canHo} căn hộ.";
                        });

                    blockedDetail = $"\nChi tiết Tầng bị ràng buộc:\n{string.Join(Environment.NewLine, topDetail)}" +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} tầng khác." : "");
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
                _logger.LogError(dbEx, "[DeleteListAsync] Lỗi ràng buộc khi xoá danh sách Tầng. Count={Count}", ids.Count);
                return ResultModel.Fail("Không thể xoá vì Tầng đang được sử dụng (ràng buộc dữ liệu). Vui lòng gỡ liên kết/căn hộ trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách Tầng. Count={Count}", ids.Count);
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách Tầng.");
            }
        }

        /// <summary>
        /// Helper: chia mảng theo batch an toàn tham số SQL.
        /// </summary>
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return new List<T>(source.Skip(i).Take(size));
        }

        #endregion

        #region Thông tin tầng
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await (
                      from tang in context.DaDanhMucTangs
                      join da in context.DaDanhMucDuAns on tang.MaDuAn equals da.MaDuAn into dtDA
                      from da2 in dtDA.DefaultIfEmpty()
                      join block in context.DaDanhMucBlocks on tang.MaBlock equals block.MaBlock into dtDong
                      from block2 in dtDong.DefaultIfEmpty()
                      where tang.MaTang == id
                      select new TangModel
                      {
                          MaTang = tang.MaTang,
                          TenTang = tang.TenTang,
                          MaDuAn = tang.MaDuAn,
                          TenDuAn = da2.TenDuAn,
                          MaBlock = tang.MaBlock,
                          TenBlock = block2.TenBlock,
                          HeSo = tang.HeSoTang??1,
                          STTTang = tang.Stttang??1
                      }).FirstOrDefaultAsync();

                var files = await context.HtFileDinhKems.Where(d => d.Controller == "LoTang" && d.MaPhieu == entity.MaTang
                    ).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType,
                        FullDomain = d.FullDomain,
                    }).ToListAsync();

                entity.HinhAnh = files;
                if (entity == null)
                {
                    entity = new TangModel();
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một tầng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Get tầng theo dự án
        public async Task<List<DaDanhMucTang>> GetTangsByMaDuAnAsync(string maDuAn)
        {

            string cacheKey = $"Tangs_{maDuAn}";
            var entity = new List<DaDanhMucTang>();
            //try
            //{
            //    using var context = _factory.CreateDbContext();
            //    entity = await context.DaDanhMucTangs
            //                             .Where(d => d.MaDuAn == maDuAn)
            //                             .ToListAsync();

            //    if (entity == null)
            //        entity = new List<DaDanhMucTang>();
            //    return entity;
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "[GetTangsByMaDuAnAsync] Lỗi khi lấy toàn bộ danh sách tầng theo dự án");
            //}
            return entity;
        }

        public async Task<List<DaDanhMucBlock>> GetByBlockAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucBlock>();
            try
            {
                using var context = _factory.CreateDbContext();
                entity = await context.DaDanhMucBlocks.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucBlock>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách block");
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

        #region Import, download file mẫu        
        public async Task<byte[]> GenerateTemplateWithDataAsync(string templatePath, string maDuAn, CancellationToken ct = default)
        {
            using var _context = _factory.CreateDbContext();
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);

            var blockSheet = workbook.Worksheet("Block");
            var listBlock = await _context.DaDanhMucBlocks.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            int row = 2;
            foreach (var item in listBlock)
            {
                blockSheet.Cell(row, 1).Value = item.MaBlock;
                blockSheet.Cell(row, 2).Value = item.TenBlock;
                row++;
            }
            // Set lại sheet "SanPham" là active
            workbook.Worksheet("Tang").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }

        public async Task<ResultModel> ImportFromExcelAsync(
      IBrowserFile file,
      string maDuAn,
      CancellationToken ct = default)
        {
            try
            {
                if (file is null) return ResultModel.Fail("File trống.");
                if (string.IsNullOrWhiteSpace(maDuAn)) return ResultModel.Fail("Thiếu mã dự án.");

                await using var _context = _factory.CreateDbContext();

                // 1) Đọc file vào memory stream
                await using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await inputStream.CopyToAsync(memoryStream, ct);
                memoryStream.Position = 0;

                // 2) Đọc dữ liệu từ Excel (CHỈ đọc, không đụng DB)
                var items = ReadTangFromExcel(memoryStream, maDuAn);

                // 3) Làm sạch & chuẩn hoá
                items = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaBlock)
                             && !string.IsNullOrWhiteSpace(x.MaTang)
                             && !string.IsNullOrWhiteSpace(x.TenTang))
                    .Select(x =>
                    {
                        x.MaBlock = x.MaBlock!.Trim().ToUpperInvariant(); // so sánh mã block không phân biệt hoa/thường
                        x.MaTang = x.MaTang!.Trim();
                        x.TenTang = x.TenTang!.Trim();
                        x.MaDuAn = maDuAn;
                        x.HeSoTang ??= 1;
                        x.STTTang ??= 1;
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // 4) Thống kê trùng ngay trong file (theo MaTang, không phân biệt hoa/thường)
                var duplicateInFile = items
                    .GroupBy(x => x.MaTang!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                // Giữ bản đầu tiên cho mỗi MaTang (bỏ trùng nội bộ file)
                items = items
                    .GroupBy(x => x.MaTang!, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                // 5) Kiểm tra MaBlock tồn tại theo MaDuAn (STRICT: nếu thiếu → fail)
                var existingBlocks = new HashSet<string>(
                    await _context.DaDanhMucBlocks
                        .AsNoTracking()
                        .Where(b => b.MaDuAn == maDuAn && b.MaBlock != null)
                        .Select(b => b.MaBlock!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                var invalidByBlock = items.Where(x => !existingBlocks.Contains(x.MaBlock!)).ToList();
                if (invalidByBlock.Count > 0)
                {
                    var preview = string.Join(", ",
                        invalidByBlock.Select(x => x.MaBlock).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                    return ResultModel.Fail(
                        $"Có {invalidByBlock.Count} dòng có MaBlock không tồn tại trong dự án '{maDuAn}'. " +
                        $"Một số mã: {preview}{(invalidByBlock.Count > 10 ? ", ..." : "")}");
                }

                // 6) Lấy các MaTang đã tồn tại TRONG TOÀN HỆ THỐNG (system-wide)
                var existingAllTangSet = new HashSet<string>(
                    await _context.DaDanhMucTangs
                        .AsNoTracking()
                        .Where(t => t.MaTang != null)
                        .Select(t => t.MaTang!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                // 6b) (Tuỳ chọn) Lấy các MaTang đã tồn tại theo dự án để báo cáo dễ hiểu
                var existingTangInProject = new HashSet<string>(
                    await _context.DaDanhMucTangs
                        .AsNoTracking()
                        .Where(t => t.MaDuAn == maDuAn && t.MaTang != null)
                        .Select(t => t.MaTang!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                // 7) Phân loại: trùng trong DB (toàn hệ thống) vs thực sự mới
                var duplicatesInDbSystem = items.Where(x => existingAllTangSet.Contains(x.MaTang!)).ToList();
                var newItems = items.Where(x => !existingAllTangSet.Contains(x.MaTang!)).ToList();

                if (newItems.Count == 0)
                {
                    // Ưu tiên hiện ví dụ trong phạm vi dự án cho dễ hiểu
                    var previewProj = string.Join(", ",
                        duplicatesInDbSystem.Where(x => existingTangInProject.Contains(x.MaTang!))
                            .Select(x => x.MaTang).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));

                    // Nếu trong dự án không có ví dụ, lấy đại từ system
                    var previewSystem = string.Join(", ",
                        duplicatesInDbSystem.Select(x => x.MaTang).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));

                    var preview = string.IsNullOrWhiteSpace(previewProj) ? previewSystem : previewProj;

                    return ResultModel.Fail(
                        $"Tất cả MaTang trong file đều đã tồn tại trong hệ thống. " +
                        (duplicatesInDbSystem.Count > 0
                            ? $"Ví dụ: {preview}{(duplicatesInDbSystem.Count > 10 ? ", ..." : "")}"
                            : ""));
                }

                // 8) Thêm hàng loạt (chỉ những mã mới system-wide)
                var entities = newItems.Select(item => new DaDanhMucTang
                {
                    MaBlock = item.MaBlock,
                    MaTang = item.MaTang,
                    TenTang = item.TenTang,
                    HeSoTang = item.HeSoTang,
                    Stttang = item.STTTang,
                    MaDuAn = item.MaDuAn
                }).ToList();

                await _context.DaDanhMucTangs.AddRangeAsync(entities, ct);
                var affected = await _context.SaveChangesAsync(ct);

                // 9) Ghép message thân thiện
                var msg = $"Import thành công {entities.Count} tầng (đã ghi {affected} bản ghi).";
                if (duplicatesInDbSystem.Count > 0)
                {
                    var dupDbPreview = string.Join(", ",
                        duplicatesInDbSystem.Select(x => x.MaTang).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                    msg += $" Bỏ qua {duplicatesInDbSystem.Count} dòng vì MaTang đã tồn tại trong DB (toàn hệ thống)"
                           + (duplicatesInDbSystem.Count > 10 ? $" (ví dụ: {dupDbPreview}, ...)." : $" (ví dụ: {dupDbPreview}).");
                }
                if (duplicateInFile.Count > 0)
                {
                    var dupFilePreview = string.Join(", ", duplicateInFile.Take(10));
                    msg += $" Trong file cũng có {duplicateInFile.Count} mã bị lặp nội bộ"
                         + (duplicateInFile.Count > 10 ? $" (ví dụ: {dupFilePreview}, ...)." : $" (ví dụ: {dupFilePreview}).");
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi import Excel");
                return ResultModel.Fail($"Lỗi import: {ex.Message}");
            }
        }
        public List<TangImportModel> ReadTangFromExcel(Stream stream, string maDuAn)
        {
            // CHỈ đọc Excel, không dùng DbContext ở đây
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });

            var table = dataSet.Tables["Tang"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'Tang' trong file Excel.");

            var list = new List<TangImportModel>();

            foreach (DataRow r in table.Rows)
            {
                // Cột: 0=MaBlock, 1=MaTang, 2=TenTang, 3=HeSoTang, 4=STTTang
                string? maBlock = r[0]?.ToString()?.Trim();
                string? maTang = r[1]?.ToString()?.Trim();
                string? tenTang = r[2]?.ToString()?.Trim();

                decimal? heSoTang = null;
                if (decimal.TryParse(r[3]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hst))
                    heSoTang = hst;

                int? sttTang = null;
                if (int.TryParse(r[4]?.ToString()?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stt))
                    sttTang = stt;

                // Nếu cả 3 đều rỗng → bỏ qua dòng trống
                if (string.IsNullOrWhiteSpace(maBlock)
                 && string.IsNullOrWhiteSpace(maTang)
                 && string.IsNullOrWhiteSpace(tenTang))
                    continue;

                list.Add(new TangImportModel
                {
                    MaBlock = maBlock,
                    MaTang = maTang,
                    TenTang = tenTang,
                    HeSoTang = heSoTang ?? 1,
                    STTTang = sttTang ?? 1,
                    MaDuAn = maDuAn
                });
            }

            return list;
        }
        #endregion
    }
}

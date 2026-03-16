using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.MauIn;
using VTTGROUP.Domain.Model.Template;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class TemplateService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<TemplateService> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ICurrentUserService _currentUser;
        private readonly string _connectionString;
        private readonly IBaseService _baseService;
        public TemplateService(IDbContextFactory<AppDbContext> factory, ILogger<TemplateService> logger, IConfiguration config, IMemoryCache cache, ICurrentUserService currentUser, IBaseService baseService)
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
        public async Task<(List<TemplatePagingDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, int page, int pageSize, string? qSearch, string fromDate, string toDate)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", maDuAn);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            param.Add("@NgayLapFrom", fromDate);
            param.Add("@NgayLapTo", toDate);

            var result = (await connection.QueryAsync<TemplatePagingDto>(
                "Proc_Template_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa template
        public async Task<ResultModel> SaveTemplateAsync(TemplateModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var record = new HtTemplate
                {
                    MaTemplate = await SinhMaPhieuTuDongAsync("TEMP-", 5),
                    SoTemplate = model.SoPhieu,
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    TenTemplate = model.TenTemplate ?? string.Empty,
                    NoiDung = model.NoiDung,
                    TieuDe = model.TieuDe
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
                            MaPhieu = model.MaTemplate ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "Template",
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
                await context.HtTemplates.AddAsync(record);
                await context.SaveChangesAsync();
                return ResultModel.Success("Thêm template thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm template");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm template: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(TemplateModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await context.HtTemplates.FirstOrDefaultAsync(d => d.MaTemplate == model.MaTemplate);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy template.");
                }
                entity.TenTemplate = model.TenTemplate ?? string.Empty;
                entity.NoiDung = model.NoiDung ?? string.Empty;
                entity.TieuDe = model.TieuDe ?? string.Empty;
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaTemplate && d.Controller == "Template").ToListAsync();

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
                            MaPhieu = model.MaTemplate,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "Template",
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

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenTemplate} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật template");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteTemplateAsync(
     string maTemplate,
     string webRootPath,
     CancellationToken ct = default)
        {
            const string ControllerTemplate = "Template";

            try
            {
                if (string.IsNullOrWhiteSpace(maTemplate))
                    return ResultModel.Fail("Thiếu mã template.");

                if (string.IsNullOrWhiteSpace(webRootPath))
                    return ResultModel.Fail("Thiếu đường dẫn webRootPath.");

                var webRootFullPath = Path.GetFullPath(webRootPath);
                if (!Directory.Exists(webRootFullPath))
                    return ResultModel.Fail($"Thư mục webRootPath không tồn tại: {webRootFullPath}");

                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // B1) Lấy template
                var template = await context.HtTemplates
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MaTemplate == maTemplate, ct);

                if (template == null)
                    return ResultModel.Fail("Không tìm thấy template.");

                // B2) (OPTIONAL) Kiểm tra ràng buộc
                // Hiện tại em nói "không có ràng buộc" thì để như vầy:
                // var isUsedSomewhere = await context.SomeTable.AnyAsync(x => x.MaTemplate == maTemplate, ct);
                // if (isUsedSomewhere) return ResultModel.Fail("Template đang được sử dụng...");

                // B3) Lấy danh sách file đính kèm để lát nữa xóa vật lý
                var fileRecords = await context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.Controller == ControllerTemplate && d.MaPhieu == maTemplate)
                    .ToListAsync(ct);

                // B4) Xóa trong DB (transaction)
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 4.1) Xóa file đính kèm trong DB
                _ = await context.HtFileDinhKems
                    .Where(d => d.Controller == ControllerTemplate && d.MaPhieu == maTemplate)
                    .ExecuteDeleteAsync(ct);

                // 4.2) Xóa template
                _ = await context.HtTemplates
                    .Where(d => d.MaTemplate == maTemplate)
                    .ExecuteDeleteAsync(ct);

                await tx.CommitAsync(ct);

                // B5) Xóa file vật lý
                var deleted = 0;
                var failed = new List<string>();

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
                        failed.Add($"{rel} (ngoài webroot)");
                        continue;
                    }

                    try
                    {
                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            deleted++;
                        }
                    }
                    catch (Exception exDel)
                    {
                        _logger.LogWarning(exDel, "[DeleteTemplateAsync] Không xoá được file: {Rel}", rel);
                        failed.Add(rel);
                    }
                }

                var msg =
                    $"Xoá template \"{template.TenTemplate}\" thành công. " +
                    (fileRecords.Count == 0
                        ? "Không có file đính kèm."
                        : $"Xoá file đính kèm: {deleted}/{fileRecords.Count}.") +
                    (failed.Count > 0
                        ? " File không xoá được: " + string.Join(", ", failed.Take(5)) + (failed.Count > 5 ? ", ..." : "")
                        : "");

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteTemplateAsync] Lỗi khi xóa template {MaTemplate}", maTemplate);
                return ResultModel.Fail("Lỗi hệ thống: không thể xoá template.");
            }
        }

        public async Task<ResultModel> DeleteListAsync(
      List<TemplatePagingDto> listTL,
      string webRootPath,
      CancellationToken ct = default)
        {
            const int BatchSize = 1800;
            const string ControllerTemplate = "Template";

            try
            {
                // B0) Normalize & guard
                var ids = (listTL ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaTemplate))
                    .Select(x => x!.MaTemplate.Trim())
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

                // ===========================
                // B1) (OPTIONAL) RÀNG BUỘC
                // ===========================
                // Nếu hiện tại chưa có bảng nào tham chiếu Template thì có thể bỏ qua.
                // Khi phát sinh ràng buộc, chỉ cần thêm các AnyAsync/CountAsync tại đây
                // và build blockedMap tương tự các hàm trước (key=MaTemplate, value=count).
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                // ví dụ hook (comment lại nếu chưa dùng):
                // foreach (var chunk in Chunk(ids, BatchSize))
                // {
                //     var used = await context.SomeTable.AsNoTracking()
                //         .Where(x => x.MaTemplate != null && chunk.Contains(x.MaTemplate!))
                //         .GroupBy(x => x.MaTemplate!)
                //         .Select(g => new { MaTemplate = g.Key, Count = g.Count() })
                //         .ToListAsync(ct);
                //     foreach (var row in used)
                //         blockedMap[row.MaTemplate] = (blockedMap.TryGetValue(row.MaTemplate, out var c) ? c : 0) + row.Count;
                // }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- Template [{kv.Key}] đang được sử dụng: {kv.Value} bản ghi."));
                    return ResultModel.Fail("Không thể xoá: tất cả Template được chọn đều đang được sử dụng.\n" + detail);
                }

                // ==========================================
                // B2) Lấy danh sách file đính kèm cần xoá
                // ==========================================
                var filePaths = new List<string>();
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var files = await context.HtFileDinhKems.AsNoTracking()
                        .Where(d => d.Controller == ControllerTemplate && chunk.Contains(d.MaPhieu))
                        .Select(d => d.TenFileDinhKemLuu ?? string.Empty)
                        .ToListAsync(ct);

                    filePaths.AddRange(files);
                }

                filePaths = filePaths
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // ==========================================
                // B3) Xoá DB trong transaction (chunked)
                // ==========================================
                await using var tx = await context.Database.BeginTransactionAsync(ct);
                var totalDeleted = 0;

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await context.HtFileDinhKems
                        .Where(d => d.Controller == ControllerTemplate && chunk.Contains(d.MaPhieu))
                        .ExecuteDeleteAsync(ct);

                    var affected = await context.HtTemplates
                        .Where(t => chunk.Contains(t.MaTemplate))
                        .ExecuteDeleteAsync(ct);

                    totalDeleted += affected;
                }

                await tx.CommitAsync(ct);

                // ==========================================
                // B4) Xoá file vật lý (sau commit)
                // ==========================================
                var deletedFiles = 0;
                var failedFiles = new List<string>();

                foreach (var rel in filePaths)
                {
                    try
                    {
                        var clean = rel.TrimStart('/', '\\');
                        var fullPath = Path.GetFullPath(Path.Combine(webRootFullPath, clean));

                        // chống path traversal
                        if (!fullPath.StartsWith(webRootFullPath, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Bỏ qua file ngoài webroot: {Path}", fullPath);
                            failedFiles.Add($"{rel} (ngoài webroot)");
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
                        _logger.LogWarning(exDel, "[Template.DeleteListAsync] Không xoá được file: {Rel}", rel);
                        failedFiles.Add(rel);
                    }
                }

                // ==========================================
                // B5) Build message
                // ==========================================
                var skipped = ids.Count - totalDeleted;
                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} Template. " +
                    (skipped > 0 ? $"{skipped} template không xoá được (đang được sử dụng hoặc không tồn tại). " : "");

                var fileMsg = filePaths.Count == 0
                    ? "Không có file đính kèm."
                    : $"Xoá file đính kèm: {deletedFiles}/{filePaths.Count}.";

                var blockedDetail = "";
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv => $"- Template [{kv.Key}] đang được sử dụng: {kv.Value} bản ghi.");
                    blockedDetail = "\nChi tiết ràng buộc:\n" + string.Join(Environment.NewLine, top) +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} template khác." : "");
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
                _logger.LogError(ex, "[Template.DeleteListAsync] Lỗi khi xoá danh sách template");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách template.");
            }
        }

        // helper chung
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }
        #endregion

        #region Thông tin template 
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await (
                      from m in context.HtTemplates
                      join da in context.DaDanhMucDuAns on m.MaDuAn equals da.MaDuAn
                      where m.MaTemplate == id
                      select new TemplateModel
                      {
                          MaTemplate = m.MaTemplate,
                          SoPhieu = m.SoTemplate,
                          TenTemplate = m.TenTemplate,
                          NoiDung = m.NoiDung,
                          NgayLap = m.NgayLap ?? DateTime.Now,
                          MaNhanVien = m.NguoiLap,
                          MaDuAn = m.MaDuAn,
                          TenDuAn = da.TenDuAn,
                          TieuDe = m.TieuDe
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new TemplateModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaTemplate = await SinhMaPhieuTuDongAsync("TEMP-", 5);
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                }
                var files = await context.HtFileDinhKems.Where(d => d.Controller == "Template" && d.MaPhieu == id).Select(d => new UploadedFileModel
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
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một template");
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

        #region Hàm tăng tự động của mã template văn bảng
        public async Task<string> SinhMaPhieuTuDongAsync(string prefix, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.HtTemplates
                .Where(kh => kh.MaTemplate.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaTemplate)
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

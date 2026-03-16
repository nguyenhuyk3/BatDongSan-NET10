using ClosedXML.Excel;
using Dapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.LoaiCanHo;
using VTTGROUP.Domain.Model.SanPham;
using VTTGROUP.Infrastructure.Database;
using static Dapper.SqlMapper;

namespace VTTGROUP.Infrastructure.Services
{
    public class LoaiCanHoService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<LoaiCanHoService> _logger;
        private readonly IConfiguration _config;
        public LoaiCanHoService(IDbContextFactory<AppDbContext> factory, ILogger<LoaiCanHoService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        #region Hiển thị toàn bộ loại căn hộ
        public async Task<(List<LoaiCanHoPagingDto> Data, int TotalCount)> GetPagingAsync(
      string? maDuAn, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", maDuAn);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            var result = (await connection.QueryAsync<LoaiCanHoPagingDto>(
                "Proc_DanhMucLoaiCanHo_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalRows ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa loại căn hộ
        public async Task<ResultModel> SaveCanHoAsync(LoaiCanHoModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var tang = await context.DaDanhMucLoaiCanHos.FirstOrDefaultAsync(d => d.MaLoaiCanHo.ToLower() == model.MaLoaiCanHo.ToLower());
                if (tang != null)
                    return ResultModel.Fail("Loại căn hộ đã tồn tại.");

                var record = new DaDanhMucLoaiCanHo
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaLoaiCanHo = model.MaLoaiCanHo ?? string.Empty,
                    TenLoaiCanHo = model.TenLoaiCanHo ?? string.Empty,
                    //DienTich = model.DienTich,
                    //DienTichLotLong = model.DienTichLotLong,
                    //DienTichSanVuon = model.DienTichSanVuon,
                    //SoPhongNgu = model.SoPhongNgu,
                    //HeSoDienTich = model.HeSoDienTich,
                    //MoTa = model.MoTa,
                    //MaLoaiThietKe = model.MaLoaiThietKe,
                    //HinhAnh = model.Files != null && model.Files.Any() ? model.Files[0].FullDomain : string.Empty,
                };

                //List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();


                //if (model.Files != null && model.Files.Any())
                //{
                //    foreach (var file in model.Files)
                //    {
                //        if (string.IsNullOrEmpty(file.FileName)) continue;
                //        var savedPath = await SaveFileWithTickAsync(file);

                //        var f = new HtFileDinhKem
                //        {
                //            MaPhieu = model.MaLoaiCanHo ?? string.Empty,
                //            TenFileDinhKem = file.FileName,
                //            TenFileDinhKemLuu = savedPath,
                //            TaiLieuUrl = savedPath,
                //            Controller = "LoaiCanHo",
                //            AcTion = "Create",
                //            NgayLap = DateTime.Now,
                //            MaNhanVien = string.Empty,
                //            TenNhanVien = string.Empty,
                //            FileSize = file.FileSize,
                //            FileType = file.ContentType,
                //            FullDomain = file.FullDomain,
                //        };
                //        listFiles.Add(f);
                //    }
                //    if (listFiles.Any())
                //        await context.HtFileDinhKems.AddRangeAsync(listFiles);
                //}

                await context.DaDanhMucLoaiCanHos.AddAsync(record);
                await context.SaveChangesAsync();
                return ResultModel.Success("Thêm căn hộ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm căn hộ");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm tầng: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(LoaiCanHoModel model, string webRootPath)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await context.DaDanhMucLoaiCanHos.FirstOrDefaultAsync(d => d.MaLoaiCanHo == model.MaLoaiCanHo);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy căn hộ.");
                }
                // entity.MaDuAn = model.MaDuAn ?? string.Empty;
                entity.TenLoaiCanHo = model.TenLoaiCanHo ?? string.Empty;
                //entity.DienTich = model.DienTich;
                //entity.DienTichLotLong = model.DienTichLotLong;
                //entity.DienTichSanVuon = model.DienTichSanVuon;
                //entity.SoPhongNgu = model.SoPhongNgu;
                //entity.HeSoDienTich = model.HeSoDienTich;
                //entity.MoTa = model.MoTa;

                //if (!string.IsNullOrEmpty(entity.HinhAnh) && model.Files.Any())
                //{
                //    if (entity.HinhAnh != model.Files[0].FullDomain) //Có sự thay đổi file mới cần xóa file cũ
                //    {
                //        var f = context.HtFileDinhKems.Where(d => d.Controller == "LoaiCanHo" && d.MaPhieu == entity.MaLoaiCanHo && d.FullDomain == entity.HinhAnh);
                //        if (f != null && f.Any())
                //        {
                //            foreach (var file in f)
                //            {
                //                var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                //                if (File.Exists(fullPath)) File.Delete(fullPath);
                //            }

                //            context.HtFileDinhKems.RemoveRange(f);
                //        }
                //    }
                //}
                //entity.HinhAnh = model.Files != null && model.Files.Any() ? model.Files[0].FullDomain : string.Empty;


                //List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                //var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaLoaiCanHo && d.Controller == "LoaiCanHo" && d.FullDomain == entity.HinhAnh).ToListAsync();

                //if (model.Files != null && model.Files.Any())
                //{
                //    foreach (var file in model.Files)
                //    {
                //        if (string.IsNullOrEmpty(file.FileName)) continue;

                //        bool exists = UploadedFiles.Any(f =>
                //            f.TenFileDinhKem == file.FileName &&
                //            f.FileSize == file.FileSize
                //        );
                //        if (exists)
                //            continue;

                //        var savedPath = await SaveFileWithTickAsync(file);

                //        var f = new HtFileDinhKem
                //        {
                //            MaPhieu = model.MaLoaiCanHo,
                //            TenFileDinhKem = file.FileName,
                //            TenFileDinhKemLuu = savedPath,
                //            TaiLieuUrl = savedPath,
                //            Controller = "LoaiCanHo",
                //            AcTion = "Edit",
                //            NgayLap = DateTime.Now,
                //            MaNhanVien = string.Empty,
                //            TenNhanVien = string.Empty,
                //            FileSize = file.FileSize,
                //            FileType = file.ContentType,
                //            FullDomain = file.FullDomain,
                //        };
                //        listFiles.Add(f);
                //    }
                //    await context.HtFileDinhKems.AddRangeAsync(listFiles);
                //}

                await context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenLoaiCanHo} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        //public async Task<ResultModel> DeleteLCHAsync(string maLoaiCH, string webRootPath)
        //{
        //    try
        //    {
        //        using var context = _factory.CreateDbContext();
        //        var lch = await context.DaDanhMucLoaiCanHos.Where(d => d.MaLoaiCanHo == maLoaiCH).FirstOrDefaultAsync();
        //        if (lch == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy loại căn hộ");
        //        }

        //        var listFiles = context.HtFileDinhKems.Where(d => d.Controller == "LoaiCanHo" && d.MaPhieu == lch.MaLoaiCanHo);
        //        if (listFiles != null && listFiles.Any())
        //        {
        //            foreach (var file in listFiles)
        //            {
        //                var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
        //                if (File.Exists(fullPath)) File.Delete(fullPath);
        //            }

        //            context.HtFileDinhKems.RemoveRange(listFiles);
        //        }
        //        context.DaDanhMucLoaiCanHos.Remove(lch);
        //        context.SaveChanges();
        //        return ResultModel.Success($"Xóa {lch.TenLoaiCanHo} thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[DeleteCongViecAsync] Lỗi khi xóa loại căn hộ");
        //        return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> DeleteLCHAsync(
    string maLoaiCH,
    string webRootPath,
    CancellationToken ct = default)
        {
            try
            {
                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // 1) Lấy thông tin tối thiểu + danh sách file đính kèm (nhẹ với AsNoTracking)
                var lch = await context.DaDanhMucLoaiCanHos
                    .AsNoTracking()
                    .Where(d => d.MaLoaiCanHo == maLoaiCH)
                    .Select(d => new { d.MaLoaiCanHo, d.TenLoaiCanHo })
                    .SingleOrDefaultAsync(ct);

                if (lch == null)
                    return ResultModel.Fail("Không tìm thấy loại căn hộ");

                var fileRelPaths = await context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.Controller == "LoaiCanHo" && d.MaPhieu == lch.MaLoaiCanHo)
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync(ct);

                // (Tuỳ chọn) Chặn xoá nếu đang được tham chiếu nơi khác
                // bool inUse = await context.DaDanhMucSanPhams.AsNoTracking()
                //     .AnyAsync(x => x.MaLoaiCanHo == maLoaiCH, ct);
                // if (inUse) return ResultModel.Fail("Loại căn hộ đang được sử dụng, không thể xoá.");

                // 2) Xoá trong DB bằng transaction
                await using var tran = await context.Database.BeginTransactionAsync(ct);

                // Xoá file đính kèm trong DB
                await context.HtFileDinhKems
                    .Where(d => d.Controller == "LoaiCanHo" && d.MaPhieu == maLoaiCH)
                    .ExecuteDeleteAsync(ct); // EF Core 7+

                // Xoá loại căn hộ
                var affected = await context.DaDanhMucLoaiCanHos
                    .Where(d => d.MaLoaiCanHo == maLoaiCH)
                    .ExecuteDeleteAsync(ct); // EF Core 7+

                if (affected == 0)
                {
                    await tran.RollbackAsync(ct);
                    return ResultModel.Fail("Không xoá được loại căn hộ (có thể đã bị thay đổi).");
                }

                await tran.CommitAsync(ct);

                // 3) Sau khi DB commit mới xoá file trên đĩa (best-effort, không rollback DB nếu lỗi file)
                int deletedCount = 0, notFoundCount = 0, errorCount = 0;

                foreach (var relPath in fileRelPaths.Distinct())
                {
                    try
                    {
                        var fullPath = SafeCombineUnderRoot(webRootPath, relPath);
                        if (fullPath == null)
                        {
                            _logger.LogWarning("[DeleteLCHAsync] Bỏ qua path không an toàn: {Path}", relPath);
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
                        _logger.LogWarning(exFile, "[DeleteLCHAsync] Lỗi khi xoá file: {RelPath}", relPath);
                    }
                }

                var msg = $"Xoá \"{lch.TenLoaiCanHo}\" thành công. " +
                          (fileRelPaths.Count > 0
                              ? $"Tệp: xoá {deletedCount}, không tìm thấy {notFoundCount}, lỗi {errorCount}."
                              : "Không có tệp đính kèm.");

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteLCHAsync] Lỗi khi xoá loại căn hộ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }

            // ===== Helper: chống path traversal & chuẩn hoá đường dẫn =====
            static string? SafeCombineUnderRoot(string root, string rel)
            {
                if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(rel))
                    return null;

                var trimmed = rel.TrimStart('/', '\\'); // tránh coi là absolute path
                var combined = Path.GetFullPath(Path.Combine(root, trimmed));
                var rootFull = Path.GetFullPath(root);

                // Chỉ cho phép xoá trong webRootPath
                if (!combined.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                    return null;

                return combined;
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<LoaiCanHoPagingDto> listPDC, string webRootPath,
    CancellationToken ct = default)
        {
            const int BatchSize = 1800;
            const string ControllerLoaiCanHo = "LoaiCanHo";

            try
            {
                // --- B0: Normalize & guard ---
                var ids = (listPDC ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaLoaiCanHo))
                    .Select(x => x!.MaLoaiCanHo.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                if (string.IsNullOrWhiteSpace(webRootPath))
                    return ResultModel.Fail("Thiếu đường dẫn webRootPath.");

                var webRootFullPath = Path.GetFullPath(webRootPath);
                if (!Directory.Exists(webRootFullPath))
                    return ResultModel.Fail($"Thư mục webRootPath không tồn tại: {webRootFullPath}");
                if (!webRootFullPath.EndsWith(Path.DirectorySeparatorChar))
                    webRootFullPath += Path.DirectorySeparatorChar;

                await using var _context = _factory.CreateDbContext();

                // --- B1: Kiểm tra ràng buộc ở DaDanhMucSanPhams (SP/Căn hộ) ---
                // map: MaLoaiCanHo -> count(SanPham)
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var counts = await _context.DaDanhMucSanPhams.AsNoTracking()
                        .Where(sp => sp.MaLoaiCan != null && chunk.Contains(sp.MaLoaiCan!))
                        .GroupBy(sp => sp.MaLoaiCan!)
                        .Select(g => new { MaLoaiCan = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync: Check DaDanhMucSanPhams by MaLoaiCanHo")
                        .ToListAsync();

                    foreach (var row in counts)
                        blockedMap[row.MaLoaiCan] = (blockedMap.TryGetValue(row.MaLoaiCan, out var cur) ? cur : 0) + row.Count;
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- Loại diện tích [{kv.Key}] đang được sử dụng bởi {kv.Value} sản phẩm/căn hộ.")
                    );
                    return ResultModel.Fail("Không thể xoá: tất cả Loại căn hộ được chọn đều đang được sử dụng.\n" + detail);
                }

                // --- B2: Lấy trước danh sách file đính kèm của các ID có thể xoá ---
                var filePaths = new List<string>();
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var files = await _context.HtFileDinhKems.AsNoTracking()
                        .Where(d => d.Controller == ControllerLoaiCanHo && chunk.Contains(d.MaPhieu))
                        .Select(d => d.TenFileDinhKemLuu ?? string.Empty)
                        .TagWith("DeleteListAsync: Load HtFileDinhKems for deletable MaLoaiCanHo")
                        .ToListAsync();

                    filePaths.AddRange(files);
                }
                filePaths = filePaths
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .Select(p => p.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                // --- B3: Xoá DB trong transaction ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await _context.HtFileDinhKems
                        .Where(d => d.Controller == ControllerLoaiCanHo && chunk.Contains(d.MaPhieu))
                        .TagWith("DeleteListAsync: ExecuteDelete HtFileDinhKems by MaPhieu")
                        .ExecuteDeleteAsync();

                    _ = await _context.DaDanhMucLoaiCanHos
                        .Where(k => chunk.Contains(k.MaLoaiCanHo))
                        .TagWith("DeleteListAsync: ExecuteDelete DaDanhMucLoaiCanHos")
                        .ExecuteDeleteAsync();
                }

                await tx.CommitAsync();

                // --- B4: Xoá file vật lý (sau khi DB đã commit) ---
                int cFileDeleted = 0;
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
                            cFileDeleted++;
                        }
                    }
                    catch (Exception exDel)
                    {
                        _logger.LogWarning(exDel, "[DeleteListAsync] Không xóa được file: {RelPath}", relPath);
                        failedFiles.Add(relPath);
                        // không throw – tránh ảnh hưởng dữ liệu đã commit
                    }
                }

                // --- B5: Thông điệp kết quả ---
                var totalDeleted = deletableIds.Count;
                var skipped = ids.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} Loại diện tích. " +
                    (skipped == 0 ? "" : $"{skipped} Loại diện tích không xoá (đang được sử dụng hoặc không tồn tại). ");

                var fileMsg = filePaths.Count == 0
                    ? "Không có file đính kèm."
                    : $"Xoá file đính kèm: {cFileDeleted}/{filePaths.Count}.";

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv => $"- Loại diện tích [{kv.Key}] đang được sử dụng bởi {kv.Value} sản phẩm/căn hộ.");
                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách loại diện tích");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách loại diện tích.");
            }
        }

        // Helper chunk
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin căn hộ
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await (
                      from lch in context.DaDanhMucLoaiCanHos
                      join duan in context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                      join ltk in context.DaDanhMucLoaiThietKes on lch.MaLoaiThietKe equals ltk.MaLoaiThietKe into dtLTK
                      from ltk2 in dtLTK.DefaultIfEmpty()
                      where lch.MaLoaiCanHo == id
                      select new LoaiCanHoModel
                      {
                          MaDuAn = lch.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaLoaiCanHo = lch.MaLoaiCanHo,
                          TenLoaiCanHo = lch.TenLoaiCanHo,
                          DienTich = lch.DienTich ?? 0,
                          DienTichLotLong = lch.DienTichLotLong ?? 0,
                          DienTichSanVuon = lch.DienTichSanVuon ?? 0,
                          SoPhongNgu = lch.SoPhongNgu ?? 1,
                          HeSoDienTich = lch.HeSoDienTich ?? 0,
                          MaLoaiThietKe = lch.MaLoaiThietKe,
                          TenLoaiThietKe = ltk2.TenLoaiThietKe,
                          MoTa = lch.MoTa
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new LoaiCanHoModel();
                }
                //var files = await context.HtFileDinhKems.Where(d => d.Controller == "LoaiCanHo" && d.MaPhieu == id).Select(d => new UploadedFileModel
                //{
                //    Id = d.Id,
                //    FileName = d.TenFileDinhKem,
                //    FileNameSave = d.TenFileDinhKemLuu,
                //    FileSize = d.FileSize,
                //    ContentType = d.FileType
                //}).ToListAsync();
                //entity.Files = files;
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một căn hộ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<List<DaDanhMucLoaiThietKe>> GetByLTKTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucLoaiThietKe>();
            try
            {
                using var context = _factory.CreateDbContext();
                entity = await context.DaDanhMucLoaiThietKes.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucLoaiThietKe>();
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

        #region Danh sách sản phẩm thuộc loại căn hộ
        public async Task<(List<SanPhamPagingDto> Data, int TotalCount)> GetPagingSanPhamAsync(
        string? maDuAn, string? maLoaiCanHo, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@MaLoaiCanHo", maLoaiCanHo);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<SanPhamPagingDto>(
                "Proc_LoaiCanHo_SanPham",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Cập nhật số lượng sản phẩm trong loại căn hộ
        public async Task<ResultModel> CapNhatSanPhamTrongLoaiCanHoAsync(List<SanPhamPagingDto?> listSP, string loaiXuLy, string maLoaiCanHo)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var maSanPhams = listSP
            .Where(x => x != null)
            .Select(x => x!.MaSanPham)
            .ToList();
                if (maSanPhams.Count == 0)
                    return ResultModel.Success("Không có sản phẩm nào để cập nhật");
                await context.DaDanhMucSanPhams
                    .Where(sp => maSanPhams.Contains(sp.MaSanPham))
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(sp => sp.MaLoaiCan, maLoaiCanHo));

                return ResultModel.Success("Cập nhật sản phẩm trong loại căn hộ thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CapNhatSanPhamTrongLoaiCanHoAsync] Lỗi khi xóa loại căn hộ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Import, download file mẫu        
        public async Task<byte[]> GenerateTemplateWithDataAsync(string templatePath, string maDuAn, CancellationToken ct = default)
        {
            using var _context = _factory.CreateDbContext();
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);

            //var blockSheet = workbook.Worksheet("ThietKe");
            //var listBlock = await _context.DaDanhMucLoaiThietKes.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            //// Bắt đầu từ dòng 2 (vì dòng 1 là header)
            //int row = 2;
            //foreach (var item in listBlock)
            //{
            //    blockSheet.Cell(row, 1).Value = item.MaLoaiThietKe;
            //    blockSheet.Cell(row, 2).Value = item.TenLoaiThietKe;
            //    row++;
            //}
            // Set lại sheet "SanPham" là active
            workbook.Worksheet("LoaiCanHo").SetTabActive();

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
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaLoaiCanHo)
                             && !string.IsNullOrWhiteSpace(x.TenLoaiCanHo))
                    .Select(x =>
                    {
                        x.MaLoaiCanHo = x.MaLoaiCanHo!.Trim().ToUpperInvariant(); // so sánh mã block không phân biệt hoa/thường                     
                        x.TenLoaiCanHo = x.TenLoaiCanHo!.Trim();
                        x.MaDuAn = maDuAn;
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // 4) Thống kê trùng ngay trong file (theo MaLoaiCanHo, không phân biệt hoa/thường)
                var duplicateInFile = items
                    .GroupBy(x => x.MaLoaiCanHo!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                // Giữ bản đầu tiên cho mỗi MaLoaiCanHo (bỏ trùng nội bộ file)
                items = items
                    .GroupBy(x => x.MaLoaiCanHo!, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                //// 5) Kiểm tra MaLoaiCanHo tồn tại theo MaDuAn (STRICT: nếu thiếu → fail)
                //var existingBlocks = new HashSet<string>(
                //    await _context.DaDanhMucLoaiThietKes
                //        .AsNoTracking()
                //        .Where(b => b.MaDuAn == maDuAn && b.MaLoaiThietKe != null)
                //        .Select(b => b.MaLoaiThietKe!)
                //        .ToListAsync(ct),
                //    StringComparer.OrdinalIgnoreCase);

                //var invalidByBlock = items.Where(x => !existingBlocks.Contains(x.MaThietKe!)).ToList();
                //if (invalidByBlock.Count > 0)
                //{
                //    var preview = string.Join(", ",
                //        invalidByBlock.Select(x => x.MaThietKe).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                //    return ResultModel.Fail(
                //        $"Có {invalidByBlock.Count} dòng có MaThietKe không tồn tại trong dự án '{maDuAn}'. " +
                //        $"Một số mã: {preview}{(invalidByBlock.Count > 10 ? ", ..." : "")}");
                //}

                // 6) Lấy các MaLoaiCanHo đã tồn tại TRONG TOÀN HỆ THỐNG (system-wide)
                var existingAllTangSet = new HashSet<string>(
                    await _context.DaDanhMucLoaiCanHos
                        .AsNoTracking()
                        .Where(t => t.MaLoaiCanHo != null)
                        .Select(t => t.MaLoaiCanHo!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                // 6b) (Tuỳ chọn) Lấy các MaLoaiCanHo đã tồn tại theo dự án để báo cáo dễ hiểu
                var existingTangInProject = new HashSet<string>(
                    await _context.DaDanhMucLoaiCanHos
                        .AsNoTracking()
                        .Where(t => t.MaDuAn == maDuAn && t.MaLoaiCanHo != null)
                        .Select(t => t.MaLoaiCanHo!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                // 7) Phân loại: trùng trong DB (toàn hệ thống) vs thực sự mới
                var duplicatesInDbSystem = items.Where(x => existingAllTangSet.Contains(x.MaLoaiCanHo!)).ToList();
                var newItems = items.Where(x => !existingAllTangSet.Contains(x.MaLoaiCanHo!)).ToList();

                if (newItems.Count == 0)
                {
                    // Ưu tiên hiện ví dụ trong phạm vi dự án cho dễ hiểu
                    var previewProj = string.Join(", ",
                        duplicatesInDbSystem.Where(x => existingTangInProject.Contains(x.MaLoaiCanHo!))
                            .Select(x => x.MaLoaiCanHo).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));

                    // Nếu trong dự án không có ví dụ, lấy đại từ system
                    var previewSystem = string.Join(", ",
                        duplicatesInDbSystem.Select(x => x.MaLoaiCanHo).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));

                    var preview = string.IsNullOrWhiteSpace(previewProj) ? previewSystem : previewProj;

                    return ResultModel.Fail(
                        $"Tất cả MaLoaiCanHo trong file đều đã tồn tại trong hệ thống. " +
                        (duplicatesInDbSystem.Count > 0
                            ? $"Ví dụ: {preview}{(duplicatesInDbSystem.Count > 10 ? ", ..." : "")}"
                            : ""));
                }

                // 8) Thêm hàng loạt (chỉ những mã mới system-wide)
                var entities = newItems.Select(item => new DaDanhMucLoaiCanHo
                {
                    MaLoaiCanHo = item.MaLoaiCanHo,
                    TenLoaiCanHo = item.TenLoaiCanHo,
                    MaDuAn = item.MaDuAn,
                    //MaLoaiThietKe = item.MaThietKe,
                    //DienTich = item.DienTich,
                    //DienTichLotLong = item.DienTichLotLong,
                    //DienTichSanVuon = item.DienTichSanVuon,
                    //HeSoDienTich = item.HeSoDienTich,
                    //SoPhongNgu = item.SoPhongNgu,                   
                    // MoTa = item.MoTa
                }).ToList();

                await _context.DaDanhMucLoaiCanHos.AddRangeAsync(entities, ct);
                var affected = await _context.SaveChangesAsync(ct);

                // 9) Ghép message thân thiện
                var msg = $"Import thành công {entities.Count} loại căn hộ (đã ghi {affected} bản ghi).";
                if (duplicatesInDbSystem.Count > 0)
                {
                    var dupDbPreview = string.Join(", ",
                        duplicatesInDbSystem.Select(x => x.MaLoaiCanHo).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                    msg += $" Bỏ qua {duplicatesInDbSystem.Count} dòng vì MaLoaiCanHo đã tồn tại trong DB (toàn hệ thống)"
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
        public List<LoaiCanHoImportModel> ReadTangFromExcel(Stream stream, string maDuAn)
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

            var table = dataSet.Tables["LoaiCanHo"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'LoaiCanHo' trong file Excel.");

            var list = new List<LoaiCanHoImportModel>();

            foreach (DataRow r in table.Rows)
            {
                string? maLoaiCan = r[0]?.ToString()?.Trim();
                string? tenLoaiCan = r[1]?.ToString()?.Trim();

                //decimal? dienTich = null;
                //if (decimal.TryParse(r[3]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dt))
                //    dienTich = dt;

                //decimal? dienTichLL = null;
                //if (decimal.TryParse(r[4]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dtll))
                //    dienTichLL = dtll;

                //decimal? dienTichSV = null;
                //if (decimal.TryParse(r[5]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dtsv))
                //    dienTichSV = dtsv;

                //int? soPhongNgu = null;
                //if (int.TryParse(r[6]?.ToString()?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stt))
                //    soPhongNgu = stt;

                //decimal? heSoDT = null;
                //if (decimal.TryParse(r[7]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hst))
                //    heSoDT = hst;

                //string? ghiChu = r[8]?.ToString()?.Trim();

                // Nếu cả 3 đều rỗng → bỏ qua dòng trống
                if (string.IsNullOrWhiteSpace(maLoaiCan)
                 || string.IsNullOrWhiteSpace(tenLoaiCan))
                    continue;

                list.Add(new LoaiCanHoImportModel
                {
                    MaDuAn = maDuAn,
                    MaLoaiCanHo = maLoaiCan,
                    TenLoaiCanHo = tenLoaiCan,
                    //DienTich = dienTich ?? 1,
                    //DienTichLotLong = dienTichLL ?? 1,
                    //DienTichSanVuon = dienTichSV ?? 1,
                    //HeSoDienTich = heSoDT ?? 1,
                    //SoPhongNgu = soPhongNgu ?? 1,
                    //MoTa = ghiChu
                });
            }

            return list;
        }
        #endregion
    }
}

using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.ViTri;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ViTriService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<ViTriService> _logger;
        private readonly IConfiguration _config;
        private sealed record ViTriKey(string MaDuAn, string MaViTri);
        public ViTriService(IDbContextFactory<AppDbContext> factory, ILogger<ViTriService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
        }

        #region Hiển thị toàn bộ danh sách vị trí
        public async Task<List<ViTriModel>> GetViTriAsync(string maDuAn, string qSearch = null)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var listVT = await (
                                from lch in _context.DaDanhMucViTris
                                join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn into dtDong
                                from duan2 in dtDong.DefaultIfEmpty()
                                where (string.IsNullOrEmpty(maDuAn) || lch.MaDuAn == maDuAn)
                              &&
                     (
                         string.IsNullOrEmpty(qSearch) ||
                         EF.Functions.Like(lch.MaViTri, $"%{qSearch}%") ||
                         EF.Functions.Like(lch.TenViTri, $"%{qSearch}%") ||
                         EF.Functions.Like(lch.MaDuAn, $"%{qSearch}%") ||
                         EF.Functions.Like(duan2.TenDuAn, $"%{qSearch}%")
                     )
                                select new ViTriModel
                                {
                                    MaViTri = lch.MaViTri,
                                    TenViTri = lch.TenViTri,
                                    MaDuAn = lch.MaDuAn,
                                    TenDuAn = duan2.TenDuAn,
                                    HeSoViTri = lch.HeSoViTri ?? 1,
                                    // Đếm số lượng vị trí theo trục
                                    SoLuongVMK = _context.DaDanhMucViewTrucs.Count(t => t.MaViTri == lch.MaViTri && t.MaDuAn == lch.MaDuAn)
                                }).ToListAsync();
                return listVT;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách vị trí");
            }
            return new List<ViTriModel>();
        }
        #endregion

        #region Them, xoa, sửa vị trí
        public async Task<ResultModel> SaveViTriAsync(ViTriModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaDanhMucViTris.FirstOrDefaultAsync(d => d.MaViTri.ToLower() == model.MaViTri.ToLower());
                if (duAn != null)
                    return ResultModel.Fail("Vị trí đã tồn tại.");

                var record = new DaDanhMucViTri
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaViTri = model.MaViTri ?? string.Empty,
                    TenViTri = model.TenViTri ?? string.Empty,
                    HeSoViTri = model.HeSoViTri
                };

                await _context.DaDanhMucViTris.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm vị trí thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm dự án");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm dự án: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(ViTriModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucViTris.FirstOrDefaultAsync(d => d.MaViTri == model.MaViTri);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy vị trí.");
                }
                entity.TenViTri = model.TenViTri ?? string.Empty;
                entity.HeSoViTri = model.HeSoViTri;

                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenViTri} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật vị trí");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteViTriAsync(string maViTri, string maDuAn)
        {
            if (string.IsNullOrWhiteSpace(maViTri))
                return ResultModel.Fail("Thiếu mã vị trí.");

            maViTri = maViTri.Trim();

            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc trong View Trục
                var usage = await _context.DaDanhMucViewTrucs.AsNoTracking()
                    .Where(v => v.MaViTri == maViTri && v.MaDuAn == maDuAn)
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteViTriAsync: Check DA_DanhMucViewTruc by MaViTri")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    var samples = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaViTri == maViTri && v.MaDuAn == maDuAn)
                        .Select(v => new { v.MaTruc, v.TenTruc, v.MaDuAn, v.MaBlock })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[{s.MaTruc}-{s.TenTruc} | DA:{s.MaDuAn} | Block:{s.MaBlock}]"));
                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{((usage!.Count > samples.Count) ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì Vị trí đang được sử dụng trong View Trục: {usage!.Count} dòng.{hint}");
                }

                // B2) Lấy entity & xoá
                var entity = await _context.DaDanhMucViTris
                    .FirstOrDefaultAsync(d => d.MaViTri == maViTri && d.MaDuAn == maDuAn);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy vị trí.");

                _context.DaDanhMucViTris.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaViTri}] - {entity.TenViTri} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteViTriAsync] Lỗi ràng buộc khi xoá {MaViTri}", maViTri);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ liên kết trong View Trục trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteViTriAsync] Lỗi hệ thống khi xoá {MaViTri}", maViTri);
                return ResultModel.Fail("Lỗi hệ thống khi xoá vị trí.");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<ViTriModel> listVT)
        {
            const int BatchSize = 1800;

            try
            {
                await using var _context = _factory.CreateDbContext();

                // --- B0: Chuẩn hoá & guard ---
                // Lấy danh sách cặp (MaDuAn, MaViTri) được chọn
                var selected = (listVT ?? new())
                    .Where(x => x.IsSelected
                                && !string.IsNullOrWhiteSpace(x.MaDuAn)
                                && !string.IsNullOrWhiteSpace(x.MaViTri))
                    .Select(x => new ViTriKey(
                        x.MaDuAn!.Trim(),
                        x.MaViTri!.Trim()
                    ))
                    .Distinct()
                    .ToList();

                if (selected.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                _context.ChangeTracker.Clear();

                // --- B1: Kiểm tra ràng buộc ở DA_DanhMucViewTruc (theo MaDuAn + MaViTri) ---
                // map: (MaDuAn, MaViTri) -> số dòng ViewTruc đang dùng
                var blockedMap = new Dictionary<ViTriKey, int>();

                // Group theo MaDuAn để query gọn
                var groupsByProject = selected.GroupBy(k => k.MaDuAn);

                foreach (var group in groupsByProject)
                {
                    var maDuAn = group.Key;
                    var viTriIds = group.Select(k => k.MaViTri).ToList();

                    foreach (var chunk in Chunk(viTriIds, BatchSize))
                    {
                        var counts = await _context.DaDanhMucViewTrucs
                            .AsNoTracking()
                            .Where(v =>
                                v.MaDuAn == maDuAn &&
                                v.MaViTri != null &&
                                chunk.Contains(v.MaViTri!)
                            )
                            .GroupBy(v => v.MaViTri!)
                            .Select(g => new { MaViTri = g.Key, Count = g.Count() })
                            .TagWith("DeleteListAsync[ViTri]: Check DA_DanhMucViewTruc by (MaDuAn, MaViTri)")
                            .ToListAsync();

                        foreach (var row in counts)
                        {
                            var key = new ViTriKey(maDuAn, row.MaViTri);
                            blockedMap[key] = blockedMap.TryGetValue(key, out var cur)
                                ? cur + row.Count
                                : row.Count;
                        }
                    }
                }

                var blockedKeys = blockedMap.Keys.ToHashSet();
                var deletableKeys = selected.Where(k => !blockedKeys.Contains(k)).ToList();

                if (deletableKeys.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap
                            .OrderBy(kv => kv.Key.MaDuAn)
                            .ThenBy(kv => kv.Key.MaViTri)
                            .Select(kv =>
                                $"- Vị trí [{kv.Key.MaViTri}] của dự án [{kv.Key.MaDuAn}] đang được sử dụng tại {kv.Value} hàng View Trục."));

                    return ResultModel.Fail(
                        "Không thể xoá: tất cả vị trí được chọn đều đang được sử dụng trong View Trục.\n" +
                        detail);
                }

                // --- B2: Xoá DB trong transaction (chỉ xoá các cặp (MaDuAn, MaViTri) không bị ràng buộc) ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var group in deletableKeys.GroupBy(k => k.MaDuAn))
                {
                    var maDuAn = group.Key;
                    var viTriIds = group.Select(k => k.MaViTri).ToList();

                    foreach (var chunk in Chunk(viTriIds, BatchSize))
                    {
                        _ = await _context.DaDanhMucViTris
                            .Where(v =>
                                v.MaDuAn == maDuAn &&              // giả sử bảng này cũng có MaDuAn
                                chunk.Contains(v.MaViTri))
                            .TagWith("DeleteListAsync[ViTri]: ExecuteDelete DaDanhMucViTris by (MaDuAn, MaViTri)")
                            .ExecuteDeleteAsync();
                    }
                }

                await tx.CommitAsync();

                // --- B3: Thông điệp kết quả ---
                var totalDeleted = deletableKeys.Count;
                var skipped = selected.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{selected.Count} vị trí (theo từng dự án). " +
                    (skipped == 0
                        ? ""
                        : $"{skipped} vị trí không xoá (đang được sử dụng trong View Trục hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedKeys.Count > 0)
                {
                    var top = blockedMap
                        .OrderBy(kv => kv.Key.MaDuAn)
                        .ThenBy(kv => kv.Key.MaViTri)
                        .Take(10)
                        .Select(kv =>
                            $"- Vị trí [{kv.Key.MaViTri}] của dự án [{kv.Key.MaDuAn}] đang được sử dụng tại {kv.Value} hàng View Trục.");

                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedKeys.Count > 10
                                        ? $"\n... và {blockedKeys.Count - 10} vị trí khác."
                                        : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách vị trí");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách vị trí.");
            }
        }

        // Helper chunk (nếu chưa có)
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin vị trí
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from lch in _context.DaDanhMucViTris
                      join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                      where lch.MaViTri == id
                      select new ViTriModel
                      {
                          MaDuAn = lch.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaViTri = lch.MaViTri,
                          TenViTri = lch.TenViTri,
                          HeSoViTri = lch.HeSoViTri ?? 1
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new ViTriModel();
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một vị trí trong dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Import, download file mẫu        
        public async Task<byte[]> GenerateTemplateWithDataAsync(string templatePath)
        {
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);
            // Set lại sheet "SanPham" là active
            //  workbook.Worksheet("Sheet1").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }

        public async Task<ResultModel> ImportFromExcelAsync(IBrowserFile file, string maDuAn)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await inputStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Đọc file
                var items = ReadLoaiGocFromExcel(memoryStream, maDuAn);

                // --- B1: Lọc rỗng + chuẩn hoá ---
                items = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaViTri) && !string.IsNullOrWhiteSpace(x.TenViTri))
                    .Select(x =>
                    {
                        x.MaViTri = x.MaViTri!.Trim().ToUpperInvariant();
                        x.TenViTri = x.TenViTri!.Trim();
                        x.HeSoViTri = x.HeSoViTri;
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // --- B2: Kiểm tra trùng trong CHÍNH FILE (case-insensitive) ---
                var dupGroups = items
                    .GroupBy(x => x.MaViTri!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (dupGroups.Count > 0)
                {
                    // Lấy tối đa 10 mã trùng + kèm danh sách dòng
                    var lines = dupGroups.Take(10).Select(g =>
                        $"- {g.Key}: dòng {string.Join(", ", g.Select(it => it.RowIndex).OrderBy(i => i))}"
                    );
                    var msg = "Phát hiện mã vị trí bị trùng ngay trong file:\n" + string.Join("\n", lines);
                    if (dupGroups.Count > 10) msg += $"\n... và {dupGroups.Count - 10} mã khác.";
                    return ResultModel.Fail(msg);
                }

                // --- B3: Lấy các mã đã tồn tại trong DB ---
                var existingCodes = new HashSet<string>(
                    await _context.DaDanhMucViTris
                        .Where(x => x.MaViTri != null && x.MaDuAn == maDuAn)
                        .Select(x => x.MaViTri!)
                        .ToListAsync(),
                    StringComparer.OrdinalIgnoreCase
                );

                // --- B4: Lọc ra các bản ghi MỚI chưa có trong DB ---
                var newItems = items
                    .Where(x => !existingCodes.Contains(x.MaViTri!))
                    .ToList();

                if (newItems.Count == 0)
                    return ResultModel.Fail("Không có vị trí nào được thêm mới (tất cả mã đã tồn tại trong hệ thống).");

                // --- B5: Insert hàng loạt ---
                var entities = newItems.Select(item => new DaDanhMucViTri
                {
                    MaViTri = item.MaViTri!,
                    TenViTri = item.TenViTri!,
                    MaDuAn = maDuAn,
                    HeSoViTri = item.HeSoViTri
                }).ToList();

                await _context.DaDanhMucViTris.AddRangeAsync(entities);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Import thành công {entities.Count} Vị trí mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public List<ViTriImportModel> ReadLoaiGocFromExcel(Stream stream, string maDuAn)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataset.Tables["ViTri"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'ViTri' trong file Excel.");

            var list = new List<ViTriImportModel>();
            // Header đang bật, nên dòng dữ liệu đầu tiên là excel row 2
            int excelRow = 2;

            foreach (DataRow row in table.Rows)
            {
                string? maViTri = row[0]?.ToString()?.Trim()?.ToUpperInvariant();
                string? tenViTri = row[1]?.ToString()?.Trim();

                decimal? heSoVT = null;
                var rawHsg = row.Table.Columns.Count > 2 ? row[2]?.ToString()?.Trim() : null;
                if (!string.IsNullOrWhiteSpace(rawHsg) &&
                    decimal.TryParse(rawHsg, NumberStyles.Any, CultureInfo.InvariantCulture, out var hsg))
                {
                    heSoVT = hsg;
                }

                list.Add(new ViTriImportModel
                {
                    MaViTri = maViTri,
                    TenViTri = tenViTri,
                    MaDuAn = maDuAn,
                    HeSoViTri = heSoVT,
                    RowIndex = excelRow
                });

                excelRow++;
            }

            return list;
        }
        #endregion
    }
}

using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.ViewMatKhoi;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class MatKhoiService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<MatKhoiService> _logger;
        private readonly IConfiguration _config;
        public MatKhoiService(IDbContextFactory<AppDbContext> factory, ILogger<MatKhoiService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
        }

        #region Hiển thị toàn bộ danh sách mặt khối
        //public async Task<List<ViewMatKhoiModel>> GetViewAsync()
        //{
        //    try
        //    {
        //        var listVT = await (
        //                        from lch in _context.DaDanhMucViewMatKhois
        //                        join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn into dtDong
        //                        from duan2 in dtDong.DefaultIfEmpty()
        //                        select new ViewMatKhoiModel
        //                        {
        //                            MaMatKhoi = lch.MaMatKhoi,
        //                            TenMatKhoi = lch.TenMatKhoi,
        //                            MaDuAn = lch.MaDuAn,
        //                            TenDuAn = duan2.TenDuAn,
        //                            HeSoMatKhoi = lch.HeSoMatKhoi ?? 0
        //                        }).ToListAsync();
        //        return listVT;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách view mặt khối");
        //    }
        //    return new List<ViewMatKhoiModel>();
        //}

        public async Task<List<ViewMatKhoiModel>> GetViewAsync(
     string? maDuAn = null,
     string? qSearch = null,
     CancellationToken ct = default)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                qSearch ??= string.Empty;
                var hasSearch = !string.IsNullOrWhiteSpace(qSearch);
                var pattern = hasSearch ? $"%{EscapeLike(qSearch.Trim())}%" : null;

                var query =
                    from lch in _context.DaDanhMucViewMatKhois.AsNoTracking()
                    join duan in _context.DaDanhMucDuAns.AsNoTracking()
                        on lch.MaDuAn equals duan.MaDuAn into dtDong
                    from duan2 in dtDong.DefaultIfEmpty()
                    where
                        // 1) Lọc chính xác theo dự án (tận dụng index) nếu có chọn
                        (string.IsNullOrEmpty(maDuAn) || lch.MaDuAn == maDuAn) &&
                        // 2) Tìm kiếm tự do nhiều cột (LIKE)
                        (!hasSearch ||
                            EF.Functions.Like(lch.MaMatKhoi!, pattern!, "\\") ||
                            EF.Functions.Like(lch.TenMatKhoi!, pattern!, "\\") ||
                            // Chỉ cho phép search mờ MaDuAn khi KHÔNG chọn maDuAn
                            (string.IsNullOrEmpty(maDuAn) && EF.Functions.Like(lch.MaDuAn!, pattern!, "\\")) ||
                            EF.Functions.Like(duan2.TenDuAn ?? "", pattern!, "\\")
                        )
                    select new ViewMatKhoiModel
                    {
                        MaMatKhoi = lch.MaMatKhoi,
                        TenMatKhoi = lch.TenMatKhoi,
                        MaDuAn = lch.MaDuAn,
                        TenDuAn = duan2 != null ? duan2.TenDuAn : null,
                        HeSoMatKhoi = lch.HeSoMatKhoi ?? 0
                    };

                return await query
                    .OrderBy(x => x.MaDuAn)
                    .ThenBy(x => x.TenMatKhoi)
                    .ThenBy(x => x.MaMatKhoi)
                    .ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách view mặt khối");
                return new List<ViewMatKhoiModel>();
            }
        }

        private static string EscapeLike(string input)
            => input.Replace("\\", "\\\\")
                    .Replace("%", "\\%")
                    .Replace("_", "\\_");


        #endregion

        #region Them, xoa, sua View mặt khối
        public async Task<ResultModel> SaveViewTrucAsync(ViewMatKhoiModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaDanhMucViewMatKhois.FirstOrDefaultAsync(d => d.MaMatKhoi.ToLower() == model.MaMatKhoi.ToLower());
                if (duAn != null)
                    return ResultModel.Fail("Mã mặt khối đã tồn tại.");

                var record = new DaDanhMucViewMatKhoi
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaMatKhoi = model.MaMatKhoi ?? string.Empty,
                    TenMatKhoi = model.TenMatKhoi ?? string.Empty,
                    HeSoMatKhoi = model.HeSoMatKhoi
                };

                await _context.DaDanhMucViewMatKhois.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm view mặt khối thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm view");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm view: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(ViewMatKhoiModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucViewMatKhois.FirstOrDefaultAsync(d => d.MaMatKhoi == model.MaMatKhoi);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy View mặt khối");
                }
                entity.TenMatKhoi = model.TenMatKhoi ?? string.Empty;
                entity.HeSoMatKhoi = model.HeSoMatKhoi;
                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenMatKhoi} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật view mặt khối");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteViewAsync(string maMatKhoi)
        {
            if (string.IsNullOrWhiteSpace(maMatKhoi))
                return ResultModel.Fail("Thiếu mã mặt khối.");

            maMatKhoi = maMatKhoi.Trim();

            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc trong View Trục (FK: MaViewMatKhoi -> MaMatKhoi)
                var usage = await _context.DaDanhMucViewTrucs.AsNoTracking()
                    .Where(v => v.MaViewMatKhoi == maMatKhoi)
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteViewAsync[MatKhoi]: Check DA_DanhMucViewTruc by MaViewMatKhoi")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    var samples = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaViewMatKhoi == maMatKhoi)
                        .Select(v => new { v.MaTruc, v.TenTruc, v.MaDuAn, v.MaBlock })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[{s.MaTruc}-{s.TenTruc} | DA:{s.MaDuAn} | Block:{s.MaBlock}]"));
                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{((usage!.Count > samples.Count) ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì Mặt khối đang được sử dụng trong View Trục: {usage!.Count} dòng.{hint}");
                }

                // B2) Tìm entity & xoá
                var entity = await _context.DaDanhMucViewMatKhois
                    .FirstOrDefaultAsync(d => d.MaMatKhoi == maMatKhoi);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy mặt khối.");

                _context.DaDanhMucViewMatKhois.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaMatKhoi}] - {entity.TenMatKhoi} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteViewAsync-MatKhoi] Lỗi ràng buộc khi xoá {MaMatKhoi}", maMatKhoi);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ liên kết trong View Trục trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteViewAsync-MatKhoi] Lỗi hệ thống khi xoá {MaMatKhoi}", maMatKhoi);
                return ResultModel.Fail("Lỗi hệ thống khi xoá mặt khối.");
            }
        }


        public async Task<ResultModel> DeleteListAsync(List<ViewMatKhoiModel> listVT)
        {
            const int BatchSize = 1800;

            try
            {
                await using var _context = _factory.CreateDbContext();

                // B0) Normalize & guard
                var ids = (listVT ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaMatKhoi))
                    .Select(x => x!.MaMatKhoi.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc ở DA_DanhMucViewTruc (FK: MaViewMatKhoi -> MaMatKhoi)
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var counts = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaViewMatKhoi != null && chunk.Contains(v.MaViewMatKhoi!))
                        .GroupBy(v => v.MaViewMatKhoi!)
                        .Select(g => new { MaMatKhoi = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync[MatKhoi]: Check DA_DanhMucViewTruc by MaViewMatKhoi")
                        .ToListAsync();

                    foreach (var row in counts)
                        blockedMap[row.MaMatKhoi] =
                            (blockedMap.TryGetValue(row.MaMatKhoi, out var cur) ? cur : 0) + row.Count;
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- Mặt khối [{kv.Key}] đang được sử dụng tại {kv.Value} hàng View Trục."));
                    return ResultModel.Fail("Không thể xoá: tất cả Mặt khối được chọn đều đang được sử dụng trong View Trục.\n" + detail);
                }

                // B2) Xoá DB trong transaction (chỉ xoá các ID không bị ràng buộc)
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await _context.DaDanhMucViewMatKhois
                        .Where(k => chunk.Contains(k.MaMatKhoi))
                        .TagWith("DeleteListAsync[MatKhoi]: ExecuteDelete DaDanhMucViewMatKhois")
                        .ExecuteDeleteAsync();
                }

                await tx.CommitAsync();

                // B3) Thông điệp kết quả
                var totalDeleted = deletableIds.Count;
                var skipped = ids.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} Mặt khối. " +
                    (skipped == 0 ? "" : $"{skipped} Mặt khối không xoá (đang được sử dụng trong View Trục hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv => $"- Mặt khối [{kv.Key}] đang được sử dụng tại {kv.Value} hàng View Trục.");
                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} mặt khối khác." : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách mặt khối");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách mặt khối.");
            }
        }

        // Helper chunk (nếu chưa có)
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin View mặt khối
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from lch in _context.DaDanhMucViewMatKhois
                      join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                      where lch.MaMatKhoi == id
                      select new ViewMatKhoiModel
                      {
                          MaDuAn = lch.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaMatKhoi = lch.MaMatKhoi,
                          TenMatKhoi = lch.TenMatKhoi,
                          HeSoMatKhoi = lch.HeSoMatKhoi??1,
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new ViewMatKhoiModel();
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một View mặt khối trong dự án");
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
                var items = ReadViewMKFromExcel(memoryStream, maDuAn);

                // --- B1: Lọc rỗng + chuẩn hoá ---
                items = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaMatKhoi) && !string.IsNullOrWhiteSpace(x.TenMatKhoi))
                    .Select(x =>
                    {
                        x.MaMatKhoi = x.MaMatKhoi!.Trim().ToUpperInvariant();
                        x.TenMatKhoi = x.TenMatKhoi!.Trim();
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // --- B2: Kiểm tra trùng trong CHÍNH FILE (case-insensitive) ---
                var dupGroups = items
                    .GroupBy(x => x.MaMatKhoi!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (dupGroups.Count > 0)
                {
                    // Lấy tối đa 10 mã trùng + kèm danh sách dòng
                    var lines = dupGroups.Take(10).Select(g =>
                        $"- {g.Key}: dòng {string.Join(", ", g.Select(it => it.RowIndex).OrderBy(i => i))}"
                    );
                    var msg = "Phát hiện mã View mặt khối bị trùng ngay trong file:\n" + string.Join("\n", lines);
                    if (dupGroups.Count > 10) msg += $"\n... và {dupGroups.Count - 10} mã khác.";
                    return ResultModel.Fail(msg);
                }

                // --- B3: Lấy các mã đã tồn tại trong DB ---
                var existingCodes = new HashSet<string>(
                    await _context.DaDanhMucViewMatKhois
                        .Where(x => x.MaMatKhoi != null)
                        .Select(x => x.MaMatKhoi!)
                        .ToListAsync(),
                    StringComparer.OrdinalIgnoreCase
                );

                // --- B4: Lọc ra các bản ghi MỚI chưa có trong DB ---
                var newItems = items
                    .Where(x => !existingCodes.Contains(x.MaMatKhoi!))
                    .ToList();

                if (newItems.Count == 0)
                    return ResultModel.Fail("Không có View mặt khối nào được thêm mới (tất cả mã đã tồn tại trong hệ thống).");

                // --- B5: Insert hàng loạt ---
                var entities = newItems.Select(item => new DaDanhMucViewMatKhoi
                {
                    MaMatKhoi = item.MaMatKhoi!,
                    TenMatKhoi = item.TenMatKhoi!,
                    MaDuAn = item.MaDuAn,
                    HeSoMatKhoi = item.HeSoMatKhoi
                }).ToList();

                await _context.DaDanhMucViewMatKhois.AddRangeAsync(entities);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Import thành công {entities.Count} View mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public List<ViewMatKhoiImportModel> ReadViewMKFromExcel(Stream stream, string maDuAn)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataset.Tables["View"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'View' trong file Excel.");

            var list = new List<ViewMatKhoiImportModel>();
            // Header đang bật, nên dòng dữ liệu đầu tiên là excel row 2
            int excelRow = 2;

            foreach (DataRow row in table.Rows)
            {
                string? maMatKhoi = row[0]?.ToString()?.Trim()?.ToUpperInvariant();
                string? tenMatKhoi = row[1]?.ToString()?.Trim();

                decimal? heSoVT = null;
                var rawHsg = row.Table.Columns.Count > 2 ? row[2]?.ToString()?.Trim() : null;
                if (!string.IsNullOrWhiteSpace(rawHsg) &&
                    decimal.TryParse(rawHsg, NumberStyles.Any, CultureInfo.InvariantCulture, out var hsg))
                {
                    heSoVT = hsg;
                }

                list.Add(new ViewMatKhoiImportModel
                {
                    MaMatKhoi = maMatKhoi,
                    TenMatKhoi = tenMatKhoi,
                    MaDuAn = maDuAn,
                    HeSoMatKhoi = heSoVT,
                    RowIndex = excelRow
                });

                excelRow++;
            }

            return list;
        }
        #endregion
    }
}

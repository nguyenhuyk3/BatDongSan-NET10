using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.LoaiGoc;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class LoaiGocService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<LoaiGocService> _logger;
        private readonly IConfiguration _config;
        public LoaiGocService(IDbContextFactory<AppDbContext> factory, ILogger<LoaiGocService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
        }

        #region Hiển thị toàn bộ loại góc
        public async Task<List<LoaiGocModel>> GetLoaiGocAsync(string maDuAn, string qSearch = null)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var listLCH = await (
                                 from lch in _context.DaDanhMucLoaiGocs
                                 join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn into dtDong
                                 from duan2 in dtDong.DefaultIfEmpty()
                                 where (string.IsNullOrEmpty(maDuAn) || lch.MaDuAn == maDuAn)
                                &&
                       (
                           string.IsNullOrEmpty(qSearch) ||
                           EF.Functions.Like(lch.MaLoaiGoc, $"%{qSearch}%") ||
                           EF.Functions.Like(lch.TenLoaiGoc, $"%{qSearch}%") ||
                           EF.Functions.Like(lch.MaDuAn, $"%{qSearch}%") ||
                           EF.Functions.Like(duan2.TenDuAn, $"%{qSearch}%")
                       )
                                 select new LoaiGocModel
                                 {
                                     MaLoaiGoc = lch.MaLoaiGoc,
                                     TenLoaiGoc = lch.TenLoaiGoc,
                                     MaDuAn = lch.MaDuAn,
                                     TenDuAn = duan2.TenDuAn,
                                     HeSoGoc = lch.HeSoGoc ?? 1
                                 }).ToListAsync();
                return listLCH;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại góc");
            }
            return new List<LoaiGocModel>();
        }
        #endregion

        #region Thêm, xóa, sửa loại góc
        public async Task<ResultModel> SaveLoaiGocAsync(LoaiGocModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var tang = await _context.DaDanhMucLoaiGocs.FirstOrDefaultAsync(d => d.MaLoaiGoc.ToLower() == model.MaLoaiGoc.ToLower());
                if (tang != null)
                    return ResultModel.Fail("Loại góc đã tồn tại.");

                var record = new DaDanhMucLoaiGoc
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaLoaiGoc = model.MaLoaiGoc ?? string.Empty,
                    TenLoaiGoc = model.TenLoaiGoc ?? string.Empty,
                    HeSoGoc = model.HeSoGoc
                };

                await _context.DaDanhMucLoaiGocs.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm loại góc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm loại góc");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm tầng: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(LoaiGocModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucLoaiGocs.FirstOrDefaultAsync(d => d.MaLoaiGoc == model.MaLoaiGoc);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy loại góc.");
                }
                entity.TenLoaiGoc = model.TenLoaiGoc ?? string.Empty;
                entity.HeSoGoc = model.HeSoGoc;
                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenLoaiGoc} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật loại góc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteLoaiGocAsync(string maLoaiGoc)
        {
            if (string.IsNullOrWhiteSpace(maLoaiGoc))
                return ResultModel.Fail("Thiếu mã loại góc.");

            maLoaiGoc = maLoaiGoc.Trim();
            using var _context = _factory.CreateDbContext();
            try
            {
                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc trong View Trục
                var usage = await _context.DaDanhMucViewTrucs.AsNoTracking()
                    .Where(v => v.MaLoaiGoc == maLoaiGoc)
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteLoaiGocAsync: Check DA_DanhMucViewTruc by MaLoaiGoc")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    // Lấy vài dòng ví dụ để báo lỗi cho dễ hiểu
                    var samples = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaLoaiGoc == maLoaiGoc)
                        .Select(v => new { v.MaTruc, v.TenTruc, v.MaDuAn, v.MaBlock })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[{s.MaTruc}-{s.TenTruc} | DA:{s.MaDuAn} | Block:{s.MaBlock}]"));

                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{(usage!.Count > samples.Count ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì Loại góc đang được sử dụng trong View Trục: {usage!.Count} dòng.{hint}");
                }

                // B2) Lấy entity để hiển thị tên khi xoá
                var entity = await _context.DaDanhMucLoaiGocs
                    .FirstOrDefaultAsync(d => d.MaLoaiGoc == maLoaiGoc);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy loại góc.");

                // B3) Xoá & lưu
                _context.DaDanhMucLoaiGocs.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaLoaiGoc}] - {entity.TenLoaiGoc} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteLoaiGocAsync] Lỗi ràng buộc khi xoá {MaLoaiGoc}", maLoaiGoc);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ liên kết trong View Trục trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteLoaiGocAsync] Lỗi hệ thống khi xoá {MaLoaiGoc}", maLoaiGoc);
                return ResultModel.Fail("Lỗi hệ thống khi xoá loại góc.");
            }
        }


        public async Task<ResultModel> DeleteListAsync(List<LoaiGocModel> listLG)
        {
            const int BatchSize = 1800;
            using var _context = _factory.CreateDbContext();
            try
            {
                // --- B0: Normalize & guard ---
                var ids = (listLG ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaLoaiGoc))
                    .Select(x => x!.MaLoaiGoc.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                _context.ChangeTracker.Clear();

                // --- B1: Kiểm tra ràng buộc ở DA_DanhMucViewTruc (theo MaLoaiGoc) ---
                // map: MaLoaiGoc -> count(ViewTruc)
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var counts = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaLoaiGoc != null && chunk.Contains(v.MaLoaiGoc!))
                        .GroupBy(v => v.MaLoaiGoc!)
                        .Select(g => new { MaLoaiGoc = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync[LoaiGoc]: Check DA_DanhMucViewTruc by MaLoaiGoc")
                        .ToListAsync();

                    foreach (var row in counts)
                        blockedMap[row.MaLoaiGoc] = (blockedMap.TryGetValue(row.MaLoaiGoc, out var cur) ? cur : 0) + row.Count;
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- Loại góc [{kv.Key}] đang được sử dụng tại {kv.Value} hàng View Trục."));
                    return ResultModel.Fail("Không thể xoá: tất cả Loại góc được chọn đều đang được sử dụng trong View Trục.\n" + detail);
                }

                // --- B2: Transaction xóa dữ liệu DB (chỉ những ID xoá được) ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await _context.DaDanhMucLoaiGocs
                        .Where(k => chunk.Contains(k.MaLoaiGoc))
                        .TagWith("DeleteListAsync[LoaiGoc]: ExecuteDelete DaDanhMucLoaiGocs")
                        .ExecuteDeleteAsync();
                }

                await tx.CommitAsync();

                // --- B3: Thông điệp kết quả ---
                var totalDeleted = deletableIds.Count;
                var skipped = ids.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} Loại góc. " +
                    (skipped == 0 ? "" : $"{skipped} Loại góc không xoá (đang được sử dụng trong View Trục hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv => $"- Loại góc [{kv.Key}] đang được sử dụng tại {kv.Value} hàng View Trục.");
                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} loại khác." : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách loại góc");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách loại góc.");
            }
        }

        // Helper (nếu chưa có)
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin loại góc
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from lch in _context.DaDanhMucLoaiGocs
                      join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                      where lch.MaLoaiGoc == id
                      select new LoaiGocModel
                      {
                          MaDuAn = lch.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaLoaiGoc = lch.MaLoaiGoc,
                          TenLoaiGoc = lch.TenLoaiGoc,
                          HeSoGoc = lch.HeSoGoc ?? 1
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new LoaiGocModel();
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một căn hộ");
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
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaLoaiGoc) && !string.IsNullOrWhiteSpace(x.TenLoaiGoc))
                    .Select(x =>
                    {
                        x.MaLoaiGoc = x.MaLoaiGoc!.Trim().ToUpperInvariant();
                        x.TenLoaiGoc = x.TenLoaiGoc!.Trim();
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // --- B2: Kiểm tra trùng trong CHÍNH FILE (case-insensitive) ---
                var dupGroups = items
                    .GroupBy(x => x.MaLoaiGoc!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (dupGroups.Count > 0)
                {
                    // Lấy tối đa 10 mã trùng + kèm danh sách dòng
                    var lines = dupGroups.Take(10).Select(g =>
                        $"- {g.Key}: dòng {string.Join(", ", g.Select(it => it.RowIndex).OrderBy(i => i))}"
                    );
                    var msg = "Phát hiện mã Loại Góc bị trùng ngay trong file:\n" + string.Join("\n", lines);
                    if (dupGroups.Count > 10) msg += $"\n... và {dupGroups.Count - 10} mã khác.";
                    return ResultModel.Fail(msg);
                }

                // --- B3: Lấy các mã đã tồn tại trong DB ---
                var existingCodes = new HashSet<string>(
                    await _context.DaDanhMucLoaiGocs
                        .Where(x => x.MaLoaiGoc != null)
                        .Select(x => x.MaLoaiGoc!)
                        .ToListAsync(),
                    StringComparer.OrdinalIgnoreCase
                );

                // --- B4: Lọc ra các bản ghi MỚI chưa có trong DB ---
                var newItems = items
                    .Where(x => !existingCodes.Contains(x.MaLoaiGoc!))
                    .ToList();

                if (newItems.Count == 0)
                    return ResultModel.Fail("Không có Loại Góc nào được thêm mới (tất cả mã đã tồn tại trong hệ thống).");

                // --- B5: Insert hàng loạt ---
                var entities = newItems.Select(item => new DaDanhMucLoaiGoc
                {
                    MaLoaiGoc = item.MaLoaiGoc!,
                    TenLoaiGoc = item.TenLoaiGoc!,
                    MaDuAn = item.MaDuAn,
                    HeSoGoc = item.HeSoGoc
                }).ToList();

                await _context.DaDanhMucLoaiGocs.AddRangeAsync(entities);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Import thành công {entities.Count} Loại Góc mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public List<LoaiGocImportModel> ReadLoaiGocFromExcel(Stream stream, string maDuAn)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataset.Tables["LoaiGoc"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'LoaiGoc' trong file Excel.");

            var list = new List<LoaiGocImportModel>();
            // Header đang bật, nên dòng dữ liệu đầu tiên là excel row 2
            int excelRow = 2;

            foreach (DataRow row in table.Rows)
            {
                string? maLoaiGoc = row[0]?.ToString()?.Trim()?.ToUpperInvariant();
                string? tenLoaiGoc = row[1]?.ToString()?.Trim();

                decimal? heSoGoc = null;
                var rawHsg = row.Table.Columns.Count > 2 ? row[2]?.ToString()?.Trim() : null;
                if (!string.IsNullOrWhiteSpace(rawHsg) &&
                    decimal.TryParse(rawHsg, NumberStyles.Any, CultureInfo.InvariantCulture, out var hsg))
                {
                    heSoGoc = hsg;
                }

                list.Add(new LoaiGocImportModel
                {
                    MaLoaiGoc = maLoaiGoc,
                    TenLoaiGoc = tenLoaiGoc,
                    MaDuAn = maDuAn,
                    HeSoGoc = heSoGoc,
                    RowIndex = excelRow
                });

                excelRow++;
            }

            return list;
        }

        #endregion
    }
}

using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.Huong;
using VTTGROUP.Domain.Model.LoaiDienTich;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class HuongService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<HuongService> _logger;
        private readonly IConfiguration _config;
        private sealed record HuongKey(string MaDuAn, string MaHuong);
        public HuongService(IDbContextFactory<AppDbContext> factory, ILogger<HuongService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
        }

        #region Hiển thị toàn bộ danh sách hướng
        public async Task<List<HuongModel>> GetHuongAsync(string maDuAn, string qSearch = null)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var listVT = await (
                                from lch in _context.DaDanhMucHuongs
                                join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn into dtDong
                                from duan2 in dtDong.DefaultIfEmpty()
                                where (string.IsNullOrEmpty(maDuAn) || lch.MaDuAn == maDuAn)
                              &&
                     (
                         string.IsNullOrEmpty(qSearch) ||
                         EF.Functions.Like(lch.MaHuong, $"%{qSearch}%") ||
                         EF.Functions.Like(lch.TenHuong, $"%{qSearch}%") ||
                         EF.Functions.Like(lch.MaDuAn, $"%{qSearch}%") ||
                         EF.Functions.Like(duan2.TenDuAn, $"%{qSearch}%")
                     )
                                select new HuongModel
                                {
                                    MaHuong = lch.MaHuong,
                                    TenHuong = lch.TenHuong,
                                    MaDuAn = lch.MaDuAn,
                                    TenDuAn = duan2.TenDuAn,
                                    HeSoHuong = lch.HeSo ?? 1,
                                    // Đếm số lượng hướng theo trục
                                    SoLuongVMK = _context.DaDanhMucViewTrucs.Count(t => t.MaHuong == lch.MaHuong && t.MaDuAn == lch.MaDuAn)
                                }).ToListAsync();
                return listVT;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách hướng");
            }
            return new List<HuongModel>();
        }
        #endregion

        #region Them, xoa, sửa hướng
        public async Task<ResultModel> SaveHuongAsync(HuongModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaDanhMucHuongs.FirstOrDefaultAsync(d => d.MaHuong.ToLower() == model.MaHuong.ToLower());
                if (duAn != null)
                    return ResultModel.Fail("Hướng đã tồn tại.");

                var record = new DaDanhMucHuong
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaHuong = model.MaHuong ?? string.Empty,
                    TenHuong = model.TenHuong ?? string.Empty,
                    HeSo = model.HeSoHuong
                };

                await _context.DaDanhMucHuongs.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm hướng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm hướng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm dự án: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByHuongAsync(HuongModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucHuongs.FirstOrDefaultAsync(d => d.MaHuong == model.MaHuong);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy hướng.");
                }
                entity.TenHuong = model.TenHuong ?? string.Empty;
                entity.HeSo = model.HeSoHuong;

                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenHuong} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật hướng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteHuongAsync(string maHuong, string maDuAn)
        {
            if (string.IsNullOrWhiteSpace(maHuong))
                return ResultModel.Fail("Thiếu mã hướng.");

            maHuong = maHuong.Trim();

            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc trong View Trục
                var usage = await _context.DaDanhMucViewTrucs.AsNoTracking()
                    .Where(v => v.MaHuong == maHuong && v.MaDuAn == maDuAn)
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteHuongAsync: Check DA_DanhMucViewTruc by MaHuong")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    var samples = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaHuong == maHuong && v.MaDuAn == maDuAn)
                        .Select(v => new { v.MaTruc, v.TenTruc, v.MaDuAn, v.MaBlock })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[{s.MaTruc}-{s.TenTruc} | DA:{s.MaDuAn} | Block:{s.MaBlock}]"));
                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{((usage!.Count > samples.Count) ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì hướng đang được sử dụng trong View Trục: {usage!.Count} dòng.{hint}");
                }

                // B2) Lấy entity & xoá
                var entity = await _context.DaDanhMucHuongs
                    .FirstOrDefaultAsync(d => d.MaHuong == maHuong && d.MaDuAn == maDuAn);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy hướng.");

                _context.DaDanhMucHuongs.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaHuong}] - {entity.TenHuong} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteHuongAsync] Lỗi ràng buộc khi xoá {MaHuong}", maHuong);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ liên kết trong View Trục trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteHuongAsync] Lỗi hệ thống khi xoá {MaHuong}", maHuong);
                return ResultModel.Fail("Lỗi hệ thống khi xoá hướng.");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<HuongModel> listVT)
        {
            const int BatchSize = 1800;

            try
            {
                await using var _context = _factory.CreateDbContext();

                // --- B0: Chuẩn hoá & guard ---
                // Lấy danh sách cặp (MaDuAn, MaHuong) được chọn
                var selected = (listVT ?? new())
                    .Where(x => x.IsSelected
                                && !string.IsNullOrWhiteSpace(x.MaDuAn)
                                && !string.IsNullOrWhiteSpace(x.MaHuong))
                    .Select(x => new HuongKey(
                        x.MaDuAn!.Trim(),
                        x.MaHuong!.Trim()
                    ))
                    .Distinct()
                    .ToList();

                if (selected.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                _context.ChangeTracker.Clear();

                // --- B1: Kiểm tra ràng buộc ở DA_DanhMucViewTruc (theo MaDuAn + MaHuong) ---
                // map: (MaDuAn, MaHuong) -> số dòng ViewTruc đang dùng
                var blockedMap = new Dictionary<HuongKey, int>();

                // Group theo MaDuAn để query gọn
                var groupsByProject = selected.GroupBy(k => k.MaDuAn);

                foreach (var group in groupsByProject)
                {
                    var maDuAn = group.Key;
                    var huongIds = group.Select(k => k.MaHuong).ToList();

                    foreach (var chunk in Chunk(huongIds, BatchSize))
                    {
                        var counts = await _context.DaDanhMucViewTrucs
                            .AsNoTracking()
                            .Where(v =>
                                v.MaDuAn == maDuAn &&
                                v.MaHuong != null &&
                                chunk.Contains(v.MaHuong!)
                            )
                            .GroupBy(v => v.MaHuong!)
                            .Select(g => new { MaHuong = g.Key, Count = g.Count() })
                            .TagWith("DeleteListAsync[Huong]: Check DA_DanhMucViewTruc by (MaDuAn, MaHuong)")
                            .ToListAsync();

                        foreach (var row in counts)
                        {
                            var key = new HuongKey(maDuAn, row.MaHuong);
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
                            .ThenBy(kv => kv.Key.MaHuong)
                            .Select(kv =>
                                $"- hướng [{kv.Key.MaHuong}] đang được sử dụng tại {kv.Value} hàng View Trục của dự án [{kv.Key.MaDuAn}]."));

                    return ResultModel.Fail(
                        "Không thể xoá: tất cả hướng được chọn đều đang được sử dụng trong View Trục.\n" +
                        detail);
                }

                // --- B2: Xoá DB trong transaction (chỉ xoá các cặp (MaDuAn, MaHuong) không bị ràng buộc) ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var group in deletableKeys.GroupBy(k => k.MaDuAn))
                {
                    var maDuAn = group.Key;
                    var huongIds = group.Select(k => k.MaHuong).ToList();

                    foreach (var chunk in Chunk(huongIds, BatchSize))
                    {
                        _ = await _context.DaDanhMucHuongs
                            .Where(h =>
                                h.MaDuAn == maDuAn &&             // giả định bảng này cũng có MaDuAn
                                chunk.Contains(h.MaHuong))
                            .TagWith("DeleteListAsync[Huong]: ExecuteDelete DaDanhMucHuongs by (MaDuAn, MaHuong)")
                            .ExecuteDeleteAsync();
                    }
                }

                await tx.CommitAsync();

                // --- B3: Thông điệp kết quả ---
                var totalDeleted = deletableKeys.Count;
                var skipped = selected.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{selected.Count} hướng (theo từng dự án). " +
                    (skipped == 0
                        ? ""
                        : $"{skipped} hướng không xoá (đang được sử dụng trong View Trục hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedKeys.Count > 0)
                {
                    var top = blockedMap
                        .OrderBy(kv => kv.Key.MaDuAn)
                        .ThenBy(kv => kv.Key.MaHuong)
                        .Take(10)
                        .Select(kv =>
                            $"- hướng [{kv.Key.MaHuong}] của dự án [{kv.Key.MaDuAn}] đang được sử dụng tại {kv.Value} hàng View Trục.");

                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedKeys.Count > 10
                                        ? $"\n... và {blockedKeys.Count - 10} hướng khác."
                                        : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách hướng");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách hướng.");
            }
        }

        // Helper chunk (nếu chưa có)
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin hướng
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from lch in _context.DaDanhMucHuongs
                      join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                      where lch.MaHuong == id
                      select new HuongModel
                      {
                          MaDuAn = lch.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaHuong = lch.MaHuong,
                          TenHuong = lch.TenHuong,
                          HeSoHuong = lch.HeSo ?? 1
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new HuongModel();
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một hướng trong dự án");
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
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaHuong) && !string.IsNullOrWhiteSpace(x.TenHuong))
                    .Select(x =>
                    {
                        x.MaHuong = x.MaHuong!.Trim().ToUpperInvariant();
                        x.TenHuong = x.TenHuong!.Trim();
                        x.HeSoHuong = x.HeSoHuong;
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // --- B2: Kiểm tra trùng trong CHÍNH FILE (case-insensitive) ---
                var dupGroups = items
                    .GroupBy(x => x.MaHuong!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (dupGroups.Count > 0)
                {
                    // Lấy tối đa 10 mã trùng + kèm danh sách dòng
                    var lines = dupGroups.Take(10).Select(g =>
                        $"- {g.Key}: dòng {string.Join(", ", g.Select(it => it.RowIndex).OrderBy(i => i))}"
                    );
                    var msg = "Phát hiện mã loại diện tích bị trùng ngay trong file:\n" + string.Join("\n", lines);
                    if (dupGroups.Count > 10) msg += $"\n... và {dupGroups.Count - 10} mã khác.";
                    return ResultModel.Fail(msg);
                }

                // --- B3: Lấy các mã đã tồn tại trong DB ---
                var existingCodes = new HashSet<string>(
                    await _context.DaDanhMucHuongs
                        .Where(x => x.MaHuong != null && x.MaDuAn == maDuAn)
                        .Select(x => x.MaHuong!)
                        .ToListAsync(),
                    StringComparer.OrdinalIgnoreCase
                );

                // --- B4: Lọc ra các bản ghi MỚI chưa có trong DB ---
                var newItems = items
                    .Where(x => !existingCodes.Contains(x.MaHuong!))
                    .ToList();

                if (newItems.Count == 0)
                    return ResultModel.Fail("Không có loại diện tích nào được thêm mới (tất cả mã đã tồn tại trong hệ thống).");

                // --- B5: Insert hàng loạt ---
                var entities = newItems.Select(item => new DaDanhMucHuong
                {
                    MaHuong = item.MaHuong!,
                    TenHuong = item.TenHuong!,
                    MaDuAn = item.MaDuAn,
                    HeSo = item.HeSoHuong
                }).ToList();

                await _context.DaDanhMucHuongs.AddRangeAsync(entities);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Import thành công {entities.Count} loại diện tích mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public List<HuongImportModel> ReadLoaiGocFromExcel(Stream stream, string maDuAn)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataset.Tables["Huong"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'Huong' trong file Excel.");

            var list = new List<HuongImportModel>();
            // Header đang bật, nên dòng dữ liệu đầu tiên là excel row 2
            int excelRow = 2;

            foreach (DataRow row in table.Rows)
            {
                string? maHuong = row[0]?.ToString()?.Trim()?.ToUpperInvariant();
                string? tenHuong = row[1]?.ToString()?.Trim();

                decimal? heSoVT = null;
                var rawHsg = row.Table.Columns.Count > 2 ? row[2]?.ToString()?.Trim() : null;
                if (!string.IsNullOrWhiteSpace(rawHsg) &&
                    decimal.TryParse(rawHsg, NumberStyles.Any, CultureInfo.InvariantCulture, out var hsg))
                {
                    heSoVT = hsg;
                }

                list.Add(new HuongImportModel
                {
                    MaHuong = maHuong,
                    TenHuong = tenHuong,
                    MaDuAn = maDuAn,
                    HeSoHuong = heSoVT,
                    RowIndex = excelRow
                });

                excelRow++;
            }

            return list;
        }
        #endregion
    }
}

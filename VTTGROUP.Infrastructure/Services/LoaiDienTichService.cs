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
    public class LoaiDienTichService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<LoaiDienTichService> _logger;
        private readonly IConfiguration _config;
        private sealed record HuongKey(string MaDuAn, string MaLoaiDT);
        public LoaiDienTichService(IDbContextFactory<AppDbContext> factory, ILogger<LoaiDienTichService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
        }

        #region Hiển thị toàn bộ danh sách loại diện tích
        public async Task<List<LoaiDienTichModel>> GetLoaiDienTichAsync(string maDuAn, string qSearch = null)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var listVT = await (
                                from lch in _context.DaLoaiDienTiches
                                join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn into dtDong
                                from duan2 in dtDong.DefaultIfEmpty()
                                where (string.IsNullOrEmpty(maDuAn) || lch.MaDuAn == maDuAn)
                              &&
                     (
                         string.IsNullOrEmpty(qSearch) ||
                         EF.Functions.Like(lch.MaLoaiDt, $"%{qSearch}%") ||
                         EF.Functions.Like(lch.TenLoaiDt, $"%{qSearch}%") ||
                         EF.Functions.Like(lch.MaDuAn, $"%{qSearch}%") ||
                         EF.Functions.Like(duan2.TenDuAn, $"%{qSearch}%")
                     )
                                select new LoaiDienTichModel
                                {
                                    MaLoai = lch.MaLoaiDt,
                                    TenLoai = lch.TenLoaiDt,
                                    MaDuAn = lch.MaDuAn,
                                    TenDuAn = duan2.TenDuAn,
                                    HeSoLoaiDT = lch.HeSo ?? 1,
                                    // Đếm số lượng loại diện tích theo sản phẩm
                                    SoLuongSP = _context.DaDanhMucSanPhams.Count(t => t.MaLoaiDienTich == lch.MaLoaiDt && t.MaDuAn == lch.MaDuAn)
                                }).ToListAsync();
                return listVT;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại diện tích");
            }
            return new List<LoaiDienTichModel>();
        }
        #endregion

        #region Thêm, xóa, sửa loại diện tích
        public async Task<ResultModel> SaveAsync(LoaiDienTichModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaLoaiDienTiches.FirstOrDefaultAsync(d => d.MaLoaiDt.ToLower() == model.MaLoai.ToLower());
                if (duAn != null)
                    return ResultModel.Fail("loại diện tích đã tồn tại.");

                var record = new DaLoaiDienTich
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaLoaiDt = model.MaLoai ?? string.Empty,
                    TenLoaiDt = model.TenLoai ?? string.Empty,
                    HeSo = model.HeSoLoaiDT
                };

                await _context.DaLoaiDienTiches.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm loại diện tích thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm loại diện tích");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm dự án: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByAsync(LoaiDienTichModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaLoaiDienTiches.FirstOrDefaultAsync(d => d.MaLoaiDt == model.MaLoai);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy loại diện tích.");
                }
                entity.TenLoaiDt = model.TenLoai ?? string.Empty;
                entity.HeSo = model.HeSoLoaiDT;

                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenLoaiDt} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật loại diện tích");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteLDTAsync(string maLoaiDT, string maDuAn)
        {
            if (string.IsNullOrWhiteSpace(maLoaiDT))
                return ResultModel.Fail("Thiếu mã loại diện tích.");

            maLoaiDT = maLoaiDT.Trim();

            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc trong Sản phẩmm
                var usage = await _context.DaDanhMucSanPhams.AsNoTracking()
                    .Where(v => v.MaLoaiDienTich == maLoaiDT && v.MaDuAn == maDuAn)
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteHuongAsync: Check DaDanhMucSanPhams by MaLoaiDienTich")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    var samples = await _context.DaDanhMucSanPhams.AsNoTracking()
                        .Where(v => v.MaLoaiDienTich == maLoaiDT && v.MaDuAn == maDuAn)
                        .Select(v => new { v.MaSanPham, v.TenSanPham, v.MaDuAn, v.MaBlock })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[{s.MaSanPham}-{s.TenSanPham} | DA:{s.MaDuAn} | Block:{s.MaBlock}]"));
                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{((usage!.Count > samples.Count) ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì loại diện tích đang được sử dụng trong Sản phẩm: {usage!.Count} dòng.{hint}");
                }

                // B2) Lấy entity & xoá
                var entity = await _context.DaLoaiDienTiches
                    .FirstOrDefaultAsync(d => d.MaLoaiDt == maLoaiDT && d.MaDuAn == maDuAn);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy loại diện tích.");

                _context.DaLoaiDienTiches.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaLoaiDt}] - {entity.TenLoaiDt} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteHuongAsync] Lỗi ràng buộc khi xoá {MaLoaiDt}", maLoaiDT);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ liên kết trong Sản phẩm trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteHuongAsync] Lỗi hệ thống khi xoá {MaLoaiDt}", maLoaiDT);
                return ResultModel.Fail("Lỗi hệ thống khi xoá loại diện tích.");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<LoaiDienTichModel> listVT)
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
                                && !string.IsNullOrWhiteSpace(x.MaLoai))
                    .Select(x => new HuongKey(
                        x.MaDuAn!.Trim(),
                        x.MaLoai!.Trim()
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
                    var huongIds = group.Select(k => k.MaLoaiDT).ToList();

                    foreach (var chunk in Chunk(huongIds, BatchSize))
                    {
                        var counts = await _context.DaDanhMucSanPhams
                            .AsNoTracking()
                            .Where(v =>
                                v.MaDuAn == maDuAn &&
                                v.MaLoaiDienTich != null &&
                                chunk.Contains(v.MaLoaiDienTich!)
                            )
                            .GroupBy(v => v.MaLoaiDienTich!)
                            .Select(g => new { MaLoaiDT = g.Key, Count = g.Count() })
                            .TagWith("DeleteListAsync[Huong]: Check DaDanhMucSanPhams by (MaDuAn, MaLoaiDT)")
                            .ToListAsync();

                        foreach (var row in counts)
                        {
                            var key = new HuongKey(maDuAn, row.MaLoaiDT);
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
                            .ThenBy(kv => kv.Key.MaLoaiDT)
                            .Select(kv =>
                                $"- loại diện tích [{kv.Key.MaLoaiDT}] đang được sử dụng tại {kv.Value} hàng Sản phẩm của dự án [{kv.Key.MaDuAn}]."));

                    return ResultModel.Fail(
                        "Không thể xoá: tất cả loại diện tích được chọn đều đang được sử dụng trong Sản phẩm.\n" +
                        detail);
                }

                // --- B2: Xoá DB trong transaction (chỉ xoá các cặp (MaDuAn, MaHuong) không bị ràng buộc) ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var group in deletableKeys.GroupBy(k => k.MaDuAn))
                {
                    var maDuAn = group.Key;
                    var huongIds = group.Select(k => k.MaLoaiDT).ToList();

                    foreach (var chunk in Chunk(huongIds, BatchSize))
                    {
                        _ = await _context.DaLoaiDienTiches
                            .Where(h =>
                                h.MaDuAn == maDuAn &&             // giả định bảng này cũng có MaDuAn
                                chunk.Contains(h.MaLoaiDt))
                            .TagWith("DeleteListAsync[Huong]: ExecuteDelete DaLoaiDienTiches by (MaDuAn, MaHuong)")
                            .ExecuteDeleteAsync();
                    }
                }

                await tx.CommitAsync();

                // --- B3: Thông điệp kết quả ---
                var totalDeleted = deletableKeys.Count;
                var skipped = selected.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{selected.Count} loại diện tích (theo từng dự án). " +
                    (skipped == 0
                        ? ""
                        : $"{skipped} loại diện tích không xoá (đang được sử dụng trong View Trục hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedKeys.Count > 0)
                {
                    var top = blockedMap
                        .OrderBy(kv => kv.Key.MaDuAn)
                        .ThenBy(kv => kv.Key.MaLoaiDT)
                        .Take(10)
                        .Select(kv =>
                            $"- loại diện tích [{kv.Key.MaLoaiDT}] của dự án [{kv.Key.MaDuAn}] đang được sử dụng tại {kv.Value} hàng Sản phẩm.");

                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedKeys.Count > 10
                                        ? $"\n... và {blockedKeys.Count - 10} loại diện tích khác."
                                        : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách loại diện tích");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách loại diện tích.");
            }
        }

        // Helper chunk (nếu chưa có)
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin loại diện tích
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from lch in _context.DaLoaiDienTiches
                      join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                      where lch.MaLoaiDt == id
                      select new LoaiDienTichModel
                      {
                          MaDuAn = lch.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaLoai = lch.MaLoaiDt,
                          TenLoai = lch.TenLoaiDt,
                          HeSoLoaiDT = lch.HeSo ?? 1
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new LoaiDienTichModel();
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một loại diện tích trong dự án");
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
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaLoai) && !string.IsNullOrWhiteSpace(x.TenLoai))
                    .Select(x =>
                    {
                        x.MaLoai = x.MaLoai!.Trim().ToUpperInvariant();
                        x.TenLoai = x.TenLoai!.Trim();
                        x.HeSoLoaiDT = x.HeSoLoaiDT;
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // --- B2: Kiểm tra trùng trong CHÍNH FILE (case-insensitive) ---
                var dupGroups = items
                    .GroupBy(x => x.MaLoai!, StringComparer.OrdinalIgnoreCase)
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
                    await _context.DaLoaiDienTiches
                        .Where(x => x.MaLoaiDt != null && x.MaDuAn == maDuAn)
                        .Select(x => x.MaLoaiDt!)
                        .ToListAsync(),
                    StringComparer.OrdinalIgnoreCase
                );

                // --- B4: Lọc ra các bản ghi MỚI chưa có trong DB ---
                var newItems = items
                    .Where(x => !existingCodes.Contains(x.MaLoai!))
                    .ToList();

                if (newItems.Count == 0)
                    return ResultModel.Fail("Không có loại diện tích nào được thêm mới (tất cả mã đã tồn tại trong hệ thống).");

                // --- B5: Insert hàng loạt ---
                var entities = newItems.Select(item => new DaLoaiDienTich
                {
                    MaLoaiDt = item.MaLoai!,
                    TenLoaiDt = item.TenLoai!,
                    MaDuAn = item.MaDuAn,
                    HeSo = item.HeSoLoaiDT
                }).ToList();

                await _context.DaLoaiDienTiches.AddRangeAsync(entities);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Import thành công {entities.Count} loại diện tích mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public List<LoaiDienTichImportModel> ReadLoaiGocFromExcel(Stream stream, string maDuAn)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataset.Tables["LoaiDienTich"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'LoaiDienTich' trong file Excel.");

            var list = new List<LoaiDienTichImportModel>();
            // Header đang bật, nên dòng dữ liệu đầu tiên là excel row 2
            int excelRow = 2;

            foreach (DataRow row in table.Rows)
            {
                string? maLoai = row[0]?.ToString()?.Trim()?.ToUpperInvariant();
                string? tenLoai = row[1]?.ToString()?.Trim();

                decimal? heSoVT = null;
                var rawHsg = row.Table.Columns.Count > 2 ? row[2]?.ToString()?.Trim() : null;
                if (!string.IsNullOrWhiteSpace(rawHsg) &&
                    decimal.TryParse(rawHsg, NumberStyles.Any, CultureInfo.InvariantCulture, out var hsg))
                {
                    heSoVT = hsg;
                }

                list.Add(new LoaiDienTichImportModel
                {
                    MaLoai = maLoai,
                    TenLoai = tenLoai,
                    MaDuAn = maDuAn,
                    HeSoLoaiDT = heSoVT,
                    RowIndex = excelRow
                });

                excelRow++;
            }

            return list;
        }
        #endregion
    }
}

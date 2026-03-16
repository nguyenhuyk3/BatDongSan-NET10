using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.View;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ViewService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<ViewService> _logger;
        private readonly IConfiguration _config;
        public ViewService(IDbContextFactory<AppDbContext> factory, ILogger<ViewService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
        }

        #region Hiển thị toàn bộ danh sách View
        public async Task<List<ViewModel>> GetViewAsync(string maDuAn, string qSearch = null)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var listVT = await (
                                from lch in _context.DaDanhMucViews
                                join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn into dtDong
                                from duan2 in dtDong.DefaultIfEmpty()
                                where (string.IsNullOrEmpty(maDuAn) || lch.MaDuAn == maDuAn)
                             &&
                    (
                        string.IsNullOrEmpty(qSearch) ||
                        EF.Functions.Like(lch.MaView, $"%{qSearch}%") ||
                        EF.Functions.Like(lch.TenView, $"%{qSearch}%") ||
                        EF.Functions.Like(lch.MaDuAn, $"%{qSearch}%") ||
                        EF.Functions.Like(duan2.TenDuAn, $"%{qSearch}%")
                    )
                                select new ViewModel
                                {
                                    MaView = lch.MaView,
                                    TenView = lch.TenView,
                                    MaDuAn = lch.MaDuAn,
                                    TenDuAn = duan2.TenDuAn,
                                    HeSoView = lch.HeSoView
                                }).ToListAsync();
                return listVT;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách view");
            }
            return new List<ViewModel>();
        }
        #endregion

        #region Them, xoa, sua View
        public async Task<ResultModel> SaveViewAsync(ViewModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaDanhMucViews.FirstOrDefaultAsync(d => d.MaView.ToLower() == model.MaView.ToLower());
                if (duAn != null)
                    return ResultModel.Fail("Vị trí đã tồn tại.");

                var record = new DaDanhMucView
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaView = model.MaView ?? string.Empty,
                    TenView = model.TenView ?? string.Empty,
                    HeSoView = model.HeSoView
                };

                await _context.DaDanhMucViews.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm view thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm view");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm view: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(ViewModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucViews.FirstOrDefaultAsync(d => d.MaView == model.MaView);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy View");
                }
                entity.TenView = model.TenView ?? string.Empty;
                entity.HeSoView = model.HeSoView ?? 1;

                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenView} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật View");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteViewAsync(string maView)
        {
            if (string.IsNullOrWhiteSpace(maView))
                return ResultModel.Fail("Thiếu mã View.");

            maView = maView.Trim();

            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc trong View Trục
                var usage = await _context.DaDanhMucViewTrucs.AsNoTracking()
                    .Where(v => v.MaLoaiView == maView)      // nếu cột khác, đổi tại đây
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteViewAsync: Check DA_DanhMucViewTruc by MaLoaiView")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    var samples = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaLoaiView == maView)
                        .Select(v => new { v.MaTruc, v.TenTruc, v.MaDuAn, v.MaBlock })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[{s.MaTruc}-{s.TenTruc} | DA:{s.MaDuAn} | Block:{s.MaBlock}]"));
                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{((usage!.Count > samples.Count) ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì View đang được sử dụng trong View Trục: {usage!.Count} dòng.{hint}");
                }

                // B2) Lấy entity & xoá
                var entity = await _context.DaDanhMucViews
                    .FirstOrDefaultAsync(d => d.MaView == maView);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy View.");

                _context.DaDanhMucViews.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaView}] - {entity.TenView} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteViewAsync] Lỗi ràng buộc khi xoá {MaView}", maView);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ liên kết trong View Trục trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteViewAsync] Lỗi hệ thống khi xoá {MaView}", maView);
                return ResultModel.Fail("Lỗi hệ thống khi xoá View.");
            }
        }


        public async Task<ResultModel> DeleteListAsync(List<ViewModel> listVT)
        {
            const int BatchSize = 1800;

            try
            {
                await using var _context = _factory.CreateDbContext();

                // B0) Normalize & guard
                var ids = (listVT ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaView))
                    .Select(x => x!.MaView.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc ở DA_DanhMucViewTruc (FK: MaLoaiView -> MaView)
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var counts = await _context.DaDanhMucViewTrucs.AsNoTracking()
                        .Where(v => v.MaLoaiView != null && chunk.Contains(v.MaLoaiView!))
                        .GroupBy(v => v.MaLoaiView!)
                        .Select(g => new { MaView = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync[View]: Check DA_DanhMucViewTruc by MaLoaiView")
                        .ToListAsync();

                    foreach (var row in counts)
                        blockedMap[row.MaView] = (blockedMap.TryGetValue(row.MaView, out var cur) ? cur : 0) + row.Count;
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- View [{kv.Key}] đang được sử dụng tại {kv.Value} hàng ViewTruc."));
                    return ResultModel.Fail("Không thể xoá: tất cả View được chọn đều đang được sử dụng trong ViewTruc.\n" + detail);
                }

                // B2) Xoá DB trong transaction (chỉ những ID không bị ràng buộc)
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await _context.DaDanhMucViews
                        .Where(k => chunk.Contains(k.MaView))
                        .TagWith("DeleteListAsync[View]: ExecuteDelete DaDanhMucViews")
                        .ExecuteDeleteAsync();
                }

                await tx.CommitAsync();

                // B3) Kết quả
                var totalDeleted = deletableIds.Count;
                var skipped = ids.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} view. " +
                    (skipped == 0 ? "" : $"{skipped} view không xoá (đang được sử dụng trong ViewTruc hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv => $"- View [{kv.Key}] đang được sử dụng tại {kv.Value} hàng ViewTruc.");
                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} view khác." : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách view");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách view.");
            }
        }

        // Helper
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin View
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from lch in _context.DaDanhMucViews
                      join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                      where lch.MaView == id
                      select new ViewModel
                      {
                          MaDuAn = lch.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaView = lch.MaView,
                          TenView = lch.TenView,
                          HeSoView = lch.HeSoView
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new ViewModel();
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một View trong dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin View
        public async Task<List<ViewModel>> GetByDuAnAsync(string maDuAn)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from lch in _context.DaDanhMucViews
                      where lch.MaDuAn == maDuAn
                      select new ViewModel
                      {
                          MaView = lch.MaView,
                          TenView = lch.TenView,
                          ThuTuHienThi = lch.ThuTuHienThi
                      })
                      .OrderBy(d => d.ThuTuHienThi)
                      .ToListAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByDuAnAsync] Lỗi khi lấy thông tin View trong dự án");
                return new List<ViewModel>();
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
                var items = ReadViewFromExcel(memoryStream, maDuAn);

                // --- B1: Lọc rỗng + chuẩn hoá ---
                items = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaView) && !string.IsNullOrWhiteSpace(x.TenView))
                    .Select(x =>
                    {
                        x.MaView = x.MaView!.Trim().ToUpperInvariant();
                        x.TenView = x.TenView!.Trim();
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // --- B2: Kiểm tra trùng trong CHÍNH FILE (case-insensitive) ---
                var dupGroups = items
                    .GroupBy(x => x.MaView!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .ToList();

                if (dupGroups.Count > 0)
                {
                    // Lấy tối đa 10 mã trùng + kèm danh sách dòng
                    var lines = dupGroups.Take(10).Select(g =>
                        $"- {g.Key}: dòng {string.Join(", ", g.Select(it => it.RowIndex).OrderBy(i => i))}"
                    );
                    var msg = "Phát hiện mã View bị trùng ngay trong file:\n" + string.Join("\n", lines);
                    if (dupGroups.Count > 10) msg += $"\n... và {dupGroups.Count - 10} mã khác.";
                    return ResultModel.Fail(msg);
                }

                // --- B3: Lấy các mã đã tồn tại trong DB ---
                var existingCodes = new HashSet<string>(
                    await _context.DaDanhMucViews
                        .Where(x => x.MaView != null)
                        .Select(x => x.MaView!)
                        .ToListAsync(),
                    StringComparer.OrdinalIgnoreCase
                );

                // --- B4: Lọc ra các bản ghi MỚI chưa có trong DB ---
                var newItems = items
                    .Where(x => !existingCodes.Contains(x.MaView!))
                    .ToList();

                if (newItems.Count == 0)
                    return ResultModel.Fail("Không có View nào được thêm mới (tất cả mã đã tồn tại trong hệ thống).");

                // --- B5: Insert hàng loạt ---
                var entities = newItems.Select(item => new DaDanhMucView
                {
                    MaView = item.MaView!,
                    TenView = item.TenView!,
                    MaDuAn = item.MaDuAn,
                    // HeSoView = item.HeSoView
                }).ToList();

                await _context.DaDanhMucViews.AddRangeAsync(entities);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Import thành công {entities.Count} View mới.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public List<ViewImportModel> ReadViewFromExcel(Stream stream, string maDuAn)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataset.Tables["ViewMatKhoi"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'ViewMatKhoi' trong file Excel.");

            var list = new List<ViewImportModel>();
            // Header đang bật, nên dòng dữ liệu đầu tiên là excel row 2
            int excelRow = 2;

            foreach (DataRow row in table.Rows)
            {
                string? maView = row[0]?.ToString()?.Trim()?.ToUpperInvariant();
                string? tenView = row[1]?.ToString()?.Trim();

                //decimal? heSoVT = null;
                //var rawHsg = row.Table.Columns.Count > 2 ? row[2]?.ToString()?.Trim() : null;
                //if (!string.IsNullOrWhiteSpace(rawHsg) &&
                //    decimal.TryParse(rawHsg, NumberStyles.Any, CultureInfo.InvariantCulture, out var hsg))
                //{
                //    heSoVT = hsg;
                //}

                list.Add(new ViewImportModel
                {
                    MaView = maView,
                    TenView = tenView,
                    MaDuAn = maDuAn,
                    //HeSoView = heSoVT,
                    RowIndex = excelRow
                });

                excelRow++;
            }

            return list;
        }
        #endregion
    }
}

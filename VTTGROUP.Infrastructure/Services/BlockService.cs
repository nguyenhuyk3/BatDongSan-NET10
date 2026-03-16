using ClosedXML.Excel;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.Block;
using VTTGROUP.Domain.Model.PhieuDatCoc;
using VTTGROUP.Domain.Model.SanPham;
using VTTGROUP.Domain.Model.TongHopBooking;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class BlockService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<BlockService> _logger;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        public BlockService(IDbContextFactory<AppDbContext> factory, ILogger<BlockService> logger, IConfiguration config, IMemoryCache cache)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _cache = cache;
        }

        #region Hiển thị toàn bộ danh sách block theo dự án
        public async Task<List<BlockModel>> GetBlockAsync(string maDuAn, string qSearch = null)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var query =
                                 from block in context.DaDanhMucBlocks.AsNoTracking()
                                 join duan in context.DaDanhMucDuAns.AsNoTracking() on block.MaDuAn equals duan.MaDuAn into dtDong
                                 from duan2 in dtDong.DefaultIfEmpty()
                                 where
                       (string.IsNullOrEmpty(maDuAn) || block.MaDuAn == maDuAn) &&
                       (
                           string.IsNullOrEmpty(qSearch) ||
                           EF.Functions.Like(block.MaBlock, $"%{qSearch}%") ||
                           EF.Functions.Like(block.TenBlock, $"%{qSearch}%") ||
                           EF.Functions.Like(duan2.MaDuAn, $"%{qSearch}%") ||
                           EF.Functions.Like(duan2.TenDuAn, $"%{qSearch}%")
                       )
                                 select new BlockModel
                                 {
                                     MaDuAn = block.MaDuAn,
                                     MaBlock = block.MaBlock,
                                     TenBlock = block.TenBlock,
                                     TenDuAn = duan2.TenDuAn
                                 };
                // Sắp xếp nếu cần
                query = query.OrderBy(x => x.MaDuAn).ThenBy(x => x.MaBlock);
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách block");
            }
            return new List<BlockModel>();
        }
        #endregion

        #region Thêm, xóa, sửa Block
        public async Task<ResultModel> SaveBlockAsync(BlockModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var block = await context.DaDanhMucBlocks.FirstOrDefaultAsync(d => d.MaBlock.ToLower() == model.MaBlock.ToLower());
                if (block != null)
                    return ResultModel.Fail("Block đã tồn tại.");

                var record = new DaDanhMucBlock
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaBlock = model.MaBlock ?? string.Empty,
                    TenBlock = model.TenBlock ?? string.Empty
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
                //            MaPhieu = model.MaBlock ?? string.Empty,
                //            TenFileDinhKem = file.FileName,
                //            TenFileDinhKemLuu = savedPath,
                //            TaiLieuUrl = savedPath,
                //            Controller = "Block",
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
                //    await context.HtFileDinhKems.AddRangeAsync(listFiles);
                //}
                await context.DaDanhMucBlocks.AddAsync(record);
                await context.SaveChangesAsync();
                return ResultModel.Success("Thêm block thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm dự án");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm dự án: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(BlockModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await context.DaDanhMucBlocks.FirstOrDefaultAsync(d => d.MaBlock == model.MaBlock);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy block.");
                }
                entity.MaDuAn = model.MaDuAn ?? string.Empty;
                entity.TenBlock = model.TenBlock ?? string.Empty;
                //List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                //var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaDuAn && d.Controller == "Block").ToListAsync();

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
                //            MaPhieu = model.MaBlock,
                //            TenFileDinhKem = file.FileName,
                //            TenFileDinhKemLuu = savedPath,
                //            TaiLieuUrl = savedPath,
                //            Controller = "Block",
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

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenBlock} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteBlockAsync(string maBlock, CancellationToken ct = default)
        {
            // --- Guard: validate đầu vào ---
            maBlock = maBlock?.Trim();
            if (string.IsNullOrWhiteSpace(maBlock))
                return ResultModel.Fail("Mã Block không hợp lệ.");

            try
            {
                await using var context = _factory.CreateDbContext();

                // Tuỳ bạn: nếu Block có child (căn hộ, tầng...), nên kiểm tra trước để báo lỗi có hướng dẫn
                // Ví dụ:
                var hasChildren = await context.DaDanhMucTangs.AnyAsync(x => x.MaBlock == maBlock, ct);
                if (hasChildren) return ResultModel.Fail("Block đang được sử dụng (tầng/căn hộ...). Vui lòng xoá/liên kết trước.");

                // Xoá trực tiếp trên DB (không materialize entity) -> nhanh & gọn
                var affected = await context.DaDanhMucBlocks
                    .Where(b => b.MaBlock == maBlock)
                    .TagWith("DeleteBlockAsync")            // hỗ trợ truy vết trong log/sql
                    .ExecuteDeleteAsync(ct);

                if (affected == 0)
                    return ResultModel.Fail("Không tìm thấy Block.");

                // Gợi ý: nếu cần trả tên block, bạn có thể query trước 1 field duy nhất:
                // var ten = await context.DaDanhMucBlocks
                //     .Where(b => b.MaBlock == maBlock)
                //     .Select(b => b.TenBlock)
                //     .FirstOrDefaultAsync(ct);

                return ResultModel.Success($"Xoá Block [{maBlock}] thành công.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteBlockAsync bị huỷ (maBlock={MaBlock})", maBlock);
                return ResultModel.Fail("Tác vụ đã bị huỷ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteBlockAsync] Lỗi khi xoá Block (maBlock={MaBlock})", maBlock);
                return ResultModel.Fail("Lỗi hệ thống khi xoá Block.");
            }
        }
        public async Task<ResultModel> DeleteListAsync(
     List<BlockModel> listBlock,
     CancellationToken ct = default)
        {
            // --- Normalize & guard ---
            var ids = (listBlock ?? new())
                .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaBlock))
                .Select(x => x!.MaBlock.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (ids.Count == 0)
                return ResultModel.Success("Không có dòng nào được chọn để xoá.");

            const int BatchSize = 1800; // an toàn dưới ngưỡng 2100 tham số

            try
            {
                await using var context = _factory.CreateDbContext();

                // ============ B1) KIỂM TRA RÀNG BUỘC (Tầng/Căn hộ) ============
                // Gom kết quả vướng ràng buộc theo MaBlock: { MaBlock -> (tangCount, canHoCount) }
                var blockedMap = new Dictionary<string, (int tang, int canHo)>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    // --- Check Tầng ---
                    var tangCounts = await context.DaDanhMucTangs.AsNoTracking()
                        .Where(t => chunk.Contains(t.MaBlock))
                        .GroupBy(t => t.MaBlock)
                        .Select(g => new { MaBlock = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync: Check DaDanhMucTangs by MaBlock")
                        .ToListAsync(ct);

                    foreach (var row in tangCounts)
                    {
                        if (!blockedMap.TryGetValue(row.MaBlock, out var cur))
                            blockedMap[row.MaBlock] = (row.Count, 0);
                        else
                            blockedMap[row.MaBlock] = (cur.tang + row.Count, cur.canHo);
                    }

                    // --- Check Căn hộ / Sản phẩm (nếu có bảng khác, sửa lại tại đây) ---
                    var canHoCounts = await context.DaDanhMucSanPhams.AsNoTracking()
                        .Where(sp => chunk.Contains(sp.MaBlock))
                        .GroupBy(sp => sp.MaBlock)
                        .Select(g => new { MaBlock = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync: Check DaDanhMucSanPhams by MaBlock")
                        .ToListAsync(ct);

                    foreach (var row in canHoCounts)
                    {
                        if (!blockedMap.TryGetValue(row.MaBlock, out var cur))
                            blockedMap[row.MaBlock] = (0, row.Count);
                        else
                            blockedMap[row.MaBlock] = (cur.tang, cur.canHo + row.Count);
                    }
                }

                // Những Block bị vướng ràng buộc
                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Những Block có thể xoá
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    // Không xoá được gì: trả về thông điệp chi tiết các block bị vướng
                    var lines = blockedMap
                        .OrderBy(kv => kv.Key)
                        .Select(kv =>
                        {
                            var (tang, canHo) = kv.Value;
                            return $"- Block [{kv.Key}] đang được sử dụng: {tang} tầng, {canHo} căn hộ.";
                        });

                    var detail = string.Join(Environment.NewLine, lines);
                    return ResultModel.Fail(
                        "Không thể xoá: tất cả Block được chọn đều đang được sử dụng.\n" + detail);
                }

                // ============ B2) XOÁ TRONG TRANSACTION CHỈ NHỮNG BLOCK KHÔNG VƯỚNG ============
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                var totalDeleted = 0;
                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    var affected = await context.DaDanhMucBlocks
                        .Where(b => chunk.Contains(b.MaBlock))
                        .TagWith("DeleteListAsync(BulkDelete DaDanhMucBlocks) - Deletable only")
                        .ExecuteDeleteAsync(ct);

                    totalDeleted += affected;
                }

                await tx.CommitAsync(ct);

                // ============ B3) THÔNG ĐIỆP TỔNG HỢP ============
                var skipped = ids.Count - totalDeleted; // gồm cả vướng ràng buộc + không tồn tại
                if (blockedIds.Count > 0)
                {
                    var topDetail = blockedMap
                        .OrderBy(kv => kv.Key)
                        .Take(10) // tránh trả về quá dài; tuỳ ý
                        .Select(kv =>
                        {
                            var (tang, canHo) = kv.Value;
                            return $"- Block [{kv.Key}] đang được sử dụng: {tang} tầng, {canHo} căn hộ.";
                        });

                    var detail = string.Join(Environment.NewLine, topDetail);
                    var more = blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} block khác." : string.Empty;

                    return ResultModel.Success(
                        $"Đã xoá {totalDeleted}/{ids.Count} Block. {skipped} Block không xoá (đang được sử dụng hoặc không tồn tại).\n" +
                        $"Chi tiết Block bị ràng buộc:\n{detail}{more}");
                }

                // Không có ràng buộc, nhưng có thể có Block không tồn tại
                var msg = skipped == 0
                    ? $"Đã xoá {totalDeleted}/{ids.Count} Block thành công."
                    : $"Đã xoá {totalDeleted}/{ids.Count} Block. {skipped} Block không tồn tại hoặc đã bị xoá trước đó.";
                return ResultModel.Success(msg);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("DeleteListAsync bị huỷ. Count={Count}", ids.Count);
                return ResultModel.Fail("Tác vụ đã bị huỷ.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteListAsync] Lỗi ràng buộc khi xoá danh sách Block. Count={Count}", ids.Count);
                return ResultModel.Fail("Không thể xoá vì Block đang được sử dụng (ràng buộc dữ liệu). Vui lòng gỡ liên kết/tầng/căn hộ trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách Block. Count={Count}", ids.Count);
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách Block.");
            }
        }

        // --- Helper: chia mảng theo batch an toàn tham số SQL ---
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return new List<T>(source.Skip(i).Take(size));
        }      

        #endregion

        #region Thông tin block
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await (
                      from block in context.DaDanhMucBlocks
                      join duan in context.DaDanhMucDuAns on block.MaDuAn equals duan.MaDuAn
                      where block.MaBlock == id
                      select new BlockModel
                      {
                          MaDuAn = block.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaBlock = block.MaBlock,
                          TenBlock = block.TenBlock
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new BlockModel();
                }
                //var files = await context.HtFileDinhKems.Where(d => d.Controller == "Block" && d.MaPhieu == id).Select(d => new UploadedFileModel
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
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một block");
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

        #region Get lock theo dự án
        public async Task<List<DaDanhMucBlock>> GetBlocksByMaDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucBlock>();
            try
            {
                using var context = _factory.CreateDbContext();
                entity = await context.DaDanhMucBlocks
                                          .Where(d => d.MaDuAn == maDuAn)
                                          .ToListAsync();

                if (entity == null)
                    entity = new List<DaDanhMucBlock>();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetBlocksByMaDuAnAsync] Lỗi khi lấy toàn bộ danh sách block theo dự án");
            }
            return entity;
        }

        //Lấy danh sách block theo dự án và những block trong giỏ hàng
        public async Task<List<DaDanhMucBlock>> GetBlocksByDuAn_TheoDanhSachTenBlockAsync(
    string maDuAn,
    IEnumerable<string> tenBlocks,
    CancellationToken ct = default)
        {
            try
            {
                var keys = (tenBlocks ?? Enumerable.Empty<string>())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct()
                    .ToList();

                if (string.IsNullOrWhiteSpace(maDuAn) || keys.Count == 0)
                    return new List<DaDanhMucBlock>();

                await using var context = _factory.CreateDbContext();

                // SQL Server thường collation CI nên Contains là ok (case-insensitive).
                var query = context.DaDanhMucBlocks.AsNoTracking()
                    .Where(d => d.MaDuAn == maDuAn && keys.Contains(d.TenBlock));

                return await query.ToListAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetBlocksByDuAn_TheoDanhSachTenBlockAsync] Error");
                return new List<DaDanhMucBlock>();
            }
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
                memoryStream.Position = 0; // Reset lại vị trí đọc

                var items = ReadBlockFromExcel(memoryStream, maDuAn);

                var existingMaSanPham = await _context.DaDanhMucBlocks
                    .Where(x => x.MaBlock != null)
                    .Select(x => x.MaBlock!)
                    .ToListAsync();

                var newItems = items
                    .Where(x => !string.IsNullOrEmpty(x.MaBlock))
                    .Where(x => !existingMaSanPham.Contains(x.MaBlock!))
                    .ToList();

                if (newItems.Count == 0)
                    return ResultModel.Fail("Không có block nào được thêm mới.");

                foreach (var item in newItems)
                {
                    _context.DaDanhMucBlocks.Add(new DaDanhMucBlock
                    {
                        MaBlock = item.MaBlock,
                        TenBlock = item.TenBlock,
                        MaDuAn = item.MaDuAn
                    });
                }

                await _context.SaveChangesAsync();
                return ResultModel.Success($"Import thành công {newItems.Count} Block.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public List<BlockImportModel> ReadBlockFromExcel(Stream stream, string maDuAn)      
        {           
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });
            var table = dataset.Tables["Sheet1"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'Sheet1' trong file Excel.");

            var list = new List<BlockImportModel>();
            foreach (DataRow row in table.Rows)
            {
                BlockImportModel ct = new BlockImportModel();
                string? maBlock = row[0]?.ToString()?.Trim().ToUpper();

                // Cột thứ 1 (index 2)
                string? tenBlock = row[1]?.ToString()?.Trim();
                ct.MaBlock = maBlock;
                ct.TenBlock = tenBlock;
                ct.MaDuAn = maDuAn;
                if (string.IsNullOrWhiteSpace(maBlock) || string.IsNullOrWhiteSpace(tenBlock))
                {
                    continue;
                }
                list.Add(ct);
            }
            return list;
        }
        #endregion
    }
}

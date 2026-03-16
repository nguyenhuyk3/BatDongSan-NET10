using ClosedXML.Excel;
using Dapper;
using DocumentFormat.OpenXml.InkML;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.ViewTruc;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ViewTrucService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<ViewTrucService> _logger;
        private readonly IConfiguration _config;
        public ViewTrucService(IDbContextFactory<AppDbContext> factory, ILogger<ViewTrucService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        #region Hiển thị toàn bộ danh sách View Trục
        public async Task<(List<ViewTrucPagingDto> Data, int TotalCount)> GetPagingAsync(
        string? maDuAn, string? maBlock, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@MaBlock", !string.IsNullOrEmpty(maBlock) ? maBlock : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<ViewTrucPagingDto>(
                "Proc_Truc_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Them, xoa, sua View Trục
        public async Task<ResultModel> SaveViewTrucAsync(ViewTrucModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaDanhMucViewTrucs.FirstOrDefaultAsync(d => d.MaTruc.ToLower() == model.MaTruc.ToLower());
                if (duAn != null)
                    return ResultModel.Fail("Mã trục đã tồn tại.");

                var record = new DaDanhMucViewTruc
                {
                    MaTruc = model.MaTruc ?? string.Empty,
                    TenTruc = model.TenTruc ?? string.Empty,
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaBlock = model.MaBlock ?? string.Empty,
                    HeSoTruc = model.HeSoTruc,
                    ThuTuHienThi = model.ThuTuHienThi,
                    MaLoaiGoc = model.MaLoaiGoc,
                    MaLoaiView = model.MaView,
                    MaViewMatKhoi = model.MaMatKhoi,
                    MaViTri = model.MaViTri,
                    MaHuong = model.MaHuong,
                };

                await _context.DaDanhMucViewTrucs.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm view trục thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm view");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm view: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(ViewTrucModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucViewTrucs.FirstOrDefaultAsync(d => d.MaTruc == model.MaTruc);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy View trục");
                }
                entity.TenTruc = model.TenTruc ?? string.Empty;
                entity.HeSoTruc = (decimal?)model.HeSoTruc ?? 1;
                entity.ThuTuHienThi = model.ThuTuHienThi;
                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenTruc} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật View");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteViewAsync(string maTruc)
        {
            if (string.IsNullOrWhiteSpace(maTruc))
                return ResultModel.Fail("Thiếu mã trục.");

            maTruc = maTruc.Trim();

            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // B1) Chặn xoá nếu đang được dùng trong Danh Mục Sản Phẩm
                var usage = await _context.DaDanhMucSanPhams.AsNoTracking()
                    .Where(sp => sp.MaTruc == maTruc)
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteViewAsync[Truc]: Check DA_DanhMucSanPhams by MaTruc")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    // Lấy vài record demo để gợi ý vị trí sửa
                    var samples = await _context.DaDanhMucSanPhams.AsNoTracking()
                        .Where(sp => sp.MaTruc == maTruc)
                        .Select(sp => new { sp.MaSanPham, sp.TenSanPham, sp.MaDuAn, sp.MaBlock, sp.MaTang })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[{s.MaSanPham}-{s.TenSanPham} | DA:{s.MaDuAn} | Block:{s.MaBlock} | Tầng:{s.MaTang}]"));
                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{((usage!.Count > samples.Count) ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì Trục đang được sử dụng trong Danh mục Sản phẩm: {usage!.Count} dòng.{hint}");
                }

                // B2) Tìm entity & xoá
                var entity = await _context.DaDanhMucViewTrucs
                    .FirstOrDefaultAsync(d => d.MaTruc == maTruc);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy trục.");

                _context.DaDanhMucViewTrucs.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaTruc}] - {entity.TenTruc} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteViewAsync-Truc] Lỗi ràng buộc khi xoá {MaTruc}", maTruc);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ liên kết trong Danh mục Sản phẩm trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteViewAsync-Truc] Lỗi hệ thống khi xoá {MaTruc}", maTruc);
                return ResultModel.Fail("Lỗi hệ thống khi xoá trục.");
            }
        }


        public async Task<ResultModel> DeleteListAsync(List<ViewTrucPagingDto> listVT)
        {
            const int BatchSize = 1800;

            try
            {
                var ids = (listVT ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaTruc))
                    .Select(x => x!.MaTruc.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // ========== B1) KIỂM TRA RÀNG BUỘC Ở DA_DanhMucSanPhams ==========
                // map: MaTruc -> count(SanPham)
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var counts = await _context.DaDanhMucSanPhams.AsNoTracking()
                        .Where(sp => sp.MaTruc != null && chunk.Contains(sp.MaTruc!))
                        .GroupBy(sp => sp.MaTruc!)
                        .Select(g => new { MaTruc = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync[Truc]: Check DA_DanhMucSanPhams by MaTruc")
                        .ToListAsync();

                    foreach (var row in counts)
                        blockedMap[row.MaTruc] = (blockedMap.TryGetValue(row.MaTruc, out var cur) ? cur : 0) + row.Count;
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- Trục [{kv.Key}] đang được sử dụng bởi {kv.Value} sản phẩm/căn hộ."));
                    return ResultModel.Fail("Không thể xoá: tất cả Trục được chọn đều đang được sử dụng trong danh mục sản phẩm.\n" + detail);
                }

                // ========== B2) XOÁ DB TRONG TRANSACTION (chỉ xoá các ID không bị ràng buộc) ==========
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await _context.DaDanhMucViewTrucs
                        .Where(k => chunk.Contains(k.MaTruc))
                        .TagWith("DeleteListAsync[Truc]: ExecuteDelete DaDanhMucViewTrucs")
                        .ExecuteDeleteAsync();
                }

                await tx.CommitAsync();

                // ========== B3) THÔNG ĐIỆP ==========
                var totalDeleted = deletableIds.Count;
                var skipped = ids.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} Trục. " +
                    (skipped == 0 ? "" : $"{skipped} Trục không xoá (đang được sử dụng trong Danh mục Sản phẩm hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv => $"- Trục [{kv.Key}] đang được sử dụng bởi {kv.Value} sản phẩm/căn hộ.");
                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} trục khác." : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách trục");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách trục.");
            }
        }

        // Helper chunk (nếu bạn chưa có)
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin View Trục
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                //var entity = await (
                //      from lch in _context.DaDanhMucViewTrucs
                //      join duan in _context.DaDanhMucDuAns on lch.MaDuAn equals duan.MaDuAn
                //      join block in _context.DaDanhMucBlocks on lch.MaBlock equals block.MaBlock into dtDong2
                //      from block2 in dtDong2.DefaultIfEmpty()

                //      join goc in _context.DaDanhMucLoaiGocs on lch.MaLoaiGoc equals goc.MaLoaiGoc into dtGoc
                //      from goc2 in dtGoc.DefaultIfEmpty()

                //      join view in _context.DaDanhMucViews on lch.MaLoaiView equals view.MaView into dtView
                //      from view2 in dtView.DefaultIfEmpty()

                //      join mk in _context.DaDanhMucViewMatKhois on lch.MaViewMatKhoi equals mk.MaMatKhoi into dtMatKhoi
                //      from mk2 in dtMatKhoi.DefaultIfEmpty()

                //      join vt in _context.DaDanhMucViTris on lch.MaViTri equals vt.MaViTri into dtViTri
                //      from vt2 in dtViTri.DefaultIfEmpty()

                //      where lch.MaTruc == id
                //      select new ViewTrucModel
                //      {
                //          MaDuAn = lch.MaDuAn,
                //          TenDuAn = duan.TenDuAn,
                //          MaTruc = lch.MaTruc,
                //          TenTruc = lch.TenTruc,
                //          HeSoTruc = lch.HeSoTruc ?? 1,
                //          ThuTuHienThi = lch.ThuTuHienThi ?? 1,
                //          MaBlock = lch.MaBlock,
                //          TenBlock = block2.TenBlock,
                //          MaLoaiGoc = lch.MaLoaiGoc ?? string.Empty,
                //          TenLoaiGoc = goc2.TenLoaiGoc,
                //          MaViTri = lch.MaViTri ?? string.Empty,
                //          TenViTri = vt2.TenViTri,
                //          MaView = lch.MaLoaiView ?? string.Empty,
                //          TenView = view2.TenView,
                //          MaMatKhoi = lch.MaViewMatKhoi ?? string.Empty,
                //          TenMatKhoi = mk2.TenMatKhoi,
                //      }).FirstOrDefaultAsync();
                //if (entity == null)
                //{
                //    entity = new ViewTrucModel();
                //}

                var query = await (
                    from truc in _context.DaDanhMucViewTrucs.AsNoTracking()

                        // Dự án
                    join duan in _context.DaDanhMucDuAns on truc.MaDuAn equals duan.MaDuAn

                    // Block (LEFT JOIN theo cả MaDuAn + MaBlock)
                    join b in _context.DaDanhMucBlocks.AsNoTracking()
                        on new { truc.MaDuAn, truc.MaBlock }
                        equals new { b.MaDuAn, b.MaBlock } into jb
                    from block in jb.DefaultIfEmpty()

                        // Loại góc (LEFT JOIN)
                    join lg in _context.DaDanhMucLoaiGocs.AsNoTracking()
                        on new { truc.MaDuAn, MaLoaiGoc = truc.MaLoaiGoc }
                        equals new { lg.MaDuAn, lg.MaLoaiGoc } into jlg
                    from loaiGoc in jlg.DefaultIfEmpty()

                        // View (LEFT JOIN)
                    join v in _context.DaDanhMucViews.AsNoTracking()
                        on new { truc.MaDuAn, MaView = truc.MaLoaiView }
                        equals new { v.MaDuAn, MaView = v.MaView } into jv
                    from view in jv.DefaultIfEmpty()

                        // View mặt khối (LEFT JOIN)
                    join mk in _context.DaDanhMucViewMatKhois.AsNoTracking()
                        on new { truc.MaDuAn, MaMK = truc.MaViewMatKhoi }
                        equals new { mk.MaDuAn, MaMK = mk.MaMatKhoi } into jvmk
                    from mk in jvmk.DefaultIfEmpty()

                        // Vị trí (LEFT JOIN)
                    join vt in _context.DaDanhMucViTris.AsNoTracking()
                        on new { truc.MaDuAn, MaViTri = truc.MaViTri }
                        equals new { vt.MaDuAn, MaViTri = vt.MaViTri } into jvt
                    from vt in jvt.DefaultIfEmpty()

                        // Hướng (LEFT JOIN)
                    join h in _context.DaDanhMucHuongs.AsNoTracking()
                        on new { truc.MaDuAn, MaHuong = truc.MaHuong }
                        equals new { h.MaDuAn, MaHuong = h.MaHuong } into jh
                    from h in jh.DefaultIfEmpty()

                    where truc.MaTruc == id

                    select new ViewTrucModel
                    {
                        MaDuAn = truc.MaDuAn,
                        TenDuAn = duan.TenDuAn,

                        MaTruc = truc.MaTruc,
                        TenTruc = truc.TenTruc,

                        MaBlock = truc.MaBlock,
                        TenBlock = block != null ? block.TenBlock : null,

                        HeSoTruc = truc.HeSoTruc ?? 1,
                        ThuTuHienThi = truc.ThuTuHienThi ?? 1,
                        IsNew = false,

                        // 🔹 Thêm các trường yêu cầu
                        MaLoaiGoc = truc.MaLoaiGoc,
                        TenLoaiGoc = loaiGoc != null ? loaiGoc.TenLoaiGoc : null,

                        MaView = truc.MaLoaiView,
                        TenView = view != null ? view.TenView : null,

                        MaMatKhoi = truc.MaViewMatKhoi,
                        TenMatKhoi = mk != null ? mk.TenMatKhoi : null,

                        MaViTri = truc.MaViTri,
                        TenViTri = vt != null ? vt.TenViTri : null,

                        MaHuong = truc.MaHuong,
                        TenHuong = h != null ? h.TenHuong : null,
                    }).FirstOrDefaultAsync();

                if (query == null)
                {
                    query = new ViewTrucModel();
                }
                return ResultModel.SuccessWithData(query, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một View trục trong dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<List<DaDanhMucBlock>> GetByBlockTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucBlock>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucBlocks.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucBlock>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách block theo dự án");
            }
            return entity;
        }

        public async Task<List<DaDanhMucLoaiGoc>> GetByLoaiGocTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucLoaiGoc>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucLoaiGocs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucLoaiGoc>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại góc theo dự án");
            }
            return entity;
        }

        public async Task<List<DaDanhMucView>> GetByViewTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucView>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucViews.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucView>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách view theo dự án");
            }
            return entity;
        }

        public async Task<List<DaDanhMucViewMatKhoi>> GetByMatKhoiTheoDuAnAsync(string maDuAn, string maBlock)
        {
            var entity = new List<DaDanhMucViewMatKhoi>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucViewMatKhois.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucViewMatKhoi>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách mặt khối theo dự án và block");
            }
            return entity;
        }
        public async Task<List<DaDanhMucViTri>> GetByViTriTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucViTri>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucViTris.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucViTri>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách vị trí theo dự án");
            }
            return entity;
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

            var blockSheetLoaiGoc = workbook.Worksheet("LoaiGoc");
            var listLoaiGoc = await _context.DaDanhMucLoaiGocs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listLoaiGoc)
            {
                blockSheetLoaiGoc.Cell(row, 1).Value = item.MaLoaiGoc;
                blockSheetLoaiGoc.Cell(row, 2).Value = item.TenLoaiGoc;
                row++;
            }

            var blockSheetVCH = workbook.Worksheet("ViewCanHo");
            var listVCH = await _context.DaDanhMucViews.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listVCH)
            {
                blockSheetVCH.Cell(row, 1).Value = item.MaView;
                blockSheetVCH.Cell(row, 2).Value = item.TenView;
                row++;
            }

            var blockSheetVT = workbook.Worksheet("ViTri");
            var listVT = await _context.DaDanhMucViTris.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listVT)
            {
                blockSheetVT.Cell(row, 1).Value = item.MaViTri;
                blockSheetVT.Cell(row, 2).Value = item.TenViTri;
                row++;
            }

            var blockSheetMCKH = workbook.Worksheet("MatKhoiCanHo");
            var listMKCH = await _context.DaDanhMucViewMatKhois.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listMKCH)
            {
                blockSheetMCKH.Cell(row, 1).Value = item.MaMatKhoi;
                blockSheetMCKH.Cell(row, 2).Value = item.TenMatKhoi;
                row++;
            }

            var blockSheetHuong = workbook.Worksheet("Huong");
            var listHuong = await _context.DaDanhMucHuongs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listHuong)
            {
                blockSheetHuong.Cell(row, 1).Value = item.MaHuong;
                blockSheetHuong.Cell(row, 2).Value = item.TenHuong;
                row++;
            }

            // Set lại sheet "SanPham" là active
            workbook.Worksheet("ViewTrucCanHo").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }

        public async Task<ResultModel> ImportFromExcelAsync(IBrowserFile file, string maDuAn, CancellationToken ct = default)
        {
            try
            {
                if (file is null) return ResultModel.Fail("File trống.");
                if (string.IsNullOrWhiteSpace(maDuAn)) return ResultModel.Fail("Thiếu mã dự án.");

                await using var _context = _factory.CreateDbContext();

                await using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var ms = new MemoryStream();
                await inputStream.CopyToAsync(ms, ct);
                ms.Position = 0;

                var items = ReadViewTrucFromExcel(ms, maDuAn);

                // --- B1: Chuẩn hoá chuỗi ---
                items = items.Select(x =>
                {
                    x.MaBlock = x.MaBlock?.Trim().ToUpperInvariant();
                    x.MaTruc = x.MaTruc?.Trim();
                    x.TenTruc = x.TenTruc?.Trim();
                    x.MaLoaiGoc = string.IsNullOrWhiteSpace(x.MaLoaiGoc) ? null : x.MaLoaiGoc.Trim();
                    x.MaView = string.IsNullOrWhiteSpace(x.MaView) ? null : x.MaView.Trim();
                    x.MaViTri = string.IsNullOrWhiteSpace(x.MaViTri) ? null : x.MaViTri.Trim();
                    x.MaMatKhoi = x.MaMatKhoi?.Trim();
                    x.MaHuong = string.IsNullOrWhiteSpace(x.MaHuong) ? null : x.MaHuong.Trim();
                    x.HeSoTruc ??= 1;
                    x.STTTang ??= 1;
                    return x;
                }).ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu.");

                // --- B2: Kiểm tra bắt buộc (MaBlock, MaTruc, TenTruc, MaMatKhoi) ---
                var requiredErrors = new List<string>();
                foreach (var it in items)
                {
                    var miss = new List<string>();
                    if (string.IsNullOrWhiteSpace(it.MaBlock)) miss.Add("MaBlock");
                    if (string.IsNullOrWhiteSpace(it.MaTruc)) miss.Add("MaTruc");
                    if (string.IsNullOrWhiteSpace(it.TenTruc)) miss.Add("TenTruc");
                    if (string.IsNullOrWhiteSpace(it.MaMatKhoi)) miss.Add("MaMatKhoi");

                    if (miss.Count > 0)
                        requiredErrors.Add($"- Dòng {it.RowIndex}: thiếu {string.Join(", ", miss)}");
                }
                if (requiredErrors.Count > 0)
                {
                    var preview = string.Join(Environment.NewLine, requiredErrors.Take(15));
                    return ResultModel.Fail("Thiếu dữ liệu bắt buộc:\n" + preview + (requiredErrors.Count > 15 ? "\n... nữa." : ""));
                }

                // --- B3: Trùng mã trong file (MaTruc) ---
                var duplicateInFile = items
                    .GroupBy(x => x.MaTruc!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => $"- {g.Key}: dòng {string.Join(", ", g.Select(i => i.RowIndex))}")
                    .ToList();
                if (duplicateInFile.Count > 0)
                    return ResultModel.Fail("Trùng MaTruc ngay trong file:\n" + string.Join("\n", duplicateInFile));

                // --- B4: Tồn tại Block theo dự án (bắt buộc) ---
                var existingBlocks = new HashSet<string>(
                    await _context.DaDanhMucBlocks.AsNoTracking()
                        .Where(b => b.MaDuAn == maDuAn && b.MaBlock != null)
                        .Select(b => b.MaBlock!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);
                var invalidBlockRows = items.Where(x => !existingBlocks.Contains(x.MaBlock!)).ToList();
                if (invalidBlockRows.Count > 0)
                {
                    var p = string.Join(", ", invalidBlockRows.Select(x => $"{x.MaBlock}(dòng {x.RowIndex})").Take(15));
                    return ResultModel.Fail($"MaBlock không tồn tại trong dự án {maDuAn}: {p}" + (invalidBlockRows.Count > 15 ? "..." : ""));
                }

                // --- B5: Tồn tại Mặt Khối (bắt buộc) ---
                var existingMatKhoi = new HashSet<string>(
                    await _context.DaDanhMucViewMatKhois.AsNoTracking()
                        .Where(m => m.MaMatKhoi != null)
                        .Select(m => m.MaMatKhoi!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);
                var invalidMatKhoiRows = items.Where(x => !existingMatKhoi.Contains(x.MaMatKhoi!)).ToList();
                if (invalidMatKhoiRows.Count > 0)
                {
                    var p = string.Join(", ", invalidMatKhoiRows.Select(x => $"{x.MaMatKhoi}(dòng {x.RowIndex})").Take(15));
                    return ResultModel.Fail($"MaMatKhoi không tồn tại: {p}" + (invalidMatKhoiRows.Count > 15 ? "..." : ""));
                }

                // --- B6: (Optional) Kiểm tra tồn tại nếu có nhập: MaLoaiGoc / MaView / MaViTri ---
                var failOptional = new List<string>();

                var inputLoaiGoc = items.Select(x => x.MaLoaiGoc).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputLoaiGoc.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucLoaiGocs.AsNoTracking().Where(x => x.MaLoaiGoc != null && inputLoaiGoc.Contains(x.MaLoaiGoc!)).Select(x => x.MaLoaiGoc!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputLoaiGoc.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MaLoaiGoc không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }


                var inputView = items.Select(x => x.MaView).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputView.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucViews.AsNoTracking().Where(x => x.MaView != null && inputView.Contains(x.MaView!)).Select(x => x.MaView!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputView.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MaView không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }

                var inputViTri = items.Select(x => x.MaViTri).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputViTri.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucViTris.AsNoTracking().Where(x => x.MaViTri != null && inputViTri.Contains(x.MaViTri!)).Select(x => x.MaViTri!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputViTri.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MaViTri không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }

                var inputHuong = items.Select(x => x.MaHuong).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputHuong.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucHuongs.AsNoTracking().Where(x => x.MaHuong != null && inputView.Contains(x.MaHuong!)).Select(x => x.MaHuong!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputHuong.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MaHuong không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }

                if (failOptional.Count > 0)
                    return ResultModel.Fail(string.Join("\n", failOptional));

                // --- B7: Trùng MaTruc trong DB (system-wide) ---
                var existingTrucSet = new HashSet<string>(
                    await _context.DaDanhMucViewTrucs.AsNoTracking().Where(t => t.MaTruc != null).Select(t => t.MaTruc!).ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                var duplicatesInDbSystem = items.Where(x => existingTrucSet.Contains(x.MaTruc!)).ToList();
                var newItems = items.Where(x => !existingTrucSet.Contains(x.MaTruc!)).ToList();

                if (newItems.Count == 0)
                {
                    var p = string.Join(", ", duplicatesInDbSystem.Select(x => $"{x.MaTruc}(dòng {x.RowIndex})").Distinct(StringComparer.OrdinalIgnoreCase).Take(15));
                    return ResultModel.Fail("Tất cả MaTruc trong file đã tồn tại trong hệ thống: " + p + (duplicatesInDbSystem.Count > 15 ? "..." : ""));
                }

                // --- B8: Insert ---
                var entities = newItems.Select(item => new DaDanhMucViewTruc
                {
                    MaBlock = item.MaBlock!,
                    MaTruc = item.MaTruc!,
                    TenTruc = item.TenTruc!,
                    MaLoaiGoc = item.MaLoaiGoc,
                    MaLoaiView = item.MaView,
                    MaViTri = item.MaViTri,
                    MaViewMatKhoi = item.MaMatKhoi!, // bắt buộc
                    HeSoTruc = item.HeSoTruc ?? 1,
                    ThuTuHienThi = item.STTTang ?? 1,
                    MaHuong = item.MaHuong,
                    MaDuAn = maDuAn
                }).ToList();

                await _context.DaDanhMucViewTrucs.AddRangeAsync(entities, ct);
                var affected = await _context.SaveChangesAsync(ct);

                // --- B9: Message ---
                var msg = $"Import thành công {entities.Count} Trục (đã ghi {affected} bản ghi).";
                if (duplicatesInDbSystem.Count > 0)
                {
                    var preview = string.Join(", ", duplicatesInDbSystem.Select(x => x.MaTruc).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                    msg += $" Bỏ qua {duplicatesInDbSystem.Count} dòng do MaTruc đã tồn tại (ví dụ: {preview}{(duplicatesInDbSystem.Count > 10 ? ", ..." : ")")}";
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi import Excel");
                return ResultModel.Fail($"Lỗi import: {ex.Message}");
            }
        }


        public List<ViewTrucImportModel> ReadViewTrucFromExcel(Stream stream, string maDuAn)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataSet.Tables["ViewTrucCanHo"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'ViewTrucCanHo' trong file Excel.");

            var list = new List<ViewTrucImportModel>();
            int row = 2; // header=1

            foreach (DataRow r in table.Rows)
            {
                string? maBlock = r[0]?.ToString()?.Trim();
                string? maTruc = r[1]?.ToString()?.Trim();
                string? tenTruc = r[2]?.ToString()?.Trim();
                string? maMatKhoi = r[3]?.ToString()?.Trim();
                string? maHuong = r[4]?.ToString()?.Trim();
                string? maView = r[5]?.ToString()?.Trim();
                string? maLoaiGoc = r[6]?.ToString()?.Trim();
                string? maViTri = r[7]?.ToString()?.Trim();

                decimal? heSoTruc = null;
                if (decimal.TryParse(r[8]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hst))
                    heSoTruc = hst;

                int? sttTruc = null;
                if (int.TryParse(r[9]?.ToString()?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stt))
                    sttTruc = stt;

                list.Add(new ViewTrucImportModel
                {
                    MaBlock = maBlock,
                    MaTruc = maTruc,
                    TenTruc = tenTruc,
                    MaLoaiGoc = maLoaiGoc,
                    MaView = maView,
                    MaViTri = maViTri ?? string.Empty,
                    MaMatKhoi = maMatKhoi ?? string.Empty,
                    HeSoTruc = heSoTruc ?? 1,
                    MaHuong = maHuong ?? string.Empty,
                    STTTang = sttTruc ?? 1,
                    MaDuAn = maDuAn,
                    RowIndex = row
                });

                row++;
            }

            return list;
        }

        #endregion
    }
}

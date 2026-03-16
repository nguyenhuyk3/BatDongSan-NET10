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
using VTTGROUP.Domain.Model.SanPham;
using VTTGROUP.Domain.Model.ViewTruc;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class SanPhamService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<SanPhamService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        public SanPhamService(IDbContextFactory<AppDbContext> factory, ILogger<SanPhamService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
        }

        #region Hiển thị danh sách sản phẩm
        public async Task<(List<SanPhamPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? maDuAn, string? maBlock, string? maTang, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@MaBlock", !string.IsNullOrEmpty(maBlock) ? maBlock : null);
            param.Add("@MaTang", !string.IsNullOrEmpty(maTang) ? maTang : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<SanPhamPagingDto>(
                "Proc_SanPham_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Them, xoa, sua sản phẩm
        public async Task<ResultModel> SaveViewSanPhamAsync(SanPhamModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaDanhMucSanPhams.FirstOrDefaultAsync(d => d.MaSanPham.ToLower() == model.MaSanPham.ToLower());
                if (duAn != null)
                    return ResultModel.Fail("Mã sản phẩm đã tồn tại.");

                var record = new DaDanhMucSanPham
                {
                    MaSanPham = model.MaSanPham ?? string.Empty,
                    TenSanPham = model.TenSanPham ?? string.Empty,
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    MaBlock = model.MaBlock ?? string.Empty,
                    MaTang = model.MaTang ?? string.Empty,
                    MaTruc = model.MaTruc ?? string.Empty,
                    MaLoaiDienTich = model.MaLoaiDT,
                    MaLoaiLayout = model.MaLoaiThietKe,
                    MaLoaiCan = model.MaLoaiCan ?? string.Empty,
                    DienTichTimTuong = (decimal?)model.DienTichTimTuong ?? 0,
                    DienTichThongThuy = (decimal?)model.DienTichThongThuy ?? 0,
                    DienTichSanVuon = (decimal?)model.DienTichSanVuon ?? 0,
                    HeSoCanHo = (decimal?)model.HeSoCanHo ?? 0,
                    LoaiSanPham = model.MaLoaiSP
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.HienTrangKd = string.Empty;
                record.NgayLap = DateTime.Now;
                await _context.DaDanhMucSanPhams.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm sản phẩm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm sản phẩm");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm sản phẩm: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(SanPhamModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucSanPhams.FirstOrDefaultAsync(d => d.MaSanPham == model.MaSanPham);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy sản phẩm");
                }
                entity.TenSanPham = model.TenSanPham ?? string.Empty;
                entity.DienTichTimTuong = (decimal?)model.DienTichTimTuong ?? 0;
                entity.DienTichThongThuy = (decimal?)model.DienTichThongThuy ?? 0;
                entity.DienTichSanVuon = (decimal?)model.DienTichSanVuon ?? 0;
                entity.HeSoCanHo = (decimal?)model.HeSoCanHo ?? 0;
                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenSanPham} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật sản phẩm");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteViewAsync(string maSanPham)
        {
            if (string.IsNullOrWhiteSpace(maSanPham))
                return ResultModel.Fail("Thiếu mã sản phẩm.");

            maSanPham = maSanPham.Trim();

            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc ở Kế hoạch/Đợt mở bán (FK: MaCanHo -> DA_DanhMucSanPhams.MaSanPham)
                var usage = await _context.BhKeHoachBanHangDotMoBanCanHos.AsNoTracking()
                    .Where(x => x.MaCanHo == maSanPham)
                    .GroupBy(_ => 1)
                    .Select(g => new { Count = g.Count() })
                    .TagWith("DeleteSanPham: Check BH_KeHoachBanHang_DotMoBanCanHo by MaCanHo")
                    .FirstOrDefaultAsync();

                if ((usage?.Count ?? 0) > 0)
                {
                    // Lấy vài dòng ví dụ cho dễ xử lý ngược
                    var samples = await _context.BhKeHoachBanHangDotMoBanCanHos.AsNoTracking()
                        .Where(x => x.MaCanHo == maSanPham)
                        .Select(x => new { x.MaPhieuKh, x.MaDotMoBan })
                        .Take(5)
                        .ToListAsync();

                    var demo = string.Join(", ",
                        samples.Select(s => $"[KHBH:{s.MaPhieuKh} | Đợt:{s.MaDotMoBan}]"));
                    var hint = samples.Count > 0 ? $" Ví dụ: {demo}{((usage!.Count > samples.Count) ? ", ..." : "")}" : "";

                    return ResultModel.Fail($"Không thể xoá vì sản phẩm đang được dùng trong Kế hoạch/Đợt mở bán: {usage!.Count} dòng.{hint}");
                }

                // B2) Tìm entity & xoá
                var entity = await _context.DaDanhMucSanPhams
                    .FirstOrDefaultAsync(d => d.MaSanPham == maSanPham);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy sản phẩm.");

                _context.DaDanhMucSanPhams.Remove(entity);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Đã xoá [{entity.MaSanPham}] - {entity.TenSanPham} thành công.");
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "[DeleteSanPham] Lỗi ràng buộc khi xoá {MaSanPham}", maSanPham);
                return ResultModel.Fail("Không thể xoá vì đang bị ràng buộc dữ liệu. Vui lòng gỡ khỏi Kế hoạch/Đợt mở bán trước.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteSanPham] Lỗi hệ thống khi xoá {MaSanPham}", maSanPham);
                return ResultModel.Fail("Lỗi hệ thống khi xoá sản phẩm.");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<SanPhamPagingDto> listPDC)
        {
            const int BatchSize = 1800;

            try
            {
                var ids = (listPDC ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaSanPham))
                    .Select(x => x!.MaSanPham.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // ===== B1) Kiểm tra ràng buộc tại BH_KeHoachBanHang_DotMoBanCanHo (MaCanHo -> MaSanPham) =====
                var blockedMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    var counts = await _context.BhKeHoachBanHangDotMoBanCanHos.AsNoTracking()
                        .Where(x => x.MaCanHo != null && chunk.Contains(x.MaCanHo!))
                        .GroupBy(x => x.MaCanHo!)
                        .Select(g => new { MaSanPham = g.Key, Count = g.Count() })
                        .TagWith("DeleteListAsync[SanPham]: Check BH_KeHoachBanHang_DotMoBanCanHo by MaCanHo")
                        .ToListAsync();

                    foreach (var row in counts)
                        blockedMap[row.MaSanPham] =
                            (blockedMap.TryGetValue(row.MaSanPham, out var cur) ? cur : 0) + row.Count;
                }

                var blockedIds = blockedMap.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var detail = string.Join(Environment.NewLine,
                        blockedMap.OrderBy(kv => kv.Key)
                                  .Select(kv => $"- Sản phẩm [{kv.Key}] đang được dùng trong kế hoạch/đợt mở bán: {kv.Value} dòng."));
                    return ResultModel.Fail("Không thể xoá: tất cả sản phẩm được chọn đều đang được sử dụng.\n" + detail);
                }

                // ===== B2) Xoá DB trong transaction (chỉ xoá những mã không bị ràng buộc) =====
                await using var tx = await _context.Database.BeginTransactionAsync();

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await _context.DaDanhMucSanPhams
                        .Where(k => chunk.Contains(k.MaSanPham))
                        .TagWith("DeleteListAsync[SanPham]: ExecuteDelete DaDanhMucSanPhams")
                        .ExecuteDeleteAsync();
                }

                await tx.CommitAsync();

                // ===== B3) Thông điệp =====
                var totalDeleted = deletableIds.Count;
                var skipped = ids.Count - totalDeleted;

                var baseMsg =
                    $"Đã xoá {totalDeleted}/{ids.Count} sản phẩm. " +
                    (skipped == 0 ? "" : $"{skipped} sản phẩm không xoá (đang dùng trong kế hoạch/đợt mở bán hoặc không tồn tại). ");

                string blockedDetail = string.Empty;
                if (blockedIds.Count > 0)
                {
                    var top = blockedMap.OrderBy(kv => kv.Key).Take(10)
                        .Select(kv => $"- [{kv.Key}] đang dùng tại {kv.Value} dòng KHBH/Đợt mở bán.");
                    blockedDetail = $"\nChi tiết ràng buộc:\n{string.Join(Environment.NewLine, top)}" +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} sản phẩm khác." : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xoá danh sách sản phẩm");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách sản phẩm.");
            }
        }

        // Helper: chia batch an toàn dưới giới hạn 2100 tham số
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        #endregion

        #region Thông tin sản phẩm
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from sp in _context.DaDanhMucSanPhams
                      join duan in _context.DaDanhMucDuAns on sp.MaDuAn equals duan.MaDuAn

                      join block in _context.DaDanhMucBlocks on sp.MaBlock equals block.MaBlock into dtBlock
                      from block2 in dtBlock.DefaultIfEmpty()

                      join tang in _context.DaDanhMucTangs on sp.MaTang equals tang.MaTang into dtTang
                      from tang2 in dtTang.DefaultIfEmpty()

                      join truc in _context.DaDanhMucViewTrucs on sp.MaTruc equals truc.MaTruc into dtTruc
                      from truc2 in dtTruc.DefaultIfEmpty()

                      join ldt in _context.DaLoaiDienTiches on sp.MaLoaiDienTich equals ldt.MaLoaiDt into dtLoaiDT
                      from ldt2 in dtLoaiDT.DefaultIfEmpty()

                      join ltk in _context.DaDanhMucLoaiThietKes on sp.MaLoaiLayout equals ltk.MaLoaiThietKe into dtLTK
                      from ltk2 in dtLTK.DefaultIfEmpty()

                      join lch in _context.DaDanhMucLoaiCanHos on sp.MaLoaiCan equals lch.MaLoaiCanHo into dtLoaiCH
                      from lch2 in dtLoaiCH.DefaultIfEmpty()

                      join lsp in _context.DaDanhMucLoaiSanPhams on sp.LoaiSanPham equals lsp.MaLoaiSanPham into dtLSP
                      from lsp2 in dtLSP.DefaultIfEmpty()

                      where sp.MaSanPham == id
                      select new SanPhamModel
                      {
                          NgayLap = sp.NgayLap ?? DateTime.Now,
                          MaSanPham = sp.MaSanPham,
                          TenSanPham = sp.TenSanPham,
                          MaDuAn = sp.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaBlock = sp.MaBlock,
                          TenBlock = block2.TenBlock,
                          MaTang = sp.MaTang,
                          TenTang = tang2.TenTang,
                          MaLoaiCan = sp.MaLoaiCan,
                          TenLoaiCan = lch2.TenLoaiCanHo,
                          MaTruc = sp.MaTruc,
                          TenTruc = truc2.TenTruc,
                          MaLoaiDT = sp.MaLoaiDienTich ?? string.Empty,
                          TenLoaiDT = ldt2.TenLoaiDt,
                          MaLoaiThietKe = sp.MaLoaiLayout ?? string.Empty,
                          TenLoaiThietKe = ltk2.TenLoaiThietKe ?? string.Empty,
                          DienTichTimTuong = sp.DienTichTimTuong ?? 0,
                          DienTichThongThuy = sp.DienTichThongThuy ?? 0,
                          DienTichSanVuon = sp.DienTichSanVuon ?? 0,
                          MaNhanVien = sp.NguoiLap,
                          MaLoaiSP = sp.LoaiSanPham,
                          TenLoaiSP = lsp2.TenLoaiSanPham,
                          HeSoCanHo = sp.HeSoCanHo ?? 1
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new SanPhamModel();
                    entity.NgayLap = DateTime.Now;
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.HeSoDienTich = 1;
                    entity.HeSoTang = 1;
                    entity.HeSoTruc = 1;
                    entity.HeSoViTri = 1;
                    entity.HeSoView = 1;
                    entity.HeSoGoc = 1;
                    entity.HeSoMatKhoi = 1;
                    //entity.HeSoCanHo = (entity.HeSoTang ?? 1) *
                    //                (entity.HeSoDienTich ?? 1) *
                    //                (entity.HeSoGoc ?? 1) *
                    //                (entity.HeSoViTri ?? 1) *
                    //                (entity.HeSoView ?? 1) *
                    //                (entity.HeSoTruc ?? 1) *
                    //                (entity.HeSoMatKhoi ?? 1);
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var thongTinTruc = await GetThongTinByMaTrucAsync(entity.MaTruc);
                    entity.TenTruc = thongTinTruc.TenTruc;
                    entity.HeSoTruc = (decimal?)thongTinTruc.HeSoTruc ?? 1;

                    entity.MaViTri = thongTinTruc.MaViTri;
                    entity.TenViTri = thongTinTruc.TenViTri;
                    entity.HeSoViTri = thongTinTruc.HeSoViTri ?? 1;

                    entity.MaView = thongTinTruc.MaView;
                    entity.TenView = thongTinTruc.TenView;
                    entity.HeSoView = thongTinTruc.HeSoView ?? 1;

                    entity.MaLoaiGoc = thongTinTruc.MaLoaiGoc;
                    entity.TenLoaiGoc = thongTinTruc.TenLoaiGoc;
                    entity.HeSoGoc = thongTinTruc.HeSoGoc ?? 1;

                    entity.MaMatKhoi = thongTinTruc.MaMatKhoi;
                    entity.TenMatKhoi = thongTinTruc.TenMatKhoi;
                    entity.HeSoMatKhoi = thongTinTruc.HeSoMatKhoi ?? 1;
                    //entity.HeSoCanHo = (entity.HeSoTang ?? 1) *
                    //                (entity.HeSoDienTich ?? 1) *
                    //                (entity.HeSoGoc ?? 1) *
                    //                (entity.HeSoViTri ?? 1) *
                    //                (entity.HeSoView ?? 1) *
                    //                (entity.HeSoTruc ?? 1) *
                    //                (entity.HeSoMatKhoi ?? 1);
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một sản phẩm trong dự án");
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

        public async Task<List<DaDanhMucTang>> GetByTangTheoBlockAsync(string maBlock, string maDuAn)
        {
            var entity = new List<DaDanhMucTang>();
            try
            {
                using var _context = _factory.CreateDbContext();
                if (string.IsNullOrWhiteSpace(maBlock))
                    return entity;

                var blockList = maBlock
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

                if (blockList == null || blockList.Count == 0)
                    return entity;

                entity = await _context.DaDanhMucTangs
                .Where(d => blockList.Contains(d.MaBlock) && d.MaDuAn == maDuAn)
                .ToListAsync();

                if (entity == null)
                {
                    entity = new List<DaDanhMucTang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách tầng theo dự án");
            }
            return entity;
        }

        public async Task<decimal?> GetHeSoTangAsync(string maTang)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucTangs.FirstOrDefaultAsync(d => d.MaTang == maTang);
                if (entity == null)
                {
                    return 1;
                }
                return entity.HeSoTang ?? 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hệ số tầng");
            }
            return 1;
        }

        public async Task<List<DaDanhMucLoaiCanHo>> GetByLoaiCanHoTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucLoaiCanHo>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucLoaiCanHos.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucLoaiCanHo>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại căn hộ theo dự án");
            }
            return entity;
        }

        public async Task<decimal?> GetHeSoLoaiCanAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucLoaiCanHos.FirstOrDefaultAsync(d => d.MaLoaiCanHo == maPhieu);
                if (entity == null)
                {
                    return 1;
                }
                return entity.HeSoDienTich ?? 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hệ số tầng");
            }
            return 1;
        }

        public async Task<List<DaDanhMucViewTruc>> GetByTrucTheoDuAnAsync(string maDuAn, string maBlock)
        {
            var entity = new List<DaDanhMucViewTruc>();
            try
            {
                using var _context = _factory.CreateDbContext();
                if (string.IsNullOrWhiteSpace(maBlock))
                    return entity;

                var blockList = maBlock
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

                if (blockList == null || blockList.Count == 0)
                    return entity;

                entity = await _context.DaDanhMucViewTrucs
                .Where(d => blockList.Contains(d.MaBlock) && d.MaDuAn == maDuAn)
                .ToListAsync();
                //entity = await _context.DaDanhMucViewTrucs.Where(d => d.MaDuAn == maDuAn && d.MaBlock == maBlock).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucViewTruc>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách trục theo dự án và block");
            }
            return entity;
        }

        public async Task<ViewTrucModel?> GetThongTinByMaTrucAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (from truc in _context.DaDanhMucViewTrucs

                                    join vt in _context.DaDanhMucViTris on truc.MaViTri equals vt.MaViTri into vtGroup
                                    from vt in vtGroup.DefaultIfEmpty()

                                    join view in _context.DaDanhMucViews on truc.MaLoaiView equals view.MaView into viewGroup
                                    from view in viewGroup.DefaultIfEmpty()

                                    join goc in _context.DaDanhMucLoaiGocs on truc.MaLoaiGoc equals goc.MaLoaiGoc into gocGroup
                                    from goc in gocGroup.DefaultIfEmpty()

                                    join mk in _context.DaDanhMucViewMatKhois on truc.MaViewMatKhoi equals mk.MaMatKhoi into mkGroup
                                    from mk in mkGroup.DefaultIfEmpty()
                                    where truc.MaTruc == maPhieu
                                    select new ViewTrucModel
                                    {
                                        MaTruc = truc.MaTruc,
                                        TenTruc = truc.TenTruc,
                                        HeSoTruc = truc.HeSoTruc ?? 1,

                                        MaViTri = truc.MaViTri,
                                        TenViTri = vt.TenViTri,
                                        HeSoViTri = vt.HeSoViTri ?? 1,

                                        MaView = truc.MaLoaiView,
                                        TenView = view.TenView,
                                        HeSoView = view.HeSoView ?? 1,

                                        MaLoaiGoc = truc.MaLoaiGoc,
                                        TenLoaiGoc = goc.TenLoaiGoc,
                                        HeSoGoc = goc.HeSoGoc ?? 1,

                                        MaMatKhoi = truc.MaViewMatKhoi,
                                        TenMatKhoi = mk.TenMatKhoi,
                                        HeSoMatKhoi = mk.HeSoMatKhoi ?? 1
                                    }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    return new ViewTrucModel();
                }
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin trục");
            }
            return new ViewTrucModel();
        }

        public async Task<List<DaDanhMucLoaiGoc>> GetByLoaiGocTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucLoaiGoc>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucLoaiGocs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                //entity = await _context.DaDanhMucLoaiGocs.ToListAsync();
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

        public async Task<List<DaDanhMucViTri>> GetByViTriTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucViTri>();
            try
            {
                using var _context = _factory.CreateDbContext();
                //entity = await _context.DaDanhMucBlocks.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                entity = await _context.DaDanhMucViTris.ToListAsync();
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

        public async Task<List<DaDanhMucView>> GetByViewTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucView>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucViews.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                //entity = await _context.DaDanhMucViews.ToListAsync();
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

        public async Task<List<DaDanhMucViewTruc>> GetByViewTrucTheoDuAnAsync(string maDuAn, string maBlock)
        {
            var entity = new List<DaDanhMucViewTruc>();
            try
            {
                using var _context = _factory.CreateDbContext();
                //entity = await _context.DaDanhMucBlocks.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                entity = await _context.DaDanhMucViewTrucs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucViewTruc>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách view trục theo dự án và block");
            }
            return entity;
        }

        public async Task<List<DaDanhMucViewMatKhoi>> GetByMatKhoiTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucViewMatKhoi>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucViewMatKhois.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                //entity = await _context.DaDanhMucViewMatKhois.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucViewMatKhoi>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách mặt khối theo dự án");
            }
            return entity;
        }

        public async Task<List<DaDanhMucDotMoBan>> GetListDotMoBanAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucDotMoBan>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucDotMoBans.Where(d => d.MaDuAn == maDuAn && d.NgayKetThuc >= DateTime.Now).OrderBy(d => d.ThuTuHienThi).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucDotMoBan>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách đợt theo dự án");
            }
            return entity;
        }

        public async Task<List<DaLoaiDienTich>> GetByLoaiDTTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaLoaiDienTich>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaLoaiDienTiches.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaLoaiDienTich>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại diện tích theo dự án");
            }
            return entity;
        }

        public async Task<List<DaDanhMucLoaiThietKe>> GetByLoaiThietKeTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucLoaiThietKe>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucLoaiThietKes.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucLoaiThietKe>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại thiết kế theo dự án");
            }
            return entity;
        }
        #endregion

        #region Load loại sản phẩm
        public async Task<List<DaDanhMucLoaiSanPham>> GetByLoaiSanPhamAsync()
        {
            var entity = new List<DaDanhMucLoaiSanPham>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucLoaiSanPhams.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucLoaiSanPham>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại sản phẩm");
            }
            return entity;
        }
        #endregion

        #region Import, download file mẫu    
        public async Task<ResultModel> ImportFromExcelAsync(
      IBrowserFile file,
      string maDuAn,
      string maLoaiSanPham)
        {
            try
            {
                if (file == null || file.Size == 0)
                    return ResultModel.Fail("File import trống.");

                var ext = Path.GetExtension(file.Name);
                if (!ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    return ResultModel.Fail("File import phải là .xlsx.");

                using var context = _factory.CreateDbContext();

                using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await inputStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // 1) Đọc + validate trên file
                var (items, fileErrors) = ReadSanPhamFromExcel(memoryStream);

                if (fileErrors.Any())
                {
                    var msg = "Lỗi dữ liệu trong file:\n" + string.Join("\n", fileErrors);
                    return ResultModel.Fail(msg);
                }

                if (items.Count == 0)
                    return ResultModel.Fail("Không có dòng dữ liệu hợp lệ nào trong file.");

                // 2) Chuẩn bị list để kiểm tra DB
                var maSanPhams = items
                    .Select(x => x.MaSanPham!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var maBlocks = items
                    .Select(x => x.MaBlock!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var maTangs = items
                    .Select(x => x.MaTang!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var maTrucs = items
                    .Select(x => x.MaTruc!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var maLoaiCans = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaLoaiCan))
                    .Select(x => x.MaLoaiCan!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var maLoaiDienTichs = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaLoaiDienTich))
                    .Select(x => x.MaLoaiDienTich!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var maLayouts = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaLoaiLayout))
                    .Select(x => x.MaLoaiLayout!)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var errors = new List<string>();

                // 2.1) Trùng mã sản phẩm trong DB
                var existedMaSp = await context.DaDanhMucSanPhams
                    .Where(sp => sp.MaDuAn == maDuAn
                                 && sp.MaSanPham != null
                                 && maSanPhams.Contains(sp.MaSanPham!))
                    .Select(sp => sp.MaSanPham!)
                    .ToListAsync();

                if (existedMaSp.Any())
                {
                    errors.Add("Các mã sản phẩm đã tồn tại trong hệ thống: " +
                               string.Join(", ", existedMaSp));
                }

                // 2.2) Block thuộc dự án
                var blocksInDb = await context.DaDanhMucBlocks
                    .Where(b => b.MaDuAn == maDuAn && maBlocks.Contains(b.MaBlock))
                    .Select(b => b.MaBlock)
                    .ToListAsync();

                var missingBlocks = maBlocks
                    .Except(blocksInDb, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (missingBlocks.Any())
                    errors.Add("Không tìm thấy Block: " + string.Join(", ", missingBlocks));

                // 2.3) (MaDuAn, MaBlock, MaTang)
                var distinctTangPairs = items
                    .Select(x => new { x.MaBlock, x.MaTang })
                    .Distinct()
                    .ToList();

                var tangPairsInDb = await context.DaDanhMucTangs
                    .Where(t => t.MaDuAn == maDuAn
                                && maBlocks.Contains(t.MaBlock)
                                && maTangs.Contains(t.MaTang))
                    .Select(t => new { t.MaBlock, t.MaTang })
                    .ToListAsync();

                var tangSet = new HashSet<string>(
                    tangPairsInDb.Select(x => $"{x.MaBlock}|{x.MaTang}"),
                    StringComparer.OrdinalIgnoreCase);

                var missingTangPairs = distinctTangPairs
                    .Where(p => !tangSet.Contains($"{p.MaBlock}|{p.MaTang}"))
                    .ToList();

                if (missingTangPairs.Any())
                {
                    errors.Add("Các cặp (MaBlock, MaTang) không tồn tại: " +
                               string.Join(", ",
                                   missingTangPairs.Select(p => $"{p.MaBlock}-{p.MaTang}")));
                }

                // 2.4) (MaDuAn, MaBlock, MaTruc)
                var distinctTrucPairs = items
                    .Select(x => new { x.MaBlock, x.MaTruc })
                    .Distinct()
                    .ToList();

                var trucPairsInDb = await context.DaDanhMucViewTrucs
                    .Where(tr => tr.MaDuAn == maDuAn
                                 && maBlocks.Contains(tr.MaBlock)
                                 && maTrucs.Contains(tr.MaTruc))
                    .Select(tr => new { tr.MaBlock, tr.MaTruc })
                    .ToListAsync();

                var trucSet = new HashSet<string>(
                    trucPairsInDb.Select(x => $"{x.MaBlock}|{x.MaTruc}"),
                    StringComparer.OrdinalIgnoreCase);

                var missingTrucPairs = distinctTrucPairs
                    .Where(p => !trucSet.Contains($"{p.MaBlock}|{p.MaTruc}"))
                    .ToList();

                if (missingTrucPairs.Any())
                {
                    errors.Add("Các cặp (MaBlock, MaTruc) không tồn tại: " +
                               string.Join(", ",
                                   missingTrucPairs.Select(p => $"{p.MaBlock}-{p.MaTruc}")));
                }

                // 2.5) Loại căn – chỉ check nếu có
                if (maLoaiCans.Any())
                {
                    var loaiCanInDb = await context.DaDanhMucLoaiCanHos
                        .Where(lc => lc.MaDuAn == maDuAn && maLoaiCans.Contains(lc.MaLoaiCanHo))
                        .Select(lc => lc.MaLoaiCanHo)
                        .ToListAsync();

                    var missingLoaiCans = maLoaiCans
                        .Except(loaiCanInDb, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (missingLoaiCans.Any())
                    {
                        errors.Add("Không tìm thấy Loại căn hộ (theo dự án): " +
                                   string.Join(", ", missingLoaiCans));
                    }
                }

                // 2.6) Loại diện tích – chỉ check nếu có
                if (maLoaiDienTichs.Any())
                {
                    var loaiDtInDb = await context.DaLoaiDienTiches
                        .Where(dt => dt.MaDuAn == maDuAn && maLoaiDienTichs.Contains(dt.MaLoaiDt))
                        .Select(dt => dt.MaLoaiDt)
                        .ToListAsync();

                    var missingLoaiDt = maLoaiDienTichs
                        .Except(loaiDtInDb, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (missingLoaiDt.Any())
                    {
                        errors.Add("Không tìm thấy Loại diện tích (theo dự án): " +
                                   string.Join(", ", missingLoaiDt));
                    }
                }

                // 2.7) Layout / Thiết kế – chỉ check nếu có
                if (maLayouts.Any())
                {
                    var layoutsInDb = await context.DaDanhMucLoaiThietKes
                        .Where(ltk => ltk.MaDuAn == maDuAn && maLayouts.Contains(ltk.MaLoaiThietKe))
                        .Select(ltk => ltk.MaLoaiThietKe)
                        .ToListAsync();

                    var missingLayouts = maLayouts
                        .Except(layoutsInDb, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    if (missingLayouts.Any())
                    {
                        errors.Add("Không tìm thấy Mã Layout/Thiết kế (MaLoaiThietKe): " +
                                   string.Join(", ", missingLayouts));
                    }
                }

                // Nếu có lỗi => stop, không insert
                if (errors.Any())
                {
                    var msg = "Lỗi kiểm tra dữ liệu với hệ thống:\n" + string.Join("\n", errors);
                    return ResultModel.Fail(msg);
                }

                // 3) Insert vào DB
                var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                var maNguoiLap = nguoiLap?.MaNhanVien ?? "SYSTEM";
                var now = DateTime.Now;

                foreach (var item in items)
                {
                    context.DaDanhMucSanPhams.Add(new DaDanhMucSanPham
                    {
                        MaDuAn = maDuAn,
                        MaSanPham = item.MaSanPham,
                        TenSanPham = item.TenSanPham,
                        MaBlock = item.MaBlock,
                        MaTang = item.MaTang,
                        MaTruc = item.MaTruc,
                        MaLoaiCan = item.MaLoaiCan,        // có thể null
                        MaLoaiDienTich = item.MaLoaiDienTich,   // có thể null
                        MaLoaiLayout = item.MaLoaiLayout,     // có thể null
                        DienTichTimTuong = item.DienTichTimTuong,
                        DienTichThongThuy = item.DienTichThongThuy,
                        DienTichSanVuon = item.DienTichSanVuon,
                        HeSoCanHo = item.HeSoCanHo,
                        NgayLap = now,
                        NguoiLap = maNguoiLap,
                        HienTrangKd = string.Empty,
                        LoaiSanPham = maLoaiSanPham
                    });
                }

                await context.SaveChangesAsync();
                return ResultModel.Success($"Import thành công {items.Count} sản phẩm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ImportSanPham] Lỗi import file Excel");
                return ResultModel.Fail($"Lỗi import file: {ex.Message}");
            }
        }

        public (List<SanPhamImportModel> Items, List<string> Errors) ReadSanPhamFromExcel(Stream stream)
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

            var table = dataset.Tables["SanPham"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'SanPham' trong file Excel.");

            // Header bắt buộc – đúng như header anh đang để (có dấu *)
            string[] requiredHeaders =
            {
        "MaSanPham*",
        "TenSanPham*",
        "MaBlock*",
        "MaTang*",
        "MaTruc*",
        // 3 cột này có thể có hoặc không – nhưng header vẫn phải tồn tại
        "MaLoaiCan",
        "MaLoaiDienTich",
        "MaLoaiLayout",
        "DienTichTimTuong*",
        "DienTichThongThuy*",
        "DienTichSanVuon",
        "HeSoViewPhu"
    };

            foreach (var header in requiredHeaders)
            {
                if (!table.Columns.Contains(header))
                    throw new Exception($"Thiếu cột '{header}' trong file Excel.");
            }

            var items = new List<SanPhamImportModel>();
            var errors = new List<string>();

            // Detect trùng mã trong file
            var maSpInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            int excelRowIndex = 1; // header = 1

            foreach (DataRow row in table.Rows)
            {
                excelRowIndex++; // data bắt đầu từ dòng 2

                // Đọc theo index để đỡ phụ thuộc tên cột
                string? maSanPham = row[0]?.ToString()?.Trim(); // MaSanPham*
                string? tenSanPham = row[1]?.ToString()?.Trim(); // TenSanPham*
                string? maBlock = row[2]?.ToString()?.Trim(); // MaBlock*
                string? maTang = row[3]?.ToString()?.Trim(); // MaTang*
                string? maTruc = row[4]?.ToString()?.Trim(); // MaTruc*
                string? maLoaiCan = row[5]?.ToString()?.Trim(); // optional
                string? maLoaiDienTich = row[6]?.ToString()?.Trim(); // optional
                string? maLoaiLayout = row[7]?.ToString()?.Trim(); // optional

                // nếu cả dòng trống => bỏ qua
                if (string.IsNullOrWhiteSpace(maSanPham)
                    && string.IsNullOrWhiteSpace(tenSanPham)
                    && string.IsNullOrWhiteSpace(maBlock)
                    && string.IsNullOrWhiteSpace(maTang)
                    && string.IsNullOrWhiteSpace(maTruc))
                {
                    continue;
                }

                var rowErrors = new List<string>();

                // Required
                if (string.IsNullOrWhiteSpace(maSanPham))
                    rowErrors.Add("Mã sản phẩm bắt buộc.");
                if (string.IsNullOrWhiteSpace(tenSanPham))
                    rowErrors.Add("Tên sản phẩm bắt buộc.");
                if (string.IsNullOrWhiteSpace(maBlock))
                    rowErrors.Add("Mã block bắt buộc.");
                if (string.IsNullOrWhiteSpace(maTang))
                    rowErrors.Add("Mã tầng bắt buộc.");
                if (string.IsNullOrWhiteSpace(maTruc))
                    rowErrors.Add("Mã trục bắt buộc.");

                // Trùng mã trong file
                if (!string.IsNullOrWhiteSpace(maSanPham))
                {
                    if (!maSpInFile.Add(maSanPham))
                        rowErrors.Add($"Mã sản phẩm '{maSanPham}' bị trùng trong file.");
                }

                // --- parse số ---

                decimal? dienTichTimTuong = null;
                var rawDttt = row[8]?.ToString()?.Trim(); // DTTT*
                if (string.IsNullOrWhiteSpace(rawDttt))
                {
                    rowErrors.Add("Diện tích tim tường bắt buộc.");
                }
                else if (!decimal.TryParse(rawDttt, NumberStyles.Any, CultureInfo.InvariantCulture, out var dttt))
                {
                    rowErrors.Add("Diện tích tim tường không đúng định dạng số.");
                }
                else
                {
                    dienTichTimTuong = dttt;
                }

                decimal? dienTichThongThuy = null;
                var rawDttThuy = row[9]?.ToString()?.Trim(); // DT thông thủy*
                if (string.IsNullOrWhiteSpace(rawDttThuy))
                {
                    rowErrors.Add("Diện tích thông thủy bắt buộc.");
                }
                else if (!decimal.TryParse(rawDttThuy, NumberStyles.Any, CultureInfo.InvariantCulture, out var dttThuy))
                {
                    rowErrors.Add("Diện tích thông thủy không đúng định dạng số.");
                }
                else
                {
                    dienTichThongThuy = dttThuy;
                }

                decimal? dienTichSanVuon = null;
                var rawDtsv = row[10]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(rawDtsv))
                {
                    if (!decimal.TryParse(rawDtsv, NumberStyles.Any, CultureInfo.InvariantCulture, out var dsv))
                        rowErrors.Add("Diện tích sân vườn không đúng định dạng số.");
                    else
                        dienTichSanVuon = dsv;
                }

                decimal? heSoCanHo = null;
                var rawHsch = row[11]?.ToString()?.Trim();
                if (!string.IsNullOrWhiteSpace(rawHsch))
                {
                    if (!decimal.TryParse(rawHsch, NumberStyles.Any, CultureInfo.InvariantCulture, out var hs))
                        rowErrors.Add("Hệ số view phụ không đúng định dạng số.");
                    else
                        heSoCanHo = hs;
                }

                if (rowErrors.Any())
                {
                    errors.Add($"Dòng {excelRowIndex}: {string.Join(" | ", rowErrors)}");
                    continue;
                }

                items.Add(new SanPhamImportModel
                {
                    ExcelRowIndex = excelRowIndex,
                    MaSanPham = maSanPham,
                    TenSanPham = tenSanPham,
                    MaBlock = maBlock,
                    MaTang = maTang,
                    MaTruc = maTruc,
                    MaLoaiCan = maLoaiCan,        // optional
                    MaLoaiDienTich = maLoaiDienTich,  // optional
                    MaLoaiLayout = maLoaiLayout,    // optional
                    DienTichTimTuong = dienTichTimTuong,
                    DienTichThongThuy = dienTichThongThuy,
                    DienTichSanVuon = dienTichSanVuon,
                    HeSoCanHo = heSoCanHo
                });
            }

            return (items, errors);
        }


        public async Task<(byte[]? FileBytes, string FileName, string ContentType, string? ErrorMessage)> DownloadTemplateAsync(string root)
        {
            try
            {
                var fileName = "FileMauSanPham.xlsx";
                var relativePath = Path.Combine(root, "templates", fileName);
                var absolutePath = Path.GetFullPath(relativePath);

                if (!System.IO.File.Exists(absolutePath))
                {
                    return (null, fileName, "", "Không tìm thấy file mẫu.");
                }

                var fileBytes = await System.IO.File.ReadAllBytesAsync(absolutePath);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                return (fileBytes, fileName, contentType, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải file mẫu sản phẩm");
                return (null, "", "", "Lỗi hệ thống: " + ex.Message);
            }
        }

        public async Task<byte[]> GenerateTemplateWithDataAsync(string templatePath, string maDuAn)
        {
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);

            var duAnSheet = workbook.Worksheet("DuAn");

            // Gọi store lấy dữ liệu
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);

            var data = (await connection.QueryAsync<TemplateSanPhamTabDuAnDto>(
                "Proc_TemplateSanPhamTabDuAn_GetAll",
                 param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            int row = 2;
            foreach (var item in data)
            {
                duAnSheet.Cell(row, 1).Value = item.MaBlock;
                duAnSheet.Cell(row, 2).Value = item.TenBlock;
                duAnSheet.Cell(row, 3).Value = item.MaTang;
                duAnSheet.Cell(row, 4).Value = item.TenTang;
                duAnSheet.Cell(row, 5).Value = item.MaTruc;
                duAnSheet.Cell(row, 6).Value = item.TenTruc;
                duAnSheet.Cell(row, 7).Value = item.MaLoaiCanHo;
                duAnSheet.Cell(row, 8).Value = item.TenLoaiCanHo;
                duAnSheet.Cell(row, 9).Value = item.MaLoaiDT;
                duAnSheet.Cell(row, 10).Value = item.TenLoaiDT;
                duAnSheet.Cell(row, 11).Value = item.MaLoaiThietKe;
                duAnSheet.Cell(row, 12).Value = item.TenLoaiThietKe;
                row++;
            }

            // Set lại sheet "SanPham" là active
            workbook.Worksheet("SanPham").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }
        #endregion
    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.DMSanGiaoDich;
using VTTGROUP.Domain.Model.DuAn;
using VTTGROUP.Domain.Model.PhieuDatCoc;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class DMSanGiaoDichService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<DMSanGiaoDichService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        public DMSanGiaoDichService(IDbContextFactory<AppDbContext> factory, ILogger<DMSanGiaoDichService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _currentUser = currentUser;
        }

        #region Danh sách sàn giao dịch
        public async Task<(List<SanGiaoDichPagingDto> Data, int TotalCount)> GetPagingAsync(string maDuAn, string? trangThai, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@TrangThai", trangThai);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<SanGiaoDichPagingDto>(
                "Proc_SanGiaoDich_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<(
                   List<SanGiaoDichPagingDto> Data,
                   int TotalCount)>
            GetPagingAvailableForDuAnAsync(string maDuAn, int page, int pageSize, string? qSearch)
        {
            if (page <= 0)
            {
                page = 1;
            }
            if (pageSize <= 0)
            {
                pageSize = 10;
            }

            qSearch = string.IsNullOrWhiteSpace(qSearch) ? null : qSearch.Trim();

            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", string.IsNullOrWhiteSpace(maDuAn) ? null : maDuAn);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            // Query lấy ra các sàn giao dịch mà dự án chưa gán, có hỗ trợ tìm kiếm và phân trang
            const string sql = @"WITH base AS (
            SELECT
                s.MaSanGiaoDich,
                ISNULL(s.TenSanGiaoDich, '') AS TenSanGiaoDich,
                ISNULL(s.DiaChi, '') AS DiaChi,
                CASE WHEN s.TrangThai IS NULL THEN NULL ELSE CAST(s.TrangThai AS varchar(10)) END AS TrangThai,
                TRY_CONVERT(datetime, s.NgayLap) AS NgayLap,
                ISNULL(s.NguoiLap, '') AS HoVaTen,
                ISNULL(s.GhiChu, '') AS GhiChu,
                ISNULL(s.DienThoai, '') AS DienThoai,
                ISNULL(s.Email, '') AS Email
            FROM DM_SanGiaoDich s
            WHERE (
                    @MaDuAn IS NULL OR NOT EXISTS (SELECT 1 FROM DM_SanGiaoDich_DuAn d WHERE d.MaSan = s.MaSanGiaoDich AND d.MaDuAn = @MaDuAn)
                ) AND 
                (
                    @QSearch IS NULL OR s.MaSanGiaoDich LIKE '%' + @QSearch + '%' OR s.TenSanGiaoDich LIKE '%' + @QSearch + '%' OR s.DiaChi LIKE '%' + @QSearch + '%')
                )       
                SELECT
                    b.MaSanGiaoDich,
                    b.TenSanGiaoDich,
                    b.DiaChi,
                    b.TrangThai,
                    ISNULL(b.NgayLap, '1900-01-01') AS NgayLap,
                    b.HoVaTen,
                    b.GhiChu,
                    b.DienThoai,
                    b.Email,
                    CAST(0 AS bit) AS IsSelected,
                    COUNT(*) OVER() AS TotalCount
                FROM base b
                ORDER BY b.MaSanGiaoDich
                OFFSET (@Page - 1) * @PageSize ROWS
                FETCH NEXT @PageSize ROWS ONLY;";

            var result = (await connection.QueryAsync<SanGiaoDichPagingDto>(sql, param)).ToList();
            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thông tin sàn giao dịch
        public async Task<ResultModel> GetByIdAsync(string? id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (from t in _context.DmSanGiaoDiches
                                    where t.MaSanGiaoDich == id
                                    select new SanGiaoDichModel
                                    {
                                        MaSanGiaoDich = t.MaSanGiaoDich,
                                        TenSanGiaoDich = t.TenSanGiaoDich ?? string.Empty,
                                        GhiChu = t.GhiChu ?? string.Empty,
                                        DiaChi = t.DiaChi ?? string.Empty,
                                        DienThoai = t.DienThoai ?? string.Empty,
                                        Email = t.Email ?? string.Empty,
                                        TrangThai = t.TrangThai ?? 1,
                                        MaNhanVien = t.NguoiLap ?? string.Empty,
                                        NgayLap = t.NgayLap ?? DateTime.Now,
                                    }).FirstOrDefaultAsync();

                if (entity == null)
                {
                    entity = new SanGiaoDichModel();
                    entity.MaSanGiaoDich = await SinhMaPhieuDCTuDongAsync("SGD-", 5);
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.TrangThai = 1;
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var duAns = await (from t in _context.DmSanGiaoDichDuAns
                                       join da in _context.DaDanhMucDuAns on t.MaDuAn equals da.MaDuAn into daGroup
                                       from da in daGroup.DefaultIfEmpty()
                                       where t.MaSan == id
                                       select new DuAnModel
                                       {
                                           MaDuAn = t.MaDuAn,
                                           TenDuAn = da.TenDuAn
                                       }).ToListAsync();
                    entity.ListDuAn = duAns;
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin quy sàn giao dịch");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thêm, xóa, sửa sàn giao dịch
        public async Task<ResultModel> SavePhieuAsync(SanGiaoDichModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                DmSanGiaoDich record = new DmSanGiaoDich
                {
                    MaSanGiaoDich = await SinhMaPhieuDCTuDongAsync("SGD-", 5),
                    TenSanGiaoDich = model.TenSanGiaoDich,
                    GhiChu = model.GhiChu,
                    DiaChi = model.DiaChi,
                    DienThoai = model.DienThoai,
                    Email = model.Email,
                    NgayLap = DateTime.Now,
                    TrangThai = model.TrangThai,
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                await _context.DmSanGiaoDiches.AddAsync(record);
                await _context.SaveChangesAsync();

                var listDuAns = new List<DmSanGiaoDichDuAn>();
                if (model.ListDuAn != null && model.ListDuAn.Any())
                {
                    foreach (var item in model.ListDuAn)
                    {
                        var r = new DmSanGiaoDichDuAn
                        {
                            MaDuAn = item.MaDuAn,
                            MaSan = record.MaSanGiaoDich
                        };
                        listDuAns.Add(r);
                    }
                    await _context.DmSanGiaoDichDuAns.AddRangeAsync(listDuAns);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaSanGiaoDich.ToString(), $"Thêm sàn giao dịch {model.TenSanGiaoDich} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SavePhieuAsync] Lỗi khi thêm quy trình duyệt");
                return ResultModel.Fail($"Lỗi hệ thống: không thể thêm: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuAsync(SanGiaoDichModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DmSanGiaoDiches.FirstOrDefaultAsync(d => d.MaSanGiaoDich == model.MaSanGiaoDich);
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy sàn giao dịch.");
                }

                entity.TenSanGiaoDich = model.TenSanGiaoDich;
                entity.GhiChu = model.GhiChu;
                entity.DiaChi = model.DiaChi;
                entity.DienThoai = model.DienThoai;
                entity.Email = model.Email;
                entity.TrangThai = model.TrangThai;

                var delDuAns = await _context.DmSanGiaoDichDuAns.Where(d => d.MaSan == model.MaSanGiaoDich).ToListAsync();
                _context.DmSanGiaoDichDuAns.RemoveRange(delDuAns);

                var listDuAns = new List<DmSanGiaoDichDuAn>();
                if (model.ListDuAn != null && model.ListDuAn.Any())
                {
                    foreach (var item in model.ListDuAn)
                    {
                        var r = new DmSanGiaoDichDuAn
                        {
                            MaDuAn = item.MaDuAn,
                            MaSan = entity.MaSanGiaoDich
                        };
                        listDuAns.Add(r);
                    }

                    await _context.DmSanGiaoDichDuAns.AddRangeAsync(listDuAns);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaSanGiaoDich.ToString(), $"Cập nhật sàn giao dịch {model.TenSanGiaoDich} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdatePhieuAsync] Lỗi khi Cập nhật sàn giao dịch");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeletePhieuAsync(
    string maSanGiaoDich,
    CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maSanGiaoDich))
                    return ResultModel.Fail("Thiếu mã sàn giao dịch.");

                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // B1) Tìm sàn
                var san = await context.DmSanGiaoDiches
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.MaSanGiaoDich == maSanGiaoDich, ct);

                if (san == null)
                    return ResultModel.Fail("Không tìm thấy sàn giao dịch.");

                // B2) Kiểm tra ràng buộc sử dụng
                var usedPgc = await context.BhPhieuGiuChos
                    .AsNoTracking()
                    .Where(x => x.MaSanMoiGioi == maSanGiaoDich)
                    .TagWith("DeleteSanGD: check BH_PhieuGiuCho")
                    .CountAsync(ct);

                var usedDkcc = await context.BhPhieuDangKiChonCans
                    .AsNoTracking()
                    .Where(x => x.SanGiaoDich == maSanGiaoDich)
                    .TagWith("DeleteSanGD: check BH_PhieuDangKiChonCan")
                    .CountAsync(ct);

                var usedTongHop = await context.KdPhieuTongHopBookings
                    .AsNoTracking()
                    .Where(x => x.MaSanGiaoDich == maSanGiaoDich)
                    .TagWith("DeleteSanGD: check KD_PhieuTongHopBooking")
                    .CountAsync(ct);

                var usedHoanTien = await context.KdPhieuDeNghiHoanTienBookings
                    .AsNoTracking()
                    .Where(x => x.MaSanGiaoDich == maSanGiaoDich)
                    .TagWith("DeleteSanGD: check KD_PhieuDeNghiHoanTienBooking")
                    .CountAsync(ct);

                var usedGioHang = await context.BhGioHangs
                    .AsNoTracking()
                    .Where(x => x.MaSanGiaoDich == maSanGiaoDich)
                    .TagWith("DeleteSanGD: check BH_GioHang")
                    .CountAsync(ct);

                var usedNhanVien = await context.TblNhanviens
                    .AsNoTracking()
                    .Where(x => x.MaSanGiaoDich == maSanGiaoDich)
                    .TagWith("DeleteSanGD: check TBL_NHANVIEN")
                    .CountAsync(ct);

                if (usedPgc + usedDkcc + usedTongHop + usedHoanTien + usedGioHang + usedNhanVien > 0)
                {
                    // build thông điệp chi tiết
                    var lines = new List<string>();
                    if (usedPgc > 0) lines.Add($"- {usedPgc} phiếu giữ chỗ");
                    if (usedDkcc > 0) lines.Add($"- {usedDkcc} phiếu đăng ký chọn căn");
                    if (usedTongHop > 0) lines.Add($"- {usedTongHop} phiếu tổng hợp booking");
                    if (usedHoanTien > 0) lines.Add($"- {usedHoanTien} phiếu đề nghị hoàn tiền booking");
                    if (usedGioHang > 0) lines.Add($"- {usedGioHang} giỏ hàng");
                    if (usedNhanVien > 0) lines.Add($"- {usedNhanVien} nhân viên");

                    var detail = string.Join(Environment.NewLine, lines);
                    return ResultModel.Fail(
                        $"Không thể xoá vì sàn giao dịch đang được sử dụng:\n{detail}");
                }

                // B3) Xoá trong transaction: map dự án trước, rồi bản ghi sàn
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                _ = await context.DmSanGiaoDichDuAns
                    .Where(d => d.MaSan == maSanGiaoDich)
                    .TagWith("DeleteSanGD: delete DmSanGiaoDich_DuAn")
                    .ExecuteDeleteAsync(ct);

                _ = await context.DmSanGiaoDiches
                    .Where(d => d.MaSanGiaoDich == maSanGiaoDich)
                    .TagWith("DeleteSanGD: delete DmSanGiaoDich")
                    .ExecuteDeleteAsync(ct);

                await tx.CommitAsync(ct);

                return ResultModel.Success("Xoá sàn giao dịch thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePhieuAsync] Lỗi khi xoá sàn giao dịch {MaSanGiaoDich}", maSanGiaoDich);
                return ResultModel.Fail("Lỗi hệ thống: không thể xoá sàn giao dịch.");
            }
        }

        public async Task<ResultModel> DeleteListAsync(
     List<SanGiaoDichPagingDto> listSGD,
     CancellationToken ct = default)
        {
            const int BatchSize = 1800;

            try
            {
                // B0) Normalize & guard
                var ids = (listSGD ?? new())
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaSanGiaoDich))
                    .Select(x => x!.MaSanGiaoDich.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có sàn giao dịch nào được chọn để xoá.");

                await using var context = _factory.CreateDbContext();
                context.ChangeTracker.Clear();

                // B1) Kiểm tra ràng buộc
                var blocked = new Dictionary<string, Count6>(StringComparer.OrdinalIgnoreCase);

                foreach (var chunk in Chunk(ids, BatchSize))
                {
                    // PGiữ chỗ
                    foreach (var r in await context.BhPhieuGiuChos.AsNoTracking()
                                 .Where(x => x.MaSanMoiGioi != null && chunk.Contains(x.MaSanMoiGioi))
                                 .GroupBy(x => x.MaSanMoiGioi!)
                                 .Select(g => new { Key = g.Key, Count = g.Count() })
                                 .ToListAsync(ct))
                        blocked[r.Key] = blocked.TryGetValue(r.Key, out var c) ? c with { Pgc = c.Pgc + r.Count } : new Count6 { Pgc = r.Count };

                    // ĐK chọn căn
                    foreach (var r in await context.BhPhieuDangKiChonCans.AsNoTracking()
                                 .Where(x => x.SanGiaoDich != null && chunk.Contains(x.SanGiaoDich))
                                 .GroupBy(x => x.SanGiaoDich!)
                                 .Select(g => new { Key = g.Key, Count = g.Count() })
                                 .ToListAsync(ct))
                        blocked[r.Key] = blocked.TryGetValue(r.Key, out var c) ? c with { Dkcc = c.Dkcc + r.Count } : new Count6 { Dkcc = r.Count };

                    // Tổng hợp booking
                    foreach (var r in await context.KdPhieuTongHopBookings.AsNoTracking()
                                 .Where(x => x.MaSanGiaoDich != null && chunk.Contains(x.MaSanGiaoDich))
                                 .GroupBy(x => x.MaSanGiaoDich!)
                                 .Select(g => new { Key = g.Key, Count = g.Count() })
                                 .ToListAsync(ct))
                        blocked[r.Key] = blocked.TryGetValue(r.Key, out var c) ? c with { TongHop = c.TongHop + r.Count } : new Count6 { TongHop = r.Count };

                    // Hoàn tiền booking
                    foreach (var r in await context.KdPhieuDeNghiHoanTienBookings.AsNoTracking()
                                 .Where(x => x.MaSanGiaoDich != null && chunk.Contains(x.MaSanGiaoDich))
                                 .GroupBy(x => x.MaSanGiaoDich!)
                                 .Select(g => new { Key = g.Key, Count = g.Count() })
                                 .ToListAsync(ct))
                        blocked[r.Key] = blocked.TryGetValue(r.Key, out var c) ? c with { HoanTien = c.HoanTien + r.Count } : new Count6 { HoanTien = r.Count };

                    // Giỏ hàng
                    foreach (var r in await context.BhGioHangs.AsNoTracking()
                                 .Where(x => x.MaSanGiaoDich != null && chunk.Contains(x.MaSanGiaoDich))
                                 .GroupBy(x => x.MaSanGiaoDich!)
                                 .Select(g => new { Key = g.Key, Count = g.Count() })
                                 .ToListAsync(ct))
                        blocked[r.Key] = blocked.TryGetValue(r.Key, out var c) ? c with { GioHang = c.GioHang + r.Count } : new Count6 { GioHang = r.Count };

                    // Nhân viên
                    foreach (var r in await context.TblNhanviens.AsNoTracking()
                                 .Where(x => x.MaSanGiaoDich != null && chunk.Contains(x.MaSanGiaoDich))
                                 .GroupBy(x => x.MaSanGiaoDich!)
                                 .Select(g => new { Key = g.Key, Count = g.Count() })
                                 .ToListAsync(ct))
                        blocked[r.Key] = blocked.TryGetValue(r.Key, out var c) ? c with { NhanVien = c.NhanVien + r.Count } : new Count6 { NhanVien = r.Count };
                }

                var blockedIds = blocked.Where(kv => kv.Value.Total > 0)
                                        .Select(kv => kv.Key)
                                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var deletableIds = ids.Where(id => !blockedIds.Contains(id)).ToList();

                if (deletableIds.Count == 0)
                {
                    var lines = blocked.OrderBy(k => k.Key).Select(kv =>
                    {
                        var v = kv.Value;
                        var parts = new List<string>();
                        if (v.Pgc > 0) parts.Add($"PGC={v.Pgc}");
                        if (v.Dkcc > 0) parts.Add($"ĐKCC={v.Dkcc}");
                        if (v.TongHop > 0) parts.Add($"TổngHợp={v.TongHop}");
                        if (v.HoanTien > 0) parts.Add($"HoànTiền={v.HoanTien}");
                        if (v.GioHang > 0) parts.Add($"GiỏHàng={v.GioHang}");
                        if (v.NhanVien > 0) parts.Add($"NhânViên={v.NhanVien}");
                        return $"- [{kv.Key}] đang được dùng ở: {string.Join(", ", parts)}";
                    });
                    return ResultModel.Fail("Không thể xoá: tất cả sàn giao dịch được chọn đều đang được sử dụng.\n" +
                                            string.Join(Environment.NewLine, lines));
                }

                // B2) Xoá trong DB (transaction) – xoá mapping dự án trước
                await using var tx = await context.Database.BeginTransactionAsync(ct);
                var totalDeleted = 0;

                foreach (var chunk in Chunk(deletableIds, BatchSize))
                {
                    _ = await context.DmSanGiaoDichDuAns
                        .Where(d => chunk.Contains(d.MaSan!))
                        .ExecuteDeleteAsync(ct);

                    var affected = await context.DmSanGiaoDiches
                        .Where(d => chunk.Contains(d.MaSanGiaoDich))
                        .ExecuteDeleteAsync(ct);

                    totalDeleted += affected;
                }

                await tx.CommitAsync(ct);

                // B3) Message
                var skipped = ids.Count - totalDeleted;
                var baseMsg = $"Đã xoá {totalDeleted}/{ids.Count} sàn giao dịch. " +
                              (skipped > 0 ? $"{skipped} sàn không xoá (đang được sử dụng hoặc không tồn tại). " : "");

                string blockedDetail = "";
                if (blockedIds.Count > 0)
                {
                    var top = blocked.Where(kv => kv.Value.Total > 0)
                                     .OrderBy(kv => kv.Key).Take(10)
                                     .Select(kv =>
                                     {
                                         var v = kv.Value;
                                         var parts = new List<string>();
                                         if (v.Pgc > 0) parts.Add($"PGC={v.Pgc}");
                                         if (v.Dkcc > 0) parts.Add($"ĐKCC={v.Dkcc}");
                                         if (v.TongHop > 0) parts.Add($"TổngHợp={v.TongHop}");
                                         if (v.HoanTien > 0) parts.Add($"HoànTiền={v.HoanTien}");
                                         if (v.GioHang > 0) parts.Add($"GiỏHàng={v.GioHang}");
                                         if (v.NhanVien > 0) parts.Add($"NhânViên={v.NhanVien}");
                                         return $"- [{kv.Key}] dùng ở: {string.Join(", ", parts)}";
                                     });

                    blockedDetail = "\nChi tiết ràng buộc:\n" + string.Join(Environment.NewLine, top) +
                                    (blockedIds.Count > 10 ? $"\n... và {blockedIds.Count - 10} sàn khác." : "");
                }

                return ResultModel.Success(baseMsg + blockedDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SGD.DeleteListAsync] Lỗi khi xoá danh sách sàn giao dịch");
                return ResultModel.Fail("Lỗi hệ thống khi xoá danh sách sàn giao dịch.");
            }
        }

        // helper: gộp theo batch
        private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.Skip(i).Take(Math.Min(size, source.Count - i)).ToList();
        }

        // struct đếm ràng buộc (C# 10+ có 'with' cho record/struct)
        private readonly record struct Count6
        {
            public int Pgc { get; init; }
            public int Dkcc { get; init; }
            public int TongHop { get; init; }
            public int HoanTien { get; init; }
            public int GioHang { get; init; }
            public int NhanVien { get; init; }
            public int Total => Pgc + Dkcc + TongHop + HoanTien + GioHang + NhanVien;
        }

        #endregion

        #region Hàm tăng tự động của mã phiếu giữ chỗ   
        public async Task<string> SinhMaPhieuDCTuDongAsync(string prefix, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.DmSanGiaoDiches
                .Where(kh => kh.MaSanGiaoDich.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaSanGiaoDich)
                .FirstOrDefaultAsync();

            // B2: Tách phần số
            int maxSo = 0;
            if (!string.IsNullOrEmpty(maLonNhat))
            {
                var soPart = maLonNhat.Replace(prefix, "");
                int.TryParse(soPart, out maxSo);
            }

            // B3: Tăng lên và format
            string maMoi = $"{prefix}{(maxSo + 1).ToString($"D{padding}")}";
            return maMoi;
        }
        #endregion
    }
}

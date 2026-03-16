using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.ChinhSachThanhToan;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ChinhSachThanhToanService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<ChinhSachThanhToanService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;

        public ChinhSachThanhToanService(IDbContextFactory<AppDbContext> factory, ILogger<ChinhSachThanhToanService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
        }

        #region Hiển thị danh sách chính sách thanh toán
        public async Task<(List<ChinhSachThanhToanPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? maDuAn, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<ChinhSachThanhToanPagingDto>(
                "Proc_ChinhSachThanhToan_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa chính sách thanh toán
        public async Task<ResultModel> SaveChinhSachTTAsync(ChinhSachThanhToanModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new BhChinhSachThanhToan
                {
                    MaCstt = await SinhMaPhieuAsync("CSTT-", _context, 5),
                    TenCstt = model.TenCSTT,
                    MaDuAn = model.MaDuAn,
                    TyLeChietKhau = model.TyLeChietKhau,
                    NoiDung = model.NoiDung,
                    IsXacNhan = false
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;
                await _context.BhChinhSachThanhToans.AddAsync(record);
                //Insert chính sách thanh toán
                if (model.ListChinhSachThanhToan != null & model.ListChinhSachThanhToan.Any() == true)
                {
                    List<BhChinhSachThanhToanChiTiet> listCT = new List<BhChinhSachThanhToanChiTiet>();
                    foreach (var item in model.ListChinhSachThanhToan)
                    {
                        var r = new BhChinhSachThanhToanChiTiet();
                        r.MaCstt = record.MaCstt;
                        r.DotTt = item.DotTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.MaKyTt = item.MaKyTT;
                        r.SoKhoangCachNgay = item.SoKhoangCachNgay;
                        r.DotThamChieu = item.DotThamChieu;
                        r.TyLeTtdatCoc = item.TyLeTTDatCoc;
                        r.IsCongNoDc = item.IsCongNoDC;
                        listCT.Add(r);
                    }
                    await _context.BhChinhSachThanhToanChiTiets.AddRangeAsync(listCT);
                }
                //Insert tiến độ thanh toán hợp đồng
                var delTDTTHD = _context.BhChinhSachThanhToanChiTietHopDongs.Where(d => d.MaCstt == record.MaCstt);
                if (model.ListChinhSachThanhToanHD != null & model.ListChinhSachThanhToanHD.Any() == true)
                {
                    List<BhChinhSachThanhToanChiTietHopDong> listCT = new List<BhChinhSachThanhToanChiTietHopDong>();
                    foreach (var item in model.ListChinhSachThanhToanHD)
                    {
                        var r = new BhChinhSachThanhToanChiTietHopDong();
                        r.MaCstt = record.MaCstt;
                        r.DotTt = item.DotTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.MaKyTt = item.MaKyTT;
                        r.SoKhoangCachNgay = item.SoKhoangCachNgay;
                        r.DotThamChieu = item.DotThamChieu;
                        r.TyLeTt = item.TyLeTTHopDong;
                        r.IsCongNo = item.IsCongNoHD;
                        r.TyLeTtvat = item.TyLeTTVAT;
                        listCT.Add(r);
                    }
                    await _context.BhChinhSachThanhToanChiTietHopDongs.AddRangeAsync(listCT);
                }
                _context.BhChinhSachThanhToanChiTietHopDongs.RemoveRange(delTDTTHD);
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaCstt, "Thêm chính sách thanh toán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm chính sách thanh toán");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm chính sách thanh toán: {ex.Message}");
            }
        }
        //public async Task<ResultModel> UpdateByIdAsync(ChinhSachThanhToanModel model)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var entity = await _context.BhChinhSachThanhToans.FirstOrDefaultAsync(d => d.MaCstt.ToLower() == model.MaCSTT.ToLower());
        //        if (entity == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy chính sách thanh toán nào.");
        //        }
        //        entity.TyLeChietKhau = model.TyLeChietKhau;
        //        entity.TenCstt = model.TenCSTT;
        //        entity.NoiDung = model.NoiDung;
        //        //Insert chính sách thanh toán
        //        var delTDTT = _context.BhChinhSachThanhToanChiTiets.Where(d => d.MaCstt == entity.MaCstt);
        //        if (model.ListChinhSachThanhToan != null & model.ListChinhSachThanhToan.Any() == true)
        //        {
        //            List<BhChinhSachThanhToanChiTiet> listCT = new List<BhChinhSachThanhToanChiTiet>();
        //            foreach (var item in model.ListChinhSachThanhToan)
        //            {
        //                var r = new BhChinhSachThanhToanChiTiet();
        //                r.MaCstt = entity.MaCstt;
        //                r.DotTt = item.DotTT;
        //                r.NoiDungTt = item.NoiDungTT;
        //                r.MaKyTt = item.MaKyTT;
        //                r.SoKhoangCachNgay = item.SoKhoangCachNgay;
        //                r.DotThamChieu = item.DotThamChieu;
        //                r.TyLeTtdatCoc = item.TyLeTTDatCoc;
        //                r.IsCongNoDc = item.IsCongNoDC;
        //                r.TyLeTtdatCocVat = item.TyLeTTVAT;
        //                listCT.Add(r);
        //            }
        //            await _context.BhChinhSachThanhToanChiTiets.AddRangeAsync(listCT);
        //        }
        //        _context.BhChinhSachThanhToanChiTiets.RemoveRange(delTDTT);
        //        //Insert tiến độ thanh toán hợp đồng
        //        var delTDTTHD = _context.BhChinhSachThanhToanChiTietHopDongs.Where(d => d.MaCstt == entity.MaCstt);
        //        if (model.ListChinhSachThanhToanHD != null & model.ListChinhSachThanhToanHD.Any() == true)
        //        {
        //            List<BhChinhSachThanhToanChiTietHopDong> listCT = new List<BhChinhSachThanhToanChiTietHopDong>();
        //            foreach (var item in model.ListChinhSachThanhToanHD)
        //            {
        //                var r = new BhChinhSachThanhToanChiTietHopDong();
        //                r.MaCstt = entity.MaCstt;
        //                r.DotTt = item.DotTT;
        //                r.NoiDungTt = item.NoiDungTT;
        //                r.MaKyTt = item.MaKyTT;
        //                r.SoKhoangCachNgay = item.SoKhoangCachNgay;
        //                r.DotThamChieu = item.DotThamChieu;
        //                r.TyLeTt = item.TyLeTTHopDong;
        //                r.IsCongNo = item.IsCongNoHD;
        //                r.TyLeTtvat = item.TyLeTTVAT;
        //                listCT.Add(r);
        //            }
        //            await _context.BhChinhSachThanhToanChiTietHopDongs.AddRangeAsync(listCT);
        //        }
        //        _context.BhChinhSachThanhToanChiTietHopDongs.RemoveRange(delTDTTHD);
        //        await _context.SaveChangesAsync();
        //        return ResultModel.SuccessWithId(entity.MaCstt, "Cập nhật chính sách thanh toán thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi cập nhật chính sách thanh toán");
        //        return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật chính sách thanh toán: {ex.Message.ToString()}");
        //    }
        //}

        public async Task<ResultModel> UpdateByIdAsync(ChinhSachThanhToanModel model, CancellationToken ct = default)
        {
            if (model == null)
                return ResultModel.Fail("Dữ liệu không hợp lệ.");

            if (string.IsNullOrWhiteSpace(model.MaCSTT))
                return ResultModel.Fail("Mã CSTT không hợp lệ.");

            var maCstt = model.MaCSTT.Trim();
            var maCsttUpper = maCstt.ToUpperInvariant();

            await using var db = await _factory.CreateDbContextAsync(ct);
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            try
            {
                // 1) Load parent
                var entity = await db.BhChinhSachThanhToans
                    .FirstOrDefaultAsync(x => (x.MaCstt ?? "").ToUpper() == maCsttUpper, ct);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy chính sách thanh toán nào.");

                // 2) Update parent fields
                entity.TyLeChietKhau = model.TyLeChietKhau;
                entity.TenCstt = model.TenCSTT;
                entity.NoiDung = model.NoiDung;

                // 3) XÓA toàn bộ dữ liệu con trước (nhanh + sạch)
                await db.BhChinhSachThanhToanChiTiets
                    .Where(x => x.MaCstt == entity.MaCstt)
                    .ExecuteDeleteAsync(ct);

                await db.BhChinhSachThanhToanChiTietHopDongs
                    .Where(x => x.MaCstt == entity.MaCstt)
                    .ExecuteDeleteAsync(ct);

                // ✅ bảng map cấn trừ đặt cọc
                await db.BhChinhSachThanhToanChiTietHopDongCanTruDatCocs
                    .Where(x => x.MaCstt == entity.MaCstt)
                    .ExecuteDeleteAsync(ct);

                // 4) Insert lại tiến độ thanh toán ĐẶT CỌC
                if (model.ListChinhSachThanhToan?.Any() == true)
                {
                    var listDC = model.ListChinhSachThanhToan
                        .Where(x => x != null)
                        .Select(x => new BhChinhSachThanhToanChiTiet
                        {
                            MaCstt = entity.MaCstt,
                            DotTt = x.DotTT,
                            NoiDungTt = x.NoiDungTT,
                            MaKyTt = x.MaKyTT,
                            SoKhoangCachNgay = x.SoKhoangCachNgay,
                            DotThamChieu = x.DotThamChieu,
                            TyLeTtdatCoc = x.TyLeTTDatCoc,
                            IsCongNoDc = x.IsCongNoDC,
                            TyLeTtdatCocVat = x.TyLeTTVAT
                        })
                        .ToList();

                    if (listDC.Count > 0)
                        await db.BhChinhSachThanhToanChiTiets.AddRangeAsync(listDC, ct);
                }

                // 5) Insert lại tiến độ thanh toán HỢP ĐỒNG
                if (model.ListChinhSachThanhToanHD?.Any() == true)
                {
                    var listHD = model.ListChinhSachThanhToanHD
                        .Where(x => x != null)
                        .Select(x => new BhChinhSachThanhToanChiTietHopDong
                        {
                            MaCstt = entity.MaCstt,
                            DotTt = x.DotTT,
                            NoiDungTt = x.NoiDungTT,
                            MaKyTt = x.MaKyTT,
                            SoKhoangCachNgay = x.SoKhoangCachNgay,
                            DotThamChieu = x.DotThamChieu,
                            TyLeTt = x.TyLeTTHopDong,
                            IsCongNo = x.IsCongNoHD,
                            TyLeTtvat = x.TyLeTTVAT
                        })
                        .ToList();

                    if (listHD.Count > 0)
                        await db.BhChinhSachThanhToanChiTietHopDongs.AddRangeAsync(listHD, ct);
                }

                // 6) ✅ Insert lại bảng MAP: HĐ cấn trừ Đặt cọc
                // - lọc placeholder
                // - distinct để tránh trùng
                if (model.ListChinhSachHopDongCanTruDatCoc?.Any() == true)
                {
                    var map = model.ListChinhSachHopDongCanTruDatCoc
                        .Where(x => x != null && x.DotTTHD > 0 && x.DotTTDC > 0)
                        .Select(x => new BhChinhSachThanhToanChiTietHopDongCanTruDatCoc
                        {
                            MaCstt = entity.MaCstt,
                            DotTthopDong = x.DotTTHD,
                            DotTtdatCoc = x.DotTTDC
                        })
                        .GroupBy(x => new { x.MaCstt, x.DotTthopDong, x.DotTtdatCoc })
                        .Select(g => g.First())
                        .ToList();

                    if (map.Count > 0)
                        await db.BhChinhSachThanhToanChiTietHopDongCanTruDatCocs.AddRangeAsync(map, ct);
                }

                // 7) Save + Commit
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ResultModel.SuccessWithId(entity.MaCstt, "Cập nhật chính sách thanh toán thành công");
            }
            catch (OperationCanceledException)
            {
                // chuẩn: bị cancel thì throw tiếp
                throw;
            }
            catch (Exception ex)
            {
                try { await tx.RollbackAsync(ct); } catch { /* ignore */ }

                _logger.LogError(ex,
                    "Lỗi khi cập nhật chính sách thanh toán. MaCSTT={MaCSTT}",
                    maCstt);

                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật chính sách thanh toán: {ex.Message}");
            }
        }


        public async Task<ResultModel> DeleteRecordAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhChinhSachThanhToans.Where(d => d.MaCstt == maPhieu).FirstOrDefaultAsync();
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy chính sách thanh toán");

                var del = await _context.BhChinhSachThanhToanChiTiets.Where(d => d.MaCstt == entity.MaCstt).ToListAsync();
                _context.BhChinhSachThanhToanChiTiets.RemoveRange(del);

                var delTDTTHD = await _context.BhChinhSachThanhToanChiTietHopDongs.Where(d => d.MaCstt == entity.MaCstt).ToListAsync();
                _context.BhChinhSachThanhToanChiTietHopDongs.RemoveRange(delTDTTHD);

                _context.BhChinhSachThanhToans.Remove(entity);
                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xóa chính sách thanh toán {entity.MaCstt} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteRecordAsync] Lỗi khi xóa chính sách thanh toán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin chính sách thanh toán
        public async Task<ResultModel> FindGetByPhieuAsync(string? id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new ChinhSachThanhToanModel();
                if (!string.IsNullOrEmpty(id))
                {
                    record = await (
                      from cstt in _context.BhChinhSachThanhToans
                      join duan in _context.DaDanhMucDuAns on cstt.MaDuAn equals duan.MaDuAn

                      where cstt.MaCstt == id
                      select new ChinhSachThanhToanModel
                      {
                          MaCSTT = cstt.MaCstt,
                          TenCSTT = cstt.TenCstt,
                          NgayLap = cstt.NgayLap,
                          MaDuAn = cstt.MaDuAn ?? string.Empty,
                          TenDuAn = duan.TenDuAn,
                          NoiDung = cstt.NoiDung,
                          MaNhanVien = cstt.NguoiLap ?? string.Empty,
                          TyLeChietKhau = cstt.TyLeChietKhau ?? 0,
                          IsXacNhan = cstt.IsXacNhan ?? false

                      }).FirstOrDefaultAsync();
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);

                    record.ListChinhSachThanhToan = await (from ct in _context.BhChinhSachThanhToanChiTiets
                                                           join cs in _context.DaDanhMucTienDoKyThanhToans on ct.MaKyTt equals cs.MaKyTt into dtDong
                                                           from cs2 in dtDong.DefaultIfEmpty()
                                                           where ct.MaCstt == id
                                                           select new ChinhSachThanhToanChiTietModel
                                                           {
                                                               MaCSTT = ct.MaCstt ?? string.Empty,
                                                               NoiDungTT = ct.NoiDungTt ?? string.Empty,
                                                               DotTT = ct.DotTt ?? 1,
                                                               DotThamChieu = ct.DotThamChieu ?? 0,
                                                               MaKyTT = ct.MaKyTt ?? string.Empty,
                                                               TenKyTT = cs2.TenKyTt ?? string.Empty,
                                                               SoKhoangCachNgay = ct.SoKhoangCachNgay ?? 0,
                                                               TyLeTTDatCoc = ct.TyLeTtdatCoc ?? 0,
                                                               IsCongNoDC = ct.IsCongNoDc ?? false,
                                                               TyLeTTVAT = ct.TyLeTtdatCocVat ?? 0
                                                           }).ToListAsync();
                    record.ListChinhSachThanhToanHD = await (from ct in _context.BhChinhSachThanhToanChiTietHopDongs
                                                             join cs in _context.DaDanhMucTienDoKyThanhToans on ct.MaKyTt equals cs.MaKyTt into dtDong
                                                             from cs2 in dtDong.DefaultIfEmpty()
                                                             where ct.MaCstt == id
                                                             select new ChinhSachThanhToanHopDongChiTietModel
                                                             {
                                                                 MaCSTT = ct.MaCstt ?? string.Empty,
                                                                 NoiDungTT = ct.NoiDungTt ?? string.Empty,
                                                                 DotTT = ct.DotTt ?? 1,
                                                                 DotThamChieu = ct.DotThamChieu ?? 0,
                                                                 MaKyTT = ct.MaKyTt ?? string.Empty,
                                                                 TenKyTT = cs2.TenKyTt ?? string.Empty,
                                                                 SoKhoangCachNgay = ct.SoKhoangCachNgay ?? 0,
                                                                 TyLeTTHopDong = ct.TyLeTt ?? 0,
                                                                 TyLeTTVAT = ct.TyLeTtvat ?? 0,
                                                                 IsCongNoHD = ct.IsCongNo ?? false,
                                                             }).ToListAsync();
                    // Cấn trừ đặt cọc (map HĐ -> ĐC)
                    record.ListChinhSachHopDongCanTruDatCoc = await
                                                (
                                                    from map in _context.BhChinhSachThanhToanChiTietHopDongCanTruDatCocs.AsNoTracking()
                                                        // join HĐ theo (MaCSTT, DotTT)
                                                    join hd in _context.BhChinhSachThanhToanChiTietHopDongs.AsNoTracking()
                                                        on new
                                                        {
                                                            MaCstt = (map.MaCstt ?? "").Trim(),
                                                            Dot = (int?)map.DotTthopDong       // ✅ ép về int?
                                                        }
                                                        equals new
                                                        {
                                                            MaCstt = (hd.MaCstt ?? "").Trim(),
                                                            Dot = hd.DotTt                    // ✅ int?
                                                        } into jhd
                                                    from hd2 in jhd.DefaultIfEmpty()
                                                        // join ĐC theo (MaCSTT, DotTT)
                                                    join dc in _context.BhChinhSachThanhToanChiTiets.AsNoTracking()
                                                        on new
                                                        {
                                                            MaCstt = (map.MaCstt ?? "").Trim(),
                                                            Dot = (int?)map.DotTtdatCoc       // ✅ ép về int?
                                                        }
                                                        equals new
                                                        {
                                                            MaCstt = (dc.MaCstt ?? "").Trim(),
                                                            Dot = dc.DotTt                    // ✅ int?
                                                        } into jdc
                                                    from dc2 in jdc.DefaultIfEmpty()

                                                    where (map.MaCstt ?? "").Trim() == id.Trim()
                                                    orderby map.DotTthopDong, map.DotTtdatCoc
                                                    select new ChinhSachThanhToanHopDongChiTietCanTruDatCoc
                                                    {
                                                        MaCSTT = map.MaCstt ?? string.Empty,

                                                        DotTTHD = map.DotTthopDong,
                                                        NoiDungTTHD = hd2 != null ? hd2.NoiDungTt : null,
                                                        TyLeTTHD = hd2 != null ? (hd2.TyLeTt ?? 0) : 0,      // ✅ đúng tỷ lệ TT

                                                        DotTTDC = map.DotTtdatCoc,
                                                        NoiDungTTDC = dc2 != null ? dc2.NoiDungTt : null,
                                                        TyLeTTDC = dc2.TyLeTtdatCoc                          // ✅ tỷ lệ cấn trừ lưu ở map
                                                    }
                                                ).ToListAsync();

                }
                else
                {
                    record.MaCSTT = await SinhMaPhieuAsync("CSTT-", _context, 5);
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    record.NgayLap = DateTime.Now;
                    record.ListChinhSachThanhToan = new List<ChinhSachThanhToanChiTietModel>();
                }
                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (Exception ex)
            {
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin phiếu duyệt giá: {ex.Message}");
            }
        }
        #endregion


        #region Hàm tăng tự động của mã phiếu
        public async Task<string> SinhMaPhieuAsync(string prefix, AppDbContext context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await context.BhChinhSachThanhToans
                .Where(kh => kh.MaCstt.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaCstt)
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

        #region Load danh mục kỳ thanh toán
        public async Task<List<DaDanhMucTienDoKyThanhToan>> GetByTienDoKyThanhToanAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucTienDoKyThanhToan>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucTienDoKyThanhToans.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucTienDoKyThanhToan>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách kỳ thanh toán");
            }
            return entity;
        }
        #endregion

        #region Danh sách cấn trừ đặt cọc
        public async Task<List<ChinhSachThanhToanChiTietModel>> GetCanTruDatCocAsync(
     string maCstt,int dotTTHopDong,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maCstt))
                return new();

            maCstt = maCstt.Trim();

            await using var db = await _factory.CreateDbContextAsync(ct);

            try
            {
                return await (
                    from dc in db.BhChinhSachThanhToanChiTiets.AsNoTracking()
                    join dm in db.DaDanhMucTienDoKyThanhToans.AsNoTracking()
                        on dc.MaKyTt equals dm.MaKyTt into jdm
                    from dm2 in jdm.DefaultIfEmpty()
                    where dc.MaCstt == maCstt
                          && (dc.IsCongNoDc ?? false) == true
                    // && dc.MaKyTt == "DC" // bật nếu chỉ muốn lấy kỳ "Đặt cọc"
                    orderby dc.DotTt
                    select new ChinhSachThanhToanChiTietModel
                    {
                        MaCSTT = dc.MaCstt ?? string.Empty,
                        DotTT = dc.DotTt ?? 0,
                        NoiDungTT = dc.NoiDungTt ?? string.Empty,

                        MaKyTT = dc.MaKyTt ?? string.Empty,
                        TenKyTT = dm2 != null ? (dm2.TenKyTt ?? string.Empty) : string.Empty,

                        SoKhoangCachNgay = dc.SoKhoangCachNgay ?? 0,
                        DotThamChieu = dc.DotThamChieu ?? 0,

                        TyLeTTDatCoc = dc.TyLeTtdatCoc ?? 0,
                        TyLeTTVAT = dc.TyLeTtdatCocVat ?? 0,

                        IsCongNoDC = dc.IsCongNoDc ?? false,
                        IsDotThamChieuInvalid = false
                    }
                ).ToListAsync(ct);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "GetCanTruDatCocAsync failed. MaCSTT={MaCSTT}",
                    maCstt);

                return new(); // UI-friendly
            }
        }

        #endregion
    }
}

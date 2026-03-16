using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.HopDongMuaBan;
using VTTGROUP.Domain.Model.KeHoachBanHang;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhuLucHopDongMuaBanService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<PhuLucHopDongMuaBanService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        public PhuLucHopDongMuaBanService(AppDbContext context, ILogger<PhuLucHopDongMuaBanService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
        }

        #region Danh sách số lượng phụ lục của hợp đồng
        public async Task<(List<PhucLucHopDongPaginDTO> Data, int TotalCount)> GetPagingAsync(
     string? maHopDong)
        {
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaHopDong", maHopDong);
            var result = (await connection.QueryAsync<PhucLucHopDongPaginDTO>(
                "Proc_PhuLucHopDong_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();
            return (result, result.Count);
        }
        #endregion

        #region Thêm, xóa, sửa phụ lục hợp đồng mua bán
        public async Task<ResultModel> SavePhuLucHopDongMuaBanAsync(PhucLucHopDongMode model, List<HopDongMuaBanTienDoThanhToanModel> listTDTT)
        {
            try
            {
                var record = new KdPhuLucHopDong
                {
                    MaPhuLuc = await SinhMaPhieuPLHDTuDongAsync("PLHDMB-", 5),
                    MaHopDong = model.HopDong.MaHopDong,
                    SoPhuLuc = model.SoPhuLuc,
                    MaCstt = model.MaChinhSachTT,
                    GiaBan = model.GiaBan,
                    GiaTriCk = model.GiaTriCK,
                    GiaBanSauCk = model.GiaBanSauCK,
                    GiaBanTruocThue = model.GiaBanTruocThue,
                    GiaBanSauThue = model.GiaBanSauThue,
                    NgayKyPl = !string.IsNullOrEmpty(model.NgayKy) ? DateTime.ParseExact(model.NgayKy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    NoiDung = model.GhiChu
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;
                await _context.KdPhuLucHopDongs.AddAsync(record);
                //Insert chính sách thanh toán
                if (listTDTT != null & listTDTT.Any() == true)
                {
                    List<KdPhuLucHopDongTienDoThanhToan> listCT = new List<KdPhuLucHopDongTienDoThanhToan>();
                    foreach (var item in listTDTT)
                    {
                        var r = new KdPhuLucHopDongTienDoThanhToan();
                        r.MaPhuLuc = record.MaPhuLuc;
                        r.MaCstt = item.MaCSTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.KyThanhToan = item.MaKyTT;
                      //  r.NgayTt = !string.IsNullOrEmpty(item.NgayThanhToan) ? DateTime.ParseExact(item.NgayThanhToan, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                        r.TyLeTt = (double?)item.TyLeThanhToan;
                       // r.SoTienTt = item.SoTienTruocThue;
                        r.DotTt = item.DotTT;
                        listCT.Add(r);
                    }
                    await _context.KdPhuLucHopDongTienDoThanhToans.AddRangeAsync(listCT);
                }
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaPhuLuc, "Thêm phụ lục hợp đồng mua bán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm phụ lục hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phụ lục hợp đồng mua bán: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(PhucLucHopDongMode model, List<HopDongMuaBanTienDoThanhToanModel> listTDTT)
        {
            try
            {
                var entity = await _context.KdPhuLucHopDongs.FirstOrDefaultAsync(d => d.MaPhuLuc.ToLower() == model.MaPhuLuc.ToLower());
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy phụ lục hợp đồng mua bán.");
                }
                entity.NgayKyPl = !string.IsNullOrEmpty(model.NgayKy) ? DateTime.ParseExact(model.NgayKy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                entity.SoPhuLuc = model.SoPhuLuc;
                entity.NoiDung = model.GhiChu;
                //Insert chính sách thanh toán
                var delTDTT = _context.KdPhuLucHopDongTienDoThanhToans.Where(d => d.MaPhuLuc == entity.MaPhuLuc);
                if (listTDTT != null & listTDTT.Any() == true)
                {
                    List<KdPhuLucHopDongTienDoThanhToan> listCT = new List<KdPhuLucHopDongTienDoThanhToan>();
                    foreach (var item in listTDTT)
                    {
                        var r = new KdPhuLucHopDongTienDoThanhToan();
                        r.MaPhuLuc = entity.MaPhuLuc;
                        r.MaCstt = item.MaCSTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.KyThanhToan = item.MaKyTT;
                       // r.NgayTt = !string.IsNullOrEmpty(item.NgayThanhToan) ? DateTime.ParseExact(item.NgayThanhToan, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                        r.TyLeTt = (double?)item.TyLeThanhToan;
                      //  r.SoTienTt = item.SoTienTruocThue;
                        // r.TyLeVat = item.TyLeVAT;
                        r.DotTt = item.DotTT;
                        listCT.Add(r);
                    }
                    await _context.KdPhuLucHopDongTienDoThanhToans.AddRangeAsync(listCT);
                }
                _context.KdPhuLucHopDongTienDoThanhToans.RemoveRange(delTDTT);
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaPhuLuc, "Cập nhật phụ lục hợp đồng mua bán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phụ lục hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật phụ lục hợp đồng mua bán: {ex.Message.ToString()}");
            }
        }
        public async Task<ResultModel> DeletePLHDMBAsync(string maPhieu)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var plhdmb = await _context.KdPhuLucHopDongs.Where(d => d.MaPhuLuc == maPhieu).FirstOrDefaultAsync();
                if (plhdmb == null)
                {
                    return ResultModel.Fail("Không tìm thấy phụ lục hợp đồng mua bán");
                }
                var delPTTT = _context.KdPhuLucHopDongTienDoThanhToans.Where(d => d.MaPhuLuc == maPhieu);
                _context.KdPhuLucHopDongs.Remove(plhdmb);
                _context.KdPhuLucHopDongTienDoThanhToans.RemoveRange(delPTTT);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {plhdmb.MaPhuLuc} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePGCAsync] Lỗi khi xóa phụ lục hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin phụ lục hợp đồng mua bán
        public async Task<ResultModel> GetByIdAsync(string id, string maHopDong)
        {
            try
            {
                var entity = new PhucLucHopDongMode();
                if (string.IsNullOrEmpty(id))
                {
                    entity = new PhucLucHopDongMode();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaPhuLuc = await SinhMaPhieuPLHDTuDongAsync("PLHDMB-", 5);
                    var thongTinHD = await ThongTinChungHDMB(maHopDong, string.Empty, string.Empty, string.Empty);
                    if (thongTinHD == null)
                    {
                        thongTinHD = new HopDongMuaBanModel();
                    }
                    entity.HopDong = thongTinHD;
                    entity.MaChinhSachTT = thongTinHD.MaChinhSachTT;
                }
                else
                {
                    entity = await (
                   from pl in _context.KdPhuLucHopDongs
                   join cstt in _context.BhChinhSachThanhToans on pl.MaCstt equals cstt.MaCstt into dtDong
                   from cstt2 in dtDong.DefaultIfEmpty()
                   where pl.MaPhuLuc == id
                   select new PhucLucHopDongMode
                   {
                       MaPhuLuc = pl.MaPhuLuc,
                       SoPhuLuc = pl.SoPhuLuc ?? string.Empty,
                       MaHopDong = pl.MaHopDong ?? string.Empty,
                       NgayLap = pl.NgayLap ?? DateTime.Now,
                       NoiDung = pl.NoiDung ?? string.Empty,
                       NgayKy = string.Format("{0:dd/MM/yyyy}", pl.NgayKyPl),
                       MaChinhSachTT = pl.MaCstt ?? string.Empty,
                       TenChinhSachTT = cstt2.TenCstt,
                       GiaBan = pl.GiaBan ?? 0,
                       GiaTriCK = pl.GiaTriCk ?? 0,
                       GiaBanSauCK = pl.GiaBanSauCk ?? 0,
                       GiaBanTruocThue = pl.GiaBanTruocThue ?? 0,
                       GiaBanSauThue = pl.GiaBanSauThue ?? 0,
                       MaQuiTrinhDuyet = pl.MaQuiTrinhDuyet ?? string.Empty,
                       TrangThaiDuyet = pl.TrangThaiDuyet ?? string.Empty,
                       MaNhanVien = pl.NguoiLap ?? string.Empty,
                       GhiChu = pl.NoiDung ?? string.Empty
                   }).FirstOrDefaultAsync();
                    if (entity != null)
                    {
                        entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                        var thongTinHD = await ThongTinChungHDMB(entity.MaHopDong, string.Empty, string.Empty, string.Empty);
                        if (thongTinHD == null)
                        {
                            thongTinHD = new HopDongMuaBanModel();
                        }
                        entity.HopDong = thongTinHD;
                    }
                    else
                    {
                        entity = new PhucLucHopDongMode();
                        entity.HopDong = new HopDongMuaBanModel();
                    }
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<HopDongMuaBanModel> ThongTinChungHDMB(string maHopDong, string maPhieuDatCoc, string maDuAn, string maChinhSacTT)
        {
            var entity = new HopDongMuaBanModel();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaHopDong", maHopDong);
                param.Add("@MaPhieuDatCoc", maPhieuDatCoc);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaChinhSacTT", maChinhSacTT);
                entity = (await connection.QueryAsync<HopDongMuaBanModel>(
                "Proc_HopDongMuaBan_ThongTinChung",
                param,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            catch
            {
                entity = new HopDongMuaBanModel();
            }
            return entity;
        }

        public async Task<List<HopDongMuaBanTienDoThanhToanModel>> GetByTienDoThanhToanAsync(string maCSTT, string maPhuLuc)
        {
            var entity = new List<HopDongMuaBanTienDoThanhToanModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaChinhSachTT", maCSTT);
                param.Add("@MaPhuLuc", maPhuLuc);

                entity = (await connection.QueryAsync<HopDongMuaBanTienDoThanhToanModel>(
                    "Proc_PhuLucHopDongMuaBan_TienDoThanhToan",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<HopDongMuaBanTienDoThanhToanModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách tiến độ thanh toán phụ lục hợp đồng mua bán");
            }
            return entity;
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu giữ chỗ   
        public async Task<string> SinhMaPhieuPLHDTuDongAsync(string prefix, int padding = 5)
        {
            _context.ChangeTracker.Clear();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KdPhuLucHopDongs
                .Where(kh => kh.MaPhuLuc.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaPhuLuc)
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

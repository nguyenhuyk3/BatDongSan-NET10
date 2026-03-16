using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.CanHo;
using VTTGROUP.Domain.Model.KeHoachBanHang;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class KeHoachBanHangService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<KeHoachBanHangService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        private decimal DoanhThuDuKien;
        public KeHoachBanHangService(IDbContextFactory<AppDbContext> factory, ILogger<KeHoachBanHangService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách kế hoạch bán hàng
        public async Task<(List<KeHoachBanHangPaginDto> Data, int TotalCount)> GetPagingAsync(
         string? maDuAn, int page, int pageSize, string? qSearch, string fromDate, string toDate, string trangThaiDuyet)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            param.Add("@TrangThai", trangThaiDuyet);
            param.Add("@NgayLapFrom", fromDate);
            param.Add("@NgayLapTo", toDate);

            var result = (await connection.QueryAsync<KeHoachBanHangPaginDto>(
                "Proc_KeHoachBanHang_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa kế hoạch bán hàng
        public async Task<ResultModel> SaveKeHoachBanHangAsync(KeHoachBanHangModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                DoanhThuDuKien = 0;
                var record = new BhKeHoachBanHang
                {
                    MaPhieuKh = await SinhMaKHTuDongAsync("KHBH-", _context, 5),
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    NgayLap = DateTime.Now,
                    NoiDung = model.NoiDung,
                    DoanhThuDuKien = 0
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.DonGiaTb = await _context.DaDanhMucDuAnCauHinhChungs.Where(d => d.MaDuAn == record.MaDuAn).Select(d => d.DonGiaTb).FirstOrDefaultAsync() ?? 0;
                await _context.BhKeHoachBanHangs.AddAsync(record);
                //Insert đợt mở bán của dự án đó
                var listDMBDA = await _context.DaDanhMucDotMoBans.Where(d => d.MaDuAn == model.MaDuAn).ToListAsync();
                List<BhKeHoachBanHangDotMoBan> listKHBHDMB = new List<BhKeHoachBanHangDotMoBan>();
                BhKeHoachBanHangDotMoBan ct;
                foreach (var item in listDMBDA)
                {
                    ct = new BhKeHoachBanHangDotMoBan();
                    ct.MaPhieuKh = record.MaPhieuKh;
                    ct.MaDotMoBan = item.MaDotMoBan;
                    listKHBHDMB.Add(ct);
                }
                await _context.BhKeHoachBanHangDotMoBans.AddRangeAsync(listKHBHDMB);
                //Insert danh sách căn hộ theo đợt kế hoạch gần nhất
                await InsertKeHoachBanHangCanHo(record.MaDuAn, record.MaPhieuKh, _context);
                await InsertKeHoachGiaBanTheoDot(record.MaDuAn, record.MaPhieuKh, _context);
                record.DoanhThuDuKien = DoanhThuDuKien * record.DonGiaTb;
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaPhieuKh, "Thêm kế hoạch bán hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm kế hoạch bán hàng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm kế hoạch bán hàng: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateKHBHAsync(KeHoachBanHangModel model, List<GiaBanTheoDotDto> danhSachGia)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhKeHoachBanHangs.FirstOrDefaultAsync(d => d.MaPhieuKh.ToLower() == model.MaPhieuKH.ToLower());
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy kế hoạch bán hàng.");
                }
                entity.DoanhThuDuKien = model.DoanhThuDuKien;
                entity.NoiDung = model.NoiDung;
                //Insert đợt mở bán giá bán
                var delDMBGB = _context.BhKeHoachBanHangDotMoBanGiaBans.Where(d => d.MaPhieuKh == model.MaPhieuKH).ToList();
                _context.BhKeHoachBanHangDotMoBanGiaBans.RemoveRange(delDMBGB);
                List<BhKeHoachBanHangDotMoBanGiaBan> listGB = new List<BhKeHoachBanHangDotMoBanGiaBan>();
                BhKeHoachBanHangDotMoBanGiaBan ct;
                foreach (var item in danhSachGia)
                {
                    ct = new BhKeHoachBanHangDotMoBanGiaBan();
                    ct.MaPhieuKh = entity.MaPhieuKh;
                    ct.MaDotMoBan = item.MaDot;
                    ct.DonGiaTbdot = item.DonGia;
                    ct.IsXacNhan = item.IsXacNhan;
                    listGB.Add(ct);
                }
                _context.BhKeHoachBanHangDotMoBanGiaBans.AddRange(listGB);
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaPhieuKh, "Cập nhật kế hoạch bán hàng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm kế hoạch bán hàng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm kế hoạch bán hàng: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteViewAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var khbh = await _context.BhKeHoachBanHangs.Where(d => d.MaPhieuKh == maPhieu).FirstOrDefaultAsync();
                if (khbh == null)
                {
                    return ResultModel.Fail("Không tìm thấy kế hoạch bán hàng");
                }
                var delDMB = await _context.BhKeHoachBanHangDotMoBans.Where(d => d.MaPhieuKh == maPhieu).ToListAsync();
                var delDMBCH = await _context.BhKeHoachBanHangDotMoBanCanHos.Where(d => d.MaPhieuKh == maPhieu).ToListAsync();
                var delGB = await _context.BhKeHoachBanHangDotMoBanGiaBans.Where(d => d.MaPhieuKh == maPhieu).ToListAsync();
                var delND = await _context.HtDmnguoiDuyets.Where(d => d.MaPhieu == maPhieu).ToListAsync();
                _context.BhKeHoachBanHangs.Remove(khbh);
                _context.BhKeHoachBanHangDotMoBans.RemoveRange(delDMB);
                _context.BhKeHoachBanHangDotMoBanCanHos.RemoveRange(delDMBCH);
                _context.BhKeHoachBanHangDotMoBanGiaBans.RemoveRange(delGB);
                _context.HtDmnguoiDuyets.RemoveRange(delND);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {khbh.MaPhieuKh} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteCongViecAsync] Lỗi khi xóa View mặt khối");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<KeHoachBanHangPaginDto> listKeHoach)
        {
            try
            {
                var ids = listKeHoach?
            .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieuKH))
            .Select(x => x.MaPhieuKH!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                var targetIds = ids;
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.BhKeHoachBanHangDotMoBanCanHos
                .Where(d => targetIds.Contains(d.MaPhieuKh))
                .ExecuteDeleteAsync();

                var c2 = await _context.BhKeHoachBanHangDotMoBanGiaBans
                    .Where(d => targetIds.Contains(d.MaPhieuKh))
                    .ExecuteDeleteAsync();

                var c3 = await _context.BhKeHoachBanHangDotMoBans
                    .Where(d => targetIds.Contains(d.MaPhieuKh))
                    .ExecuteDeleteAsync();

                var c4 = await _context.HtDmnguoiDuyets
                    .Where(d => targetIds.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var cParent = await _context.BhKeHoachBanHangs
                .Where(k => targetIds.Contains(k.MaPhieuKh))
                .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success($"Xóa danh sách kế hoạch bán hàng thành công");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách kế hoạch");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task RemoveCanHoTrongDot(string maPhieuKH, string maDotMoBan, string maCanHo)
        {
            using var _context = _factory.CreateDbContext();
            var entity = await _context.BhKeHoachBanHangDotMoBanCanHos
                .FirstOrDefaultAsync(x =>
                    x.MaPhieuKh == maPhieuKH &&
                    x.MaDotMoBan == maDotMoBan &&
                    x.MaCanHo == maCanHo);

            if (entity != null)
            {
                _context.BhKeHoachBanHangDotMoBanCanHos.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveAllCanHoTrongDot(string maPhieuKH, string maDot)
        {
            using var _context = _factory.CreateDbContext();
            var entity = await _context.BhKeHoachBanHangDotMoBanCanHos.Where(d => d.MaPhieuKh == maPhieuKH && d.MaDotMoBan == maDot).ToListAsync();
            if (entity != null)
            {
                _context.BhKeHoachBanHangDotMoBanCanHos.RemoveRange(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task InsertKeHoachBanHangCanHo(string maDuAn, string maKeHoach, AppDbContext context)
        {
            var entity = new List<KeHoachBanHangChiTietCanHo>();
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaPhieu", string.Empty);
            param.Add("@MaDuAn", maDuAn);
            entity = (await connection.QueryAsync<KeHoachBanHangChiTietCanHo>("Proc_KeHoachBanHang_ChiTietCanHo", param, commandType: CommandType.StoredProcedure)).ToList();
            if (entity.Any())
            {
                List<BhKeHoachBanHangDotMoBanCanHo> listDMBCH = new List<BhKeHoachBanHangDotMoBanCanHo>();
                BhKeHoachBanHangDotMoBanCanHo ct;
                foreach (var item in entity)
                {
                    ct = new BhKeHoachBanHangDotMoBanCanHo();
                    ct.MaPhieuKh = maKeHoach;
                    ct.MaDotMoBan = item.MaDotMoBan;
                    ct.MaCanHo = item.MaSanPham;
                    ct.HeSoCanHo = item.HeSoCanHo;
                    ct.DienTichCanHo = item.DienTichCanHo;
                    listDMBCH.Add(ct);
                }
                await context.BhKeHoachBanHangDotMoBanCanHos.AddRangeAsync(listDMBCH);
            }
        }

        public async Task InsertKeHoachGiaBanTheoDot(string maDuAn, string maKeHoach, AppDbContext context)
        {
            var entity = new List<GiaBanTheoDotDto>();
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaDuAn", maDuAn);
            param.Add("@MaPhieuKH", string.Empty);
            param.Add("@isTinh", false);
            entity = (await connection.QueryAsync<GiaBanTheoDotDto>("Proc_KeHoachMuaHang_GiaBanTheoDot", param, commandType: CommandType.StoredProcedure)).ToList();
            if (entity.Any())
            {
                DoanhThuDuKien = entity.Sum(d => d.TongDienTichCanHo);
                List<BhKeHoachBanHangDotMoBanGiaBan> listGBTD = new List<BhKeHoachBanHangDotMoBanGiaBan>();
                BhKeHoachBanHangDotMoBanGiaBan ct;
                foreach (var item in entity)
                {
                    ct = new BhKeHoachBanHangDotMoBanGiaBan();
                    ct.MaPhieuKh = maKeHoach;
                    ct.MaDotMoBan = item.MaDot;
                    ct.DonGiaTbdot = item.DonGia;
                    ct.IsXacNhan = item.IsXacNhan;
                    listGBTD.Add(ct);
                }
                await context.BhKeHoachBanHangDotMoBanGiaBans.AddRangeAsync(listGBTD);
            }
        }
        #endregion

        #region Thông tin kế hoạch bán hàng
        public async Task<ResultModel> FindGetByKeHoachBHAsync(string? id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new KeHoachBanHangModel();
                if (!string.IsNullOrEmpty(id))
                {
                    record = await (
                     from khbh in _context.BhKeHoachBanHangs
                     join da in _context.DaDanhMucDuAns on khbh.MaDuAn equals da.MaDuAn into dtKHBH
                     from da2 in dtKHBH.DefaultIfEmpty()
                     where khbh.MaPhieuKh == id
                     select new KeHoachBanHangModel
                     {
                         MaPhieuKH = khbh.MaPhieuKh,
                         NgayLap = khbh.NgayLap ?? DateTime.Now,
                         MaDuAn = khbh.MaDuAn ?? string.Empty,
                         TenDuAn = da2.TenDuAn,
                         NoiDung = khbh.NoiDung ?? string.Empty,
                         DonGiaTB = khbh.DonGiaTb ?? 0,
                         DoanhThuDuKien = khbh.DoanhThuDuKien ?? 0,
                         MaQuiTrinhDuyet = khbh.MaQuiTrinhDuyet ?? 0,
                         TrangThaiDuyet = khbh.TrangThaiDuyet ?? 0,
                         MaNhanVien = khbh.NguoiLap
                     }).FirstOrDefaultAsync();
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("KHBH", record.MaPhieuKH);
                    record.MaNhanVienDP = ttnd == null ? string.Empty : ttnd.MaNhanVien;
                    record.TrangThaiDuyetCuoi = await _baseService.BuocDuyetCuoi(record.MaQuiTrinhDuyet);
                    if (record.TrangThaiDuyet == 0 && record.NguoiLap != null && record.NguoiLap.MaNhanVien == _currentUser.MaNhanVien)
                    {
                        record.FlagTong = true;
                    }
                    else if (record.MaNhanVienDP == _currentUser.MaNhanVien && record.TrangThaiDuyet != record.TrangThaiDuyetCuoi)
                    {
                        record.FlagTong = true;
                    }
                    record.SaiSoDoanhThuChoPhepKHBH = await _context.DaDanhMucDuAnCauHinhChungs.Select(d => d.SaiSoDoanhThuChoPhepKhbh).FirstOrDefaultAsync();
                    record.TongSoLuongCanHo = await _context.DaDanhMucSanPhams.Where(d => d.MaDuAn == record.MaDuAn).CountAsync();
                }
                else
                {
                    record.MaPhieuKH = await SinhMaKHTuDongAsync("KHBH-", _context, 5);
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    record.NgayLap = DateTime.Now;
                    record.FlagTong = false;
                }
                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (Exception ex)
            {
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin kế hoạch bán hàng: {ex.Message}");
            }
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu kế hoach     
        public async Task<string> SinhMaKHTuDongAsync(string prefix, AppDbContext context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await context.BhKeHoachBanHangs
                .Where(kh => kh.MaPhieuKh.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaPhieuKh)
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

        #region Lấy danh sách các đợt theo dự án
        public async Task<List<DotMoBanCanHoTheoDuAn>> GetDotMoBanTheoDuAnAsync(string maPhieuKH)
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<DotMoBanCanHoTheoDuAn>();
            try
            {
                entity = await (
                     from khbh in _context.BhKeHoachBanHangDotMoBans
                     join da in _context.DaDanhMucDotMoBans on khbh.MaDotMoBan equals da.MaDotMoBan into dtKHBH
                     from da2 in dtKHBH.DefaultIfEmpty()
                     where khbh.MaPhieuKh == maPhieuKH
                     select new DotMoBanCanHoTheoDuAn
                     {
                         MaPhieuKeHoach = khbh.MaPhieuKh,
                         MaDot = da2.MaDotMoBan,
                         TenDot = da2.TenDotMoBan,
                         ThuTuHienThi = da2.ThuTuHienThi ?? 1,
                         MaMau = da2.MaMau ?? string.Empty
                     }).OrderBy(d => d.ThuTuHienThi).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DotMoBanCanHoTheoDuAn>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách đợt mở bán theo dự án");
            }
            return entity;
        }
        #endregion

        #region Lấy danh sách chi tiết căn hộ theo các đợt
        public async Task<List<KeHoachBanHangChiTietCanHo>> GeChiTietCanHoAsync(string? maPhieuKH)
        {
            var entity = new List<KeHoachBanHangChiTietCanHo>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaPhieu", maPhieuKH);
                param.Add("@MaDuAn", string.Empty);
                entity = (await connection.QueryAsync<KeHoachBanHangChiTietCanHo>("Proc_KeHoachBanHang_ChiTietCanHo", param, commandType: CommandType.StoredProcedure)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách đợt mở bán theo dự án");
            }
            return entity;
        }

        public async Task<List<GiaBanTheoDotDto>> GeChiTietGiaBanTheoDotAsync(string? maPhieuKH, string maDuAn, bool? isTinh)
        {
            var entity = new List<GiaBanTheoDotDto>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaPhieuKH", maPhieuKH);
                param.Add("@isTinh", isTinh ?? false);
                entity = (await connection.QueryAsync<GiaBanTheoDotDto>("Proc_KeHoachMuaHang_GiaBanTheoDot", param, commandType: CommandType.StoredProcedure)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách giá bán theo đợt căn hộ");
            }
            return entity;
        }
        #endregion

        #region Load danh sách Popup căn hộ theo dự án
        public async Task<(List<SanPhamPopupPaginModel> Data, int TotalCount)> GetPagingPopupAsync(
      string? maDuAn, string maBlock, string maTang, string maTruc)
        {
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@MaBlock", !string.IsNullOrEmpty(maBlock) ? maBlock : null);
            param.Add("@MaTang", !string.IsNullOrEmpty(maTang) ? maTang : null);
            param.Add("@MaTruc", !string.IsNullOrEmpty(maTruc) ? maTruc : null);

            var result = (await connection.QueryAsync<SanPhamPopupPaginModel>(
                "Proc_KeHoachMuaHang_SanPhamPopUp_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;
            return (result, total);
        }

        public async Task<List<SoDoCanHoModel>> GetSoDoCanHoAsync(string maDuAn, string maBlock, string maTang, string maTruc, string maKeHoach)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
                parameters.Add("@MaBlock", !string.IsNullOrEmpty(maBlock) ? maBlock : null);
                parameters.Add("@MaTang", !string.IsNullOrEmpty(maTang) ? maTang : null);
                parameters.Add("@MaTruc", !string.IsNullOrEmpty(maTruc) ? maTruc : null);
                parameters.Add("@MaPhieuKH", !string.IsNullOrEmpty(maKeHoach) ? maKeHoach : null);

                var result = await connection.QueryAsync<SoDoCanHoModel>(
                    "Proc_KeHoachMuaHang_SanPhamPopUp_GetPaging",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi GetSoDoCanHoAsync với maDuAn={maDuAn}, maBlock={maBlock}", maDuAn, maBlock);
                return new List<SoDoCanHoModel>();
            }
        }
        #endregion

        #region Insert căn hộ theo đợt mở bán của kế hoạch
        public async Task<ResultModel> ThemCanHoVaoKeHoach(string maPhieuKH, string maDotMoBan, List<SanPhamPopupPaginModel> danhSachMaCanHo, bool isList = true)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var maCanHoList = danhSachMaCanHo
                .Select(x => x.MaSanPham?.ToLowerInvariant())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

                var existingItems = await _context.BhKeHoachBanHangDotMoBanCanHos
                .Where(d => d.MaPhieuKh == maPhieuKH && d.MaDotMoBan == maDotMoBan && maCanHoList.Contains(d.MaCanHo.ToLower()))
                .ToListAsync();

                var existingMap = existingItems.ToDictionary(x => x.MaCanHo.ToLowerInvariant());
                var canHoThemMoi = new List<BhKeHoachBanHangDotMoBanCanHo>();
                var canHoXoa = new List<BhKeHoachBanHangDotMoBanCanHo>();


                foreach (var item in danhSachMaCanHo)
                {
                    var maCanHo = item.MaSanPham?.ToLowerInvariant();
                    if (string.IsNullOrEmpty(maCanHo)) continue;

                    if (!existingMap.ContainsKey(maCanHo))
                    {
                        canHoThemMoi.Add(new BhKeHoachBanHangDotMoBanCanHo
                        {
                            MaPhieuKh = maPhieuKH,
                            MaDotMoBan = maDotMoBan,
                            MaCanHo = item.MaSanPham,
                            HeSoCanHo = Convert.ToDecimal(item.HeSoTuTinh),
                            DienTichCanHo = Convert.ToDecimal(item.DienTichTimTuong)
                        });
                    }
                    else if (!isList)
                    {
                        canHoXoa.Add(existingMap[maCanHo]);
                    }
                }

                if (canHoThemMoi.Any())
                    await _context.BhKeHoachBanHangDotMoBanCanHos.AddRangeAsync(canHoThemMoi);

                if (canHoXoa.Any())
                    _context.BhKeHoachBanHangDotMoBanCanHos.RemoveRange(canHoXoa);

                await _context.SaveChangesAsync();

                await GetDoanhThuDuKien(maPhieuKH);

                return new ResultModel { Status = true, Message = "Đã lưu thành công." };
            }
            catch (Exception ex)
            {
                // Có thể log lỗi tại đây nếu muốn
                return new ResultModel
                {
                    Status = false,
                    Message = "Lỗi khi lưu căn hộ: " + ex.Message.ToString()
                };
            }
        }
        public async Task<ResultModel> RemoveCanHoCuaKeHoach(string maPhieuKH, string maDotMoBan, List<SanPhamPopupPaginModel> danhSachMaCanHo)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var maCanHoList = danhSachMaCanHo
                    .Select(x => x.MaSanPham?.ToLowerInvariant())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();

                var canHoCanXoa = await _context.BhKeHoachBanHangDotMoBanCanHos
                    .Where(d => d.MaPhieuKh == maPhieuKH &&
                                d.MaDotMoBan == maDotMoBan &&
                                maCanHoList.Contains(d.MaCanHo.ToLower()))
                    .ToListAsync();

                if (canHoCanXoa.Any())
                {
                    _context.BhKeHoachBanHangDotMoBanCanHos.RemoveRange(canHoCanXoa);
                    await _context.SaveChangesAsync();
                }

                await GetDoanhThuDuKien(maPhieuKH);

                return new ResultModel { Status = true, Message = "Đã xóa thành công." };
            }
            catch (Exception ex)
            {
                return new ResultModel
                {
                    Status = false,
                    Message = "Lỗi khi xóa căn hộ: " + ex.Message
                };
            }
        }
        #endregion

        #region Hàm tính doanh thu dự kiến căn hộ khi chọn sản phẩm trên popup
        public async Task GetDoanhThuDuKien(string? maPhieuKH)
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<GiaBanTheoDotDto>();
            try
            {
                var phieuKH = await _context.BhKeHoachBanHangs.FirstOrDefaultAsync(d => d.MaPhieuKh == maPhieuKH);
                if (phieuKH == null)
                    return;

                var sumDienTich = await _context.BhKeHoachBanHangDotMoBanCanHos
                .Where(d => d.MaPhieuKh == maPhieuKH)
                .SumAsync(d => (decimal?)d.DienTichCanHo) ?? 0;

                phieuKH.DoanhThuDuKien = Math.Round((phieuKH.DonGiaTb ?? 0) * sumDienTich, 0);
                await _context.SaveChangesAsync();
                //using var connection = new SqlConnection(_connectionString);
                //var param = new DynamicParameters();
                //param.Add("@MaPhieuKH", maPhieuKH);
                //param.Add("@isTinh", false);
                //entity = (await connection.QueryAsync<GiaBanTheoDotDto>("Proc_KeHoachMuaHang_GiaBanTheoDot", param, commandType: CommandType.StoredProcedure)).ToList();
                //if (entity != null && entity.Count > 0)
                //{
                //    var donGiaTb = await _context.BhKeHoachBanHangs.Where(d => d.MaPhieuKh == maPhieuKH).FirstOrDefaultAsync();
                //    if (donGiaTb != null)
                //    {
                //        donGiaTb.DoanhThuDuKien = Math.Round((donGiaTb?.DonGiaTb ?? 0) * entity.Sum(dot => dot.TongDienTichCanHo), 0);
                //        _context.SaveChanges();
                //    }
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lưu doanh thu dự kiến");
            }
        }
        #endregion

        #region Cập nhật đơn giá trung bình
        public async Task<ResultModel> CapNhatDonGiaTrungBinh(string maKeHoach, string maDuAn)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maKeHoach))
                    return ResultModel.Fail("Thiếu mã kế hoạch.");

                if (string.IsNullOrWhiteSpace(maDuAn))
                    return ResultModel.Fail("Thiếu mã công trình.");

                await using var context = _factory.CreateDbContext();

                // Lấy entity trước cho chắc
                var entity = await context.BhKeHoachBanHangs
                    .FirstOrDefaultAsync(d => d.MaPhieuKh == maKeHoach
                                           && d.MaDuAn == maDuAn);

                if (entity == null)
                    return ResultModel.Fail($"Không tìm thấy phiếu kế hoạch bán hàng: {maKeHoach}");

                // Transaction để tránh race-condition khi nhiều user cùng cập nhật
                await using var tx = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

                decimal donGiaTB = await context.DaDanhMucDuAnCauHinhChungs.Where(d => d.MaDuAn == entity.MaDuAn).Select(d => d.DonGiaTb).FirstOrDefaultAsync() ?? 0;
                entity.DonGiaTb = donGiaTB;
                await context.SaveChangesAsync();
                await tx.CommitAsync();

                return ResultModel.SuccessWithGiaTri(donGiaTB, "Cập nhật đơn giá trung bình của kế hoạch thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CapNhatDonGiaTrungBinh] Lỗi khi cập nhật đơn giá trung bình");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> LayDonGiaTrungBinh(string maDuAn)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDuAn))
                    return ResultModel.Fail("Thiếu mã công trình.");

                await using var context = _factory.CreateDbContext();
                decimal donGiaTB = await context.DaDanhMucDuAnCauHinhChungs.Where(d => d.MaDuAn == maDuAn).Select(d => d.DonGiaTb).FirstOrDefaultAsync() ?? 0;

                return ResultModel.SuccessWithGiaTri(donGiaTB, "Lấy đơn giá trung bình thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CapNhatDonGiaTrungBinh] Lỗi khi lấy giá trung bình");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion
    }
}

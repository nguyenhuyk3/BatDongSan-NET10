using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.CanHo;
using VTTGROUP.Domain.Model.Hubs;
using VTTGROUP.Domain.Model.PhieuDangKy;
using VTTGROUP.Domain.Model.PhieuGiuCho;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuDangKyService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        //private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<PhieuDangKyService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationSender _notifier;
        private readonly IBaseService _baseService;
        private readonly NavigationManager _nav;
        public PhieuDangKyService(IDbContextFactory<AppDbContext> factory, ILogger<PhieuDangKyService> logger, IConfiguration config, ICurrentUserService currentUser, INotificationSender notifier, IBaseService baseService, NavigationManager nav)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _notifier = notifier;
            _baseService = baseService;
            _nav = nav;
        }

        #region Hiển thị danh sách phiếu đăng ký
        public async Task<(List<PhieuDangKyPagingDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, string? maSanGG, int page, int pageSize, string? qSearch, string? trangThai, bool? trangThaiXN, string fromDate, string toDate)
        {
            try
            {
                qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaSanGD", maSanGG);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                param.Add("@TrangThai", trangThai);
                param.Add("@NgayLapFrom", fromDate);
                param.Add("@NgayLapTo", toDate);
                param.Add("@IsXacNhan", trangThaiXN);

                var result = (await connection.QueryAsync<PhieuDangKyPagingDto>(
                    "Proc_PhieuDangKy_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;
                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sanh sách phiếu giữ chỗ sàn giao dịch");
                var result = new List<PhieuDangKyPagingDto>();
                return (result, 0);
            }
        }
        #endregion

        #region Thông tin phiếu đăng ký

        //public async Task<ResultModel> GetByGioHangIdAsync(string id, string maCanHo, SoDoCanHoModel soDo)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        if (await _baseService.HienTrangKinhDoanhSanPham(soDo.MaDuAn,maCanHo) == "DaBan")
        //        {
        //            _nav.NavigateTo("/sanpham-dadangky");
        //        }
        //        var entity = await (
        //              from pgc in _context.BhGioHangs

        //              join da in _context.DaDanhMucDuAns on pgc.MaDuAn equals da.MaDuAn into dtDuAn
        //              from da2 in dtDuAn.DefaultIfEmpty()

        //              join sgg in _context.DmSanGiaoDiches on pgc.MaSanGiaoDich equals sgg.MaSanGiaoDich into dtSGG
        //              from sgg2 in dtSGG.DefaultIfEmpty()

        //              join dmb in _context.DaDanhMucDotMoBans on pgc.MaDotMoBan equals dmb.MaDotMoBan into dtDMB
        //              from dmb2 in dtDMB.DefaultIfEmpty()

        //              where pgc.MaPhieu == id
        //              select new PhieuDangKyModel
        //              {
        //                  MaGioHang = pgc.MaPhieu,
        //                  LoaiGioHang = pgc.LoaiGioHang ?? false,
        //                  NgayLap = DateTime.Now,
        //                  MaDuAn = pgc.MaDuAn ?? string.Empty,
        //                  TenDuAn = da2.TenDuAn,
        //                  DotMoBan = pgc.MaDotMoBan ?? string.Empty,
        //                  TenDotMoBan = dmb2.TenDotMoBan,
        //                  GiaBanTheoCSTT = 0,
        //                  SanGiaoDich = pgc.MaSanGiaoDich ?? string.Empty,
        //                  TenSanGiaoDich = sgg2.TenSanGiaoDich ?? string.Empty
        //              }).FirstOrDefaultAsync();
        //        entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
        //        entity.MaPhieu = await SinhMaPhieuTuDongAsync("PDK-", 5);
        //        entity.MaLoaiThietKe = await (from sp in _context.DaDanhMucSanPhams
        //                                      join ltk in _context.DaDanhMucLoaiCanHos on sp.MaLoaiCan equals ltk.MaLoaiCanHo into dtLTK
        //                                      from ltk2 in dtLTK.DefaultIfEmpty()
        //                                      where sp.MaSanPham == maCanHo
        //                                      select new PhieuDangKyModel
        //                                      {
        //                                          MaLoaiThietKe = ltk2.MaLoaiThietKe
        //                                      }).Select(d => d.MaLoaiThietKe).FirstOrDefaultAsync() ?? string.Empty;
        //        entity.ListPDKCSBH = await ListChiTietBanHangPDKAsync(string.Empty, entity.MaDuAn, entity.DotMoBan, soDo.GiaBanTruocThue ?? 0, soDo.GiaBanChinhThucTruocThue ?? 0, soDo.PhuongThucTinhCk);
        //        return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu đăng ký");
        //        return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> GetByGioHangIdAsync(
    string id,
    string maCanHo,
    SoDoCanHoModel soDo,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ResultModel.Fail("Mã giỏ hàng không hợp lệ.");

            if (string.IsNullOrWhiteSpace(maCanHo))
                return ResultModel.Fail("Mã căn hộ không hợp lệ.");

            if (soDo == null)
                return ResultModel.Fail("Dữ liệu sơ đồ căn hộ không hợp lệ.");

            try
            {
                await using var context = _factory.CreateDbContext();

                // 1) Chặn nghiệp vụ: đã bán thì không cho tạo phiếu từ giỏ
                var trangThai = await _baseService.CanShowDangKyAsync(soDo.MaDuAn ?? string.Empty, maCanHo, id);                
                if (!trangThai.Status)
                {
                    _nav.NavigateTo("/sanpham-dadangky");
                    return ResultModel.Fail(trangThai.Message);
                }
                // (Nếu method này có overload nhận ct thì truyền ct vào luôn)
                //var hienTrang = await _baseService.HienTrangKinhDoanhSanPham(soDo.MaDuAn, maCanHo);
                //if (string.Equals(hienTrang, "DaBan", StringComparison.OrdinalIgnoreCase))
                //{
                //    _nav.NavigateTo("/sanpham-dadangky");
                //    return ResultModel.Fail("Sản phẩm đã bán, không thể thao tác.");
                //}


                // 2) Lấy thông tin giỏ hàng + dự án + sàn + đợt
                var entity = await (
                    from gh in context.BhGioHangs.AsNoTracking()
                    where gh.MaPhieu == id

                    join da in context.DaDanhMucDuAns.AsNoTracking()
                        on gh.MaDuAn equals da.MaDuAn into daGroup
                    from da2 in daGroup.DefaultIfEmpty()

                    join sgg in context.DmSanGiaoDiches.AsNoTracking()
                        on gh.MaSanGiaoDich equals sgg.MaSanGiaoDich into sggGroup
                    from sgg2 in sggGroup.DefaultIfEmpty()

                    join dmb in context.DaDanhMucDotMoBans.AsNoTracking()
                        on gh.MaDotMoBan equals dmb.MaDotMoBan into dmbGroup
                    from dmb2 in dmbGroup.DefaultIfEmpty()

                    select new PhieuDangKyModel
                    {
                        MaGioHang = gh.MaPhieu,
                        LoaiGioHang = gh.LoaiGioHang ?? false,
                        NgayLap = DateTime.Now,

                        MaDuAn = gh.MaDuAn ?? string.Empty,
                        TenDuAn = da2 != null ? da2.TenDuAn : null,

                        DotMoBan = gh.MaDotMoBan ?? string.Empty,
                        TenDotMoBan = dmb2 != null ? dmb2.TenDotMoBan : null,

                        GiaBanTheoCSTT = 0,

                        SanGiaoDich = gh.MaSanGiaoDich ?? string.Empty,
                        TenSanGiaoDich = sgg2 != null ? (sgg2.TenSanGiaoDich ?? string.Empty) : string.Empty
                    }
                ).FirstOrDefaultAsync(ct);

                if (entity is null)
                    return ResultModel.Fail("Không tìm thấy giỏ hàng.");

                // 3) Enrich: người lập + sinh mã phiếu
                // (Nếu method này có overload nhận ct thì truyền ct vào luôn)
                entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                entity.MaPhieu = await SinhMaPhieuTuDongAsync("PDK-", 5);

                // 4) Lấy mã loại thiết kế theo căn hộ
                entity.MaLoaiThietKe = await (
                    from sp in context.DaDanhMucSanPhams.AsNoTracking()
                    where sp.MaSanPham == maCanHo

                    join ltk in context.DaDanhMucLoaiCanHos.AsNoTracking()
                        on sp.MaLoaiCan equals ltk.MaLoaiCanHo into ltkGroup
                    from ltk2 in ltkGroup.DefaultIfEmpty()

                    select ltk2 != null ? (ltk2.MaLoaiThietKe ?? string.Empty) : string.Empty
                ).FirstOrDefaultAsync(ct) ?? string.Empty;

                // 5) Load chi tiết CSBH theo giá từ sơ đồ
                // (Nếu ListChiTietBanHangPDKAsync có ct thì truyền vào luôn)
                entity.ListPDKCSBH = await ListChiTietBanHangPDKAsync(
                    string.Empty,
                    entity.MaDuAn,
                    entity.DotMoBan,
                    soDo.GiaBanTruocThue ?? 0,
                    soDo.GiaBanChinhThucTruocThue ?? 0,
                    soDo.PhuongThucTinhCk
                );

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (OperationCanceledException)
            {
                return ResultModel.Fail("Thao tác đã bị huỷ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[GetByGioHangIdAsync] Lỗi khi lấy dữ liệu. GioHangId={GioHangId}, MaCanHo={MaCanHo}, MaDuAn={MaDuAn}",
                    id, maCanHo, soDo?.MaDuAn);

                return ResultModel.Fail("Lỗi hệ thống: Không thể lấy dữ liệu giỏ hàng.");
            }
        }


        public async Task<List<PhieuDangKyChinhSachBanHang>> ListChiTietBanHangPDKAsync(string maPhieu, string maDuAn, string maDot, decimal giaBanTruocThue, decimal giaBanTruocThueChinhThuc, string phuongThucTinhCK)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                string MaNhanVien = NguoiLap.MaNhanVien;
                var parameters = new DynamicParameters();
                parameters.Add("@MaDuAn", maDuAn);
                parameters.Add("@DotBanHang", maDot);
                parameters.Add("@GiaBanTruocThue", giaBanTruocThue);
                parameters.Add("@GiaBanTruocThueChinhThuc", giaBanTruocThueChinhThuc);
                parameters.Add("@PhuongThucTinhCK", phuongThucTinhCK);
                parameters.Add("@MaPhieu", maPhieu);

                var result = await connection.QueryAsync<PhieuDangKyChinhSachBanHang>(
                    "Proc_PhieuDangKy_ChinhSachBanHang",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                return result.ToList();
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi ListChiTietBanHangPDKAsync với maDuAn={maDuAn}, maDot={maDot}", maDuAn, maDot);
                return new List<PhieuDangKyChinhSachBanHang>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi ListChiTietBanHangPDKAsync với maDuAn={maDuAn}, maDot={maDot}", maDuAn, maDot);
                return new List<PhieuDangKyChinhSachBanHang>();
            }
        }

        public async Task<ResultModel> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ResultModel.Fail("Mã phiếu không hợp lệ.");

            try
            {
                await using var context = _factory.CreateDbContext();

                // Query chính (NoTracking cho view/detail)
                var entity = await (
                    from pgc in context.BhPhieuDangKiChonCans.AsNoTracking()
                    where pgc.MaPhieu == id

                    // Giỏ hàng
                    join gh in context.BhGioHangs.AsNoTracking()
                        on pgc.MaGioHang equals gh.MaPhieu into ghGroup
                    from gh2 in ghGroup.DefaultIfEmpty()

                        // KH tạm
                    join kh in context.KhDmkhachHangTams.AsNoTracking()
                        on pgc.MaKhachHangTam equals kh.MaKhachHangTam into khGroup
                    from kh2 in khGroup.DefaultIfEmpty()

                        // Dự án
                    join da in context.DaDanhMucDuAns.AsNoTracking()
                        on pgc.MaDuAn equals da.MaDuAn into daGroup
                    from da2 in daGroup.DefaultIfEmpty()

                        // Căn hộ / sản phẩm
                    join sp in context.DaDanhMucSanPhams.AsNoTracking()
                        on pgc.MaCanHo equals sp.MaSanPham into spGroup
                    from sp2 in spGroup.DefaultIfEmpty()

                        // Loại căn hộ
                    join ltk in context.DaDanhMucLoaiCanHos.AsNoTracking()
                        on sp2.MaLoaiCan equals ltk.MaLoaiCanHo into ltkGroup
                    from ltk2 in ltkGroup.DefaultIfEmpty()

                        // Sàn giao dịch
                    join sgg in context.DmSanGiaoDiches.AsNoTracking()
                        on pgc.SanGiaoDich equals sgg.MaSanGiaoDich into sggGroup
                    from sgg2 in sggGroup.DefaultIfEmpty()

                        // Đợt mở bán
                    join dmb in context.DaDanhMucDotMoBans.AsNoTracking()
                        on pgc.DotMoBan equals dmb.MaDotMoBan into dmbGroup
                    from dmb2 in dmbGroup.DefaultIfEmpty()

                        // CSTT
                    join cs in context.BhChinhSachThanhToans.AsNoTracking()
                        on pgc.MaChinhSachTt equals cs.MaCstt into csGroup
                    from cs2 in csGroup.DefaultIfEmpty()

                        // ✅ MaPhieuDC: lấy phiếu đặt cọc mới nhất theo MaPhieuDC (nếu có NgayLap thì đổi order theo ngày)
                    let maPhieuDc = context.BhPhieuDatCocs.AsNoTracking()
                        .Where(dc => dc.MaPhieuDangKy == pgc.MaPhieu)
                        // TODO: nếu có cột ngày: .OrderByDescending(dc => dc.NgayLap)
                        .OrderByDescending(dc => dc.MaPhieuDc)
                        .Select(dc => dc.MaPhieuDc)
                        .FirstOrDefault()

                    select new PhieuDangKyModel
                    {
                        MaPhieu = pgc.MaPhieu,

                        // ✅ Thêm property này trong model: public string? MaPhieuDC {get;set;}
                        MaPhieuDC = maPhieuDc,

                        MaKhachHang = pgc.MaKhachHangTam ?? string.Empty,
                        TenKhachHang = kh2 != null ? kh2.TenKhachHang : null,

                        NgayLap = pgc.NgayLap ?? DateTime.Now,
                        MaNhanVien = pgc.NguoiLap ?? string.Empty,

                        MaDuAn = pgc.MaDuAn ?? string.Empty,
                        TenDuAn = da2 != null ? da2.TenDuAn : null,

                        MaCanHo = pgc.MaCanHo ?? string.Empty,
                        TenCanHo = sp2 != null ? sp2.TenSanPham : null,

                        DotMoBan = pgc.DotMoBan ?? string.Empty,
                        TenDotMoBan = dmb2 != null ? dmb2.TenDotMoBan : null,

                        GiaBan = pgc.GiaBan ?? 0,

                        SanGiaoDich = pgc.SanGiaoDich ?? string.Empty,
                        TenSanGiaoDich = sgg2 != null ? (sgg2.TenSanGiaoDich ?? string.Empty) : string.Empty,

                        TenNhanVienMoiGioi = pgc.NhanVienMoiGioi ?? string.Empty,

                        MaChinhSachTt = pgc.MaChinhSachTt,
                        TenChinhSachTt = cs2 != null ? cs2.TenCstt : null,

                        MaGioHang = pgc.MaGioHang ?? string.Empty,
                        LoaiGioHang = gh2 != null ? (gh2.LoaiGioHang ?? false) : false,

                        GiaBanTheoCSTT = pgc.GiaBanTheoCstt ?? 0,
                        MaPhieuBooking = pgc.MaPhieuBooking,

                        IsXacNhan = pgc.IsXacNhan ?? false,
                        IsHetHieuLuc = pgc.IsHetHieuLuc ?? false,

                        NoiDung = pgc.NoiDung,

                        MaLoaiThietKe = ltk2 != null ? (ltk2.MaLoaiThietKe ?? string.Empty) : string.Empty,
                        TyLeChietKhau = cs2 != null ? (cs2.TyLeChietKhau ?? 0) : 0,

                        IsXacNhanChuyenCoc = pgc.IsXacNhanChuyenCoc ?? false,
                        IsMoBanCoGia = pgc.IsMoBanCoGia ?? false,

                        GiaBanChinhThuc = pgc.GiaBanChinhThuc,
                        GiaBanTruocThue = pgc.GiaBanTruocThue,
                        GiaBanChinhThucTruocThue = pgc.GiaBanChinhThucTruocThue,

                        PhuongThucTinhCk = pgc.PhuongThucTinhChietKhauKm,

                        DienTichCanHo = pgc.DienTichTimTuong,
                        DienTichLotLong = pgc.DienTichThongThuy,
                        DienTichSanVuon = pgc.DienTichSanVuon,

                        IsMoBanGiaTran = pgc.IsMoBanGiaTran,
                    }
                ).FirstOrDefaultAsync();

                if (entity is null)
                    return ResultModel.Fail("Không tìm thấy phiếu đăng ký.");

                // Enrich data (tách rõ phần bổ sung dữ liệu)
                entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);

                //entity.FlagTong =
                //    !entity.IsXacNhan
                //    && !entity.IsHetHieuLuc
                //    && entity.NguoiLap != null
                //    && string.Equals(entity.NguoiLap.MaNhanVien, _currentUser.MaNhanVien, StringComparison.OrdinalIgnoreCase);

                if (!entity.IsXacNhan && entity.NguoiLap != null && entity.NguoiLap.MaNhanVien == _currentUser.MaNhanVien && !entity.IsHetHieuLuc)
                {
                    entity.FlagTong = true;
                }

                entity.ListPDKCSBH = await ListChiTietBanHangPDKAsync(
                    entity.MaPhieu,
                    entity.MaDuAn,
                    entity.DotMoBan,
                    entity.GiaBanTruocThue ?? 0,
                    entity.GiaBanChinhThucTruocThue ?? 0,
                    entity.PhuongThucTinhCk
                );

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu đăng ký. Id={Id}", id);
                return ResultModel.Fail("Lỗi hệ thống, vui lòng thử lại sau.");
            }
        }

        public async Task<ResultModel> GetById2Async(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from pgc in _context.BhPhieuDangKiChonCans
                      join gh in _context.BhGioHangs on pgc.MaGioHang equals gh.MaPhieu into dtGH
                      from gh2 in dtGH.DefaultIfEmpty()
                      join khGoc in _context.KhDmkhachHangTams on pgc.MaKhachHangTam equals khGoc.MaKhachHangTam into dtKH
                      from khGoc2 in dtKH.DefaultIfEmpty()

                      join da in _context.DaDanhMucDuAns on pgc.MaDuAn equals da.MaDuAn into dtDuAn
                      from da2 in dtDuAn.DefaultIfEmpty()

                      join sp in _context.DaDanhMucSanPhams on pgc.MaCanHo equals sp.MaSanPham into spGroup
                      from sp2 in spGroup.DefaultIfEmpty()

                      join ltk in _context.DaDanhMucLoaiCanHos on sp2.MaLoaiCan equals ltk.MaLoaiCanHo into dtLTK
                      from ltk2 in dtLTK.DefaultIfEmpty()

                      join sgg in _context.DmSanGiaoDiches on pgc.SanGiaoDich equals sgg.MaSanGiaoDich into dtSGG
                      from sgg2 in dtSGG.DefaultIfEmpty()

                      join dmb in _context.DaDanhMucDotMoBans on pgc.DotMoBan equals dmb.MaDotMoBan into dtDMB
                      from dmb2 in dtDMB.DefaultIfEmpty()

                      join cs in _context.BhChinhSachThanhToans on pgc.MaChinhSachTt equals cs.MaCstt into csGroup
                      from cs in csGroup.DefaultIfEmpty()

                      where pgc.MaPhieu == id
                      select new PhieuDangKyModel
                      {
                          MaPhieu = pgc.MaPhieu,
                          MaKhachHang = pgc.MaKhachHangTam ?? string.Empty,
                          TenKhachHang = khGoc2.TenKhachHang,
                          NgayLap = pgc.NgayLap ?? DateTime.Now,
                          MaNhanVien = pgc.NguoiLap ?? string.Empty,
                          MaDuAn = pgc.MaDuAn ?? string.Empty,
                          TenDuAn = da2.TenDuAn,
                          MaCanHo = pgc.MaCanHo ?? string.Empty,
                          TenCanHo = sp2.TenSanPham,
                          DotMoBan = pgc.DotMoBan ?? string.Empty,
                          TenDotMoBan = dmb2.TenDotMoBan,
                          GiaBan = pgc.GiaBan ?? 0,
                          SanGiaoDich = pgc.SanGiaoDich ?? string.Empty,
                          TenSanGiaoDich = sgg2.TenSanGiaoDich ?? string.Empty,
                          TenNhanVienMoiGioi = pgc.NhanVienMoiGioi ?? string.Empty,
                          MaChinhSachTt = pgc.MaChinhSachTt,
                          TenChinhSachTt = cs.TenCstt,
                          MaGioHang = pgc.MaGioHang ?? string.Empty,
                          LoaiGioHang = gh2.LoaiGioHang ?? false,
                          GiaBanTheoCSTT = pgc.GiaBanTheoCstt ?? 0,
                          MaPhieuBooking = pgc.MaPhieuBooking,
                          IsXacNhan = pgc.IsXacNhan ?? false,
                          IsHetHieuLuc = pgc.IsHetHieuLuc ?? false,
                          NoiDung = pgc.NoiDung,
                          MaLoaiThietKe = ltk2.MaLoaiThietKe ?? string.Empty,
                          TyLeChietKhau = cs.TyLeChietKhau ?? 0,
                          IsXacNhanChuyenCoc = pgc.IsXacNhanChuyenCoc ?? false,
                          IsMoBanCoGia = pgc.IsMoBanCoGia ?? false,
                          GiaBanChinhThuc = pgc.GiaBanChinhThuc,
                          GiaBanTruocThue = pgc.GiaBanTruocThue,
                          GiaBanChinhThucTruocThue = pgc.GiaBanChinhThucTruocThue,
                          PhuongThucTinhCk = pgc.PhuongThucTinhChietKhauKm,
                          DienTichCanHo = pgc.DienTichTimTuong,
                          DienTichLotLong = pgc.DienTichThongThuy,
                          DienTichSanVuon = pgc.DienTichSanVuon,
                          IsMoBanGiaTran = pgc.IsMoBanGiaTran,
                      }).FirstOrDefaultAsync();
                if (entity != null)
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    if (!entity.IsXacNhan && entity.NguoiLap != null && entity.NguoiLap.MaNhanVien == _currentUser.MaNhanVien && !entity.IsHetHieuLuc)
                    {
                        entity.FlagTong = true;
                    }
                    entity.ListPDKCSBH = await ListChiTietBanHangPDKAsync(entity.MaPhieu, entity.MaDuAn, entity.DotMoBan, entity.GiaBanTruocThue ?? 0, entity.GiaBanChinhThucTruocThue ?? 0, entity.PhuongThucTinhCk);
                }

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu đăng ký");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thêm xóa sửa
        //public async Task<ResultModel> SavePhieuAsync(PhieuDangKyModel model)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var record = new BhPhieuDangKiChonCan
        //        {
        //            MaPhieu = await SinhMaPhieuTuDongAsync("PDK-", 5),
        //            MaKhachHangTam = model.MaKhachHang,
        //            MaDuAn = model.MaDuAn,
        //            DotMoBan = model.DotMoBan,
        //            SanGiaoDich = model.SanGiaoDich,
        //            NhanVienMoiGioi = model.TenNhanVienMoiGioi,
        //            GiaBan = model.GiaBan,
        //            GiaBanTheoCstt = model.GiaBanTheoCSTT,
        //            MaGioHang = model.MaGioHang,
        //            LoaiGioHang = model.LoaiGioHang,
        //            MaCanHo = model.MaCanHo,
        //            MaChinhSachTt = model.MaChinhSachTt,
        //            MaPhieuBooking = model.MaPhieuBooking,
        //            NoiDung = model.NoiDung
        //        };
        //        var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
        //        record.NguoiLap = NguoiLap.MaNhanVien;
        //        record.NgayLap = DateTime.Now;

        //        await _context.BhPhieuDangKiChonCans.AddAsync(record);

        //        var updateSP = await _context.DaDanhMucSanPhams.Where(d => d.MaSanPham == model.MaCanHo).FirstOrDefaultAsync();
        //        if (updateSP != null)
        //        {
        //            if ((updateSP.HienTrangKd ?? "ConTrong") == "DaBan")
        //            {
        //                _nav.NavigateTo("/sanpham-dadangky");
        //            }
        //            updateSP.HienTrangKd = "MB";
        //        }
        //        await _context.SaveChangesAsync();
        //        // Thêm hub ở đây thông báo cho biết là đã có thay đổi trình trạng kinh doanh căn hộ.
        //        await _notifier.UpdateTTCHAsync();
        //        return ResultModel.SuccessWithId(record.MaPhieu, "Thêm phiếu đăng ký thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi Thêm phiếu đăng ký");
        //        return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu đăng ký: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> SavePhieuAsync(PhieuDangKyModel model, CancellationToken ct = default)
        {
            try
            {
                await using var _context = _factory.CreateDbContext();

                // Dùng giao dịch mức cô lập cao để chống race condition khi 2 người bấm cùng lúc
                await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                // 0) Chặn nếu căn hộ đã có phiếu ĐK đang hiệu lực & chưa xác nhận
                //var hasActivePdk = await _context.BhPhieuDangKiChonCans
                //    .AsNoTracking()
                //    .AnyAsync(x =>
                //        x.MaCanHo == model.MaCanHo &&
                //        x.MaGioHang == model.MaGioHang &&
                //         x.MaDuAn == model.MaDuAn &&
                //        (x.IsHetHieuLuc != true) &&                 // NULL hoặc false đều xem là còn hiệu lực
                //        (x.IsXacNhan == null || x.IsXacNhan == false));

                //if (hasActivePdk)
                //{
                //    return ResultModel.Fail("Căn hộ này đã có phiếu đăng ký còn hiệu lực và chưa xác nhận. Không thể đăng ký thêm.");
                //}

                var trangThai = await _baseService.CanShowDangKyAsync(model.MaDuAn ?? string.Empty, model.MaCanHo, model.MaGioHang);
                if (!trangThai.Status)
                {
                    return ResultModel.Fail(trangThai.Message);
                }

                // 1) Khóa bản ghi Sản phẩm để cập nhật trạng thái an toàn
                var updateSP = await _context.DaDanhMucSanPhams
                    .Where(d => d.MaSanPham == model.MaCanHo && d.MaDuAn == model.MaDuAn)
                    .FirstOrDefaultAsync();

                if (updateSP == null)
                    return ResultModel.Fail("Không tìm thấy căn hộ.");

                // Nếu đã bán hoặc đã được giữ (MB) thì không cho đăng ký
                //var ht = (updateSP.HienTrangKd ?? "ConTrong");
                //if (string.Equals(ht, "DaBan", StringComparison.OrdinalIgnoreCase))
                //    return ResultModel.Fail("Căn hộ đã bán. Không thể đăng ký.");
                //if (string.Equals(ht, "MB", StringComparison.OrdinalIgnoreCase))
                //    return ResultModel.Fail("Căn hộ đang được giữ/MB. Không thể đăng ký.");

                // 2) Sinh mã phiếu & tạo bản ghi
                var record = new BhPhieuDangKiChonCan
                {
                    MaPhieu = await SinhMaPhieuTuDongAsync("PDK-", 5),
                    MaKhachHangTam = model.MaKhachHang,
                    MaDuAn = model.MaDuAn,
                    DotMoBan = model.DotMoBan,
                    SanGiaoDich = model.SanGiaoDich,
                    NhanVienMoiGioi = model.TenNhanVienMoiGioi,
                    GiaBan = model.GiaBan,
                    GiaBanTheoCstt = model.GiaBanTheoCSTT,
                    MaGioHang = model.MaGioHang,
                    LoaiGioHang = model.LoaiGioHang,
                    MaCanHo = model.MaCanHo,
                    MaChinhSachTt = model.MaChinhSachTt,
                    MaPhieuBooking = model.MaPhieuBooking,
                    NoiDung = model.NoiDung,
                    IsMoBanCoGia = model.IsMoBanCoGia,
                    GiaBanChinhThuc = model.GiaBanChinhThuc,
                    GiaBanTruocThue = model.GiaBanTruocThue,
                    GiaBanChinhThucTruocThue = model.GiaBanChinhThucTruocThue,
                    PhuongThucTinhChietKhauKm = model.PhuongThucTinhCk,
                    DienTichTimTuong = model.DienTichCanHo,
                    DienTichThongThuy = model.DienTichLotLong,
                    DienTichSanVuon = model.DienTichSanVuon,
                    IsMoBanGiaTran = model.IsMoBanGiaTran,
                    IsXacNhanChuyenCoc = false,
                };

                var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = nguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;

                await _context.BhPhieuDangKiChonCans.AddAsync(record);

                //3 Insert chính sách bán hàng phiếu đăng ký chọn căn               
                var details = BuildDetails(record.MaPhieu, model.ListPDKCSBH);
                if (details.Count > 0)
                    await _context.BhPhieuDangKiChonCanCsbhs.AddRangeAsync(details, ct);

                //4) Cập nhật trạng thái căn hộ → MB (đang giữ)
                updateSP.HienTrangKd = "MB";

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                // 5) Thông báo realtime
                await _notifier.UpdateTTCHAsync();

                return ResultModel.SuccessWithId(record.MaPhieu, "Thêm phiếu đăng ký thành công");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // Phòng trường hợp vi phạm unique index (nếu bạn tạo ở DB) → báo lỗi thân thiện
                _logger.LogError(dbEx, "Lỗi khi Thêm phiếu đăng ký (unique/index)");
                return ResultModel.Fail("Căn hộ đã có phiếu đăng ký còn hiệu lực. Không thể đăng ký thêm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm phiếu đăng ký");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu đăng ký: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuAsync(PhieuDangKyModel model, CancellationToken ct = default)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.BhPhieuDangKiChonCans.FirstOrDefaultAsync(d => d.MaPhieu == model.MaPhieu);
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy thông tin phiếu đăng ký chọn căn");
                entity.NhanVienMoiGioi = model.TenNhanVienMoiGioi;
                entity.MaKhachHangTam = model.MaKhachHang;
                entity.MaPhieuBooking = model.MaPhieuBooking;
                entity.MaChinhSachTt = model.MaChinhSachTt;
                entity.NoiDung = model.NoiDung;

                // 1) Xoá detail cũ trước
                await _context.BhPhieuDangKiChonCanCsbhs
                    .Where(x => x.MaPhieuDangKy == entity.MaPhieu)
                    .ExecuteDeleteAsync(ct); // EF Core 7/8

                //2 Insert chính sách bán hàng phiếu đăng ký chọn căn               
                var details = BuildDetails(entity.MaPhieu, model.ListPDKCSBH);
                if (details.Count > 0)
                    await _context.BhPhieuDangKiChonCanCsbhs.AddRangeAsync(details, ct);

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật phiếu đăng ký chọn căn thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu đăng ký chọn căn");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật phiếu đăng ký chọn căn: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuXacNhanChuyenCocAsync(
     PhieuDangKyModel model,
     CancellationToken ct = default)
        {
            if (model == null)
                return ResultModel.Fail("Dữ liệu không hợp lệ.");

            var maPhieu = (model.MaPhieu ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(maPhieu))
                return ResultModel.Fail("Mã phiếu không hợp lệ.");

            try
            {
                await using var context = _factory.CreateDbContext();

                var entity = await context.BhPhieuDangKiChonCans
                    .FirstOrDefaultAsync(d => d.MaPhieu == maPhieu, ct);

                if (entity is null)
                    return ResultModel.Fail("Không tìm thấy thông tin phiếu đăng ký chọn căn.");

                // ✅ RULE: Đã lên phiếu đặt cọc => KHÔNG cho cập nhật nữa
                var hasDatCoc = await context.BhPhieuDatCocs.AsNoTracking()
                    .AnyAsync(dc => dc.MaPhieuDangKy == maPhieu, ct);

                if (hasDatCoc)
                    return ResultModel.Fail("Phiếu đã lên phiếu đặt cọc nên không thể cập nhật.");

                // Chuẩn hoá input
                var newMaKh = (model.MaKhachHang ?? string.Empty).Trim();
                var newBooking = (model.MaPhieuBooking ?? string.Empty).Trim();
                var newNoiDung = model.NoiDung;

                bool changedCustomer =
                    !string.Equals((entity.MaKhachHangTam ?? string.Empty).Trim(), newMaKh, StringComparison.OrdinalIgnoreCase);

                bool changedBooking =
                    !string.Equals((entity.MaPhieuBooking ?? string.Empty).Trim(), newBooking, StringComparison.OrdinalIgnoreCase);

                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 1) CHƯA xác nhận chuyển cọc => cho phép chọn lại CSBH
                if (!(entity.IsXacNhanChuyenCoc ?? false))
                {
                    await context.BhPhieuDangKiChonCanCsbhs
                        .Where(x => x.MaPhieuDangKy == entity.MaPhieu)
                        .ExecuteDeleteAsync(ct);

                    var details = BuildDetails(entity.MaPhieu, model.ListPDKCSBH);
                    if (details.Count > 0)
                        await context.BhPhieuDangKiChonCanCsbhs.AddRangeAsync(details, ct);
                }

                // 2) Nếu đổi KH hoặc booking => lưu lịch sử
                if (changedCustomer || changedBooking)
                {
                    var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();

                    var history = new BhPhieuDangKiChonCanLichSuThayDoiKhachHang
                    {
                        MaPhieuDangKy = entity.MaPhieu,
                        MaPhieuBooking = entity.MaPhieuBooking,
                        MaKhachHangTam = entity.MaKhachHangTam,
                        NgayCapNhat = DateTime.Now,
                        NguoiCapNhat = nguoiLap?.MaNhanVien
                    };

                    await context.BhPhieuDangKiChonCanLichSuThayDoiKhachHangs.AddAsync(history, ct);
                }

                // 3) Update phiếu
                entity.MaKhachHangTam = newMaKh;
                entity.MaPhieuBooking = newBooking;
                entity.NoiDung = newNoiDung;
                entity.MaChinhSachTt = model.MaChinhSachTt;

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật phiếu đăng ký chọn căn thành công.");
            }
            catch (OperationCanceledException)
            {
                return ResultModel.Fail("Thao tác đã bị huỷ.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[UpdatePhieuXacNhanChuyenCocAsync] Lỗi khi cập nhật. MaPhieu={MaPhieu}", model?.MaPhieu);

                return ResultModel.Fail("Lỗi hệ thống: Không thể cập nhật phiếu đăng ký chọn căn.");
            }
        }


        //public async Task<ResultModel> DeleteRecordAsync(string maPhieu)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var entity = await _context.BhPhieuDangKiChonCans.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
        //        if (entity == null)
        //            return ResultModel.Fail("Không tìm thấy phiếu đăng ký");

        //        var updaHDKD = await _context.DaDanhMucSanPhams.Where(d => d.MaSanPham == entity.MaCanHo).FirstOrDefaultAsync();
        //        updaHDKD.HienTrangKd = "MB";
        //        _context.BhPhieuDangKiChonCans.Remove(entity);
        //        await _context.SaveChangesAsync();
        //        // Thêm hub ở đây thông báo cho biết là đã có thay đổi trình trạng kinh doanh căn hộ.
        //        await _notifier.UpdateTTCHAsync();
        //        return ResultModel.Success($"Xóa phiếu đăng ký {entity.MaPhieu} thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[DeleteRecordAsync] Lỗi khi xóa phiếu đăng ký");
        //        return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> DeleteRecordAsync(string maPhieu, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maPhieu))
                return ResultModel.Fail("Mã phiếu không hợp lệ.");

            maPhieu = maPhieu.Trim();

            try
            {
                await using var context = _factory.CreateDbContext();

                // Transaction để đảm bảo: xóa CSBH + xóa phiếu + update sản phẩm là 1 gói
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // Lấy phiếu
                var entity = await context.BhPhieuDangKiChonCans
                    .FirstOrDefaultAsync(x => x.MaPhieu == maPhieu, ct);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy phiếu đăng ký.");

                var maCanHo = (entity.MaCanHo ?? "").Trim();

                // 1) Xóa bảng con: BH_PhieuDangKiChonCan_CSBH
                //    (DbSet tên gì thì đổi cho đúng)
                var deletedCsbhs = await context.BhPhieuDangKiChonCanCsbhs
                    .Where(x => x.MaPhieuDangKy == maPhieu)
                    .ExecuteDeleteAsync(ct);

                // 2) Update tình trạng kinh doanh căn hộ về MB
                if (!string.IsNullOrWhiteSpace(maCanHo))
                {
                    var sp = await context.DaDanhMucSanPhams
                        .FirstOrDefaultAsync(x => x.MaSanPham == maCanHo && x.MaDuAn == entity.MaDuAn, ct);

                    if (sp != null)
                    {
                        sp.HienTrangKd = "MB";
                        // nếu bạn có các cột audit thì set luôn
                        // sp.NgayCapNhat = DateTime.Now;
                        // sp.NguoiCapNhat = ...
                    }
                }

                // 3) Xóa phiếu
                context.BhPhieuDangKiChonCans.Remove(entity);
                await context.SaveChangesAsync(ct);

                await tx.CommitAsync(ct);

                // 4) Thêm hub ở đây thông báo cho biết là đã có thay đổi trình trạng kinh doanh căn hộ.
                await _notifier.UpdateTTCHAsync();

                _logger.LogInformation(
                    "[DeleteRecordAsync] Deleted MaPhieu={MaPhieu}, MaCanHo={MaCanHo}, DeletedCSBH={DeletedCsbhs}",
                    maPhieu, maCanHo, deletedCsbhs);

                return ResultModel.Success($"Xóa phiếu đăng ký {maPhieu} thành công.");
            }
            catch (OperationCanceledException)
            {
                return ResultModel.Fail("Thao tác đã bị hủy.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteRecordAsync] Lỗi khi xóa phiếu đăng ký. MaPhieu={MaPhieu}", maPhieu);
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        //public async Task<ResultModel> DeleteListAsync(List<PhieuDangKyPagingDto> listPGC)
        //{
        //    try
        //    {
        //        var ids = listPGC?
        //    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
        //    .Select(x => x.MaPhieu!.Trim())
        //    .Distinct(StringComparer.OrdinalIgnoreCase)
        //    .ToList() ?? new List<string>();

        //        if (ids.Count == 0)
        //            return ResultModel.Success("Không có dòng nào được chọn để xoá.");

        //        await using var _context = _factory.CreateDbContext();

        //        var targetIds = ids;
        //        await using var tx = await _context.Database.BeginTransactionAsync();

        //        // 1) Lấy danh sách MaSanPham liên quan đến các phiếu sẽ xoá (lấy TRƯỚC khi xoá)
        //        var sanPhamIds = await _context.BhPhieuDangKiChonCans
        //            .Where(d => targetIds.Contains(d.MaPhieu))
        //            .Select(d => d.MaCanHo)                // đổi tên field nếu của bạn khác
        //            .Where(ma => ma != null && ma != "")
        //            .Distinct()
        //            .ToListAsync();

        //        // 2) Xoá các phiếu
        //        var c1 = await _context.BhPhieuDangKiChonCans
        //        .Where(d => targetIds.Contains(d.MaPhieu))
        //        .ExecuteDeleteAsync();

        //        // 3) Cập nhật trạng thái KD của các sản phẩm vừa liên quan về "Đang mở bán"
        //        if (sanPhamIds.Count > 0)
        //        {
        //            var updated = await _context.DaDanhMucSanPhams
        //                .Where(sp => sanPhamIds.Contains(sp.MaSanPham))
        //                .ExecuteUpdateAsync(setters => setters
        //                    .SetProperty(sp => sp.HienTrangKd, sp => "MB")
        //                // .SetProperty(sp => sp.NgayCapNhat, sp => DateTime.Now)        // nếu bạn có cột audit
        //                // .SetProperty(sp => sp.NguoiCapNhat, sp => currentUserName)    // nếu có
        //                );
        //        }

        //        await tx.CommitAsync();
        //        // Thêm hub ở đây thông báo cho biết là đã có thay đổi trình trạng kinh doanh căn hộ.
        //        await _notifier.UpdateTTCHAsync();
        //        return ResultModel.Success($"Xóa danh sách thanh phiếu đăng ký thành công");

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách phiếu đăng ký");
        //        return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
        //    }
        //}
        public async Task<ResultModel> DeleteListAsync(List<PhieuDangKyPagingDto> listPGC, CancellationToken ct = default)
        {
            var ids = listPGC?
                .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
                .Select(x => x!.MaPhieu.Trim())
                .Where(x => x.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            if (ids.Count == 0)
                return ResultModel.Success("Không có dòng nào được chọn để xoá.");

            try
            {
                await using var context = _factory.CreateDbContext();
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 1) Lấy danh sách (MaDuAn, MaCanHo) liên quan TRƯỚC khi xóa
                var spKeys = await context.BhPhieuDangKiChonCans
                    .Where(d => ids.Contains(d.MaPhieu))
                    .Select(d => new
                    {
                        MaDuAn = (d.MaDuAn ?? "").Trim(),
                        MaSanPham = (d.MaCanHo ?? "").Trim()
                    })
                    .Where(x => x.MaDuAn != "" && x.MaSanPham != "")
                    .Distinct()
                    .ToListAsync(ct);

                // 2) Xóa bảng con: BH_PhieuDangKiChonCan_CSBH (đổi DbSet đúng tên)
                var deletedChild = await context.BhPhieuDangKiChonCanCsbhs
                    .Where(x => ids.Contains(x.MaPhieuDangKy))
                    .ExecuteDeleteAsync(ct);

                // 3) Xóa bảng cha
                var deletedParent = await context.BhPhieuDangKiChonCans
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync(ct);

                // 4) Update trạng thái KD về MB theo (MaDuAn + MaSanPham)
                int updatedSp = 0;

                if (spKeys.Count > 0)
                {
                    // cách an toàn: update từng key (số lượng phiếu xóa thường không quá lớn)
                    foreach (var k in spKeys)
                    {
                        updatedSp += await context.DaDanhMucSanPhams
                            .Where(sp => sp.MaDuAn == k.MaDuAn && sp.MaSanPham == k.MaSanPham)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(sp => sp.HienTrangKd, _ => "MB")
                            , ct);
                    }
                }

                await tx.CommitAsync(ct);

                // 5) Thêm hub ở đây thông báo cho biết là đã có thay đổi trình trạng kinh doanh căn hộ.
                await _notifier.UpdateTTCHAsync();

                _logger.LogInformation(
                    "[DeleteListAsync] DeletedParent={DeletedParent}, DeletedChild={DeletedChild}, UpdatedSP={UpdatedSp}, IdsCount={IdsCount}",
                    deletedParent, deletedChild, updatedSp, ids.Count);

                return ResultModel.Success("Xóa danh sách phiếu đăng ký thành công.");
            }
            catch (OperationCanceledException)
            {
                return ResultModel.Fail("Thao tác đã bị hủy.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách phiếu đăng ký. IdsCount={IdsCount}", ids.Count);
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> XacNhanPhieuDKAsync(string maPhieu, string maPhieuBooking, string maKhachHang)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                // Giao dịch mức cô lập cao để chống xác nhận trùng
                await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                var entity = await _context.BhPhieuDangKiChonCans.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu đăng ký");
                }
                if (entity.IsHetHieuLuc == true)
                {
                    return ResultModel.Fail("Phiếu đăng ký đã hết hiệu lực bạn không được quyền xác nhận nữa");
                }
                if (entity.IsXacNhan == true)
                {
                    return ResultModel.Fail("Phiếu đăng ký này đã được xác nhận trước đó.");
                }
                var updateSP = await _context.DaDanhMucSanPhams.Where(d => d.MaSanPham == entity.MaCanHo && d.MaDuAn == entity.MaDuAn).FirstOrDefaultAsync();
                if (updateSP != null)
                {
                    updateSP.HienTrangKd = "DaBan";
                }
                else
                {
                    return ResultModel.Fail("Không tìm thấy căn hộ.");
                }
                entity.IsXacNhan = true;
                entity.MaKhachHangTam = maKhachHang;
                entity.MaPhieuBooking = maPhieuBooking;
                /* Insert khách hàng tạm vào khách hàng khi xác nhận phiếu đăng ký
                 1. Kiểm tra mã khách hàng tạm đã có trong bảng KH_DMKhachHang_Nguon hay chưa nếu có thì dừng không đẩy vô
                    Còn ngược lại chưa có thì đầy vô bảng KH_DMKhachHang_Nguon, KH_DMKhachHang_Nguon, KH_DMKhachHangChiTiet
                 */
                var flagKHT = await _context.KhDmkhachHangNguons.Where(d => d.MaKhachHangTam == entity.MaKhachHangTam).FirstOrDefaultAsync();
                if (flagKHT == null)
                {
                    var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    var khTam = await _context.KhDmkhachHangTams.Where(d => d.MaKhachHangTam == entity.MaKhachHangTam).FirstOrDefaultAsync();
                    if (khTam != null)
                    {
                        KhDmkhachHang kh = new KhDmkhachHang();
                        kh.MaKhachHang = await SinhMaKhachHangTuDongAsync("KH-", 5);
                        kh.TenKhachHang = khTam.TenKhachHang;
                        kh.MaDoiTuongKhachHang = khTam.MaDoiTuongKhachHang;
                        kh.MaNhanVien = NguoiLap.MaNhanVien;
                        kh.NgayLap = DateTime.Now;
                        //Insert bảng khách hàng nguồn
                        var recordNguon = new KhDmkhachHangNguon
                        {
                            MaKhachHang = kh.MaKhachHang,
                            MaKhachHangTam = entity.MaKhachHangTam,
                        };
                        //Insert bảng chi tiết
                        var recordCT = new KhDmkhachHangChiTiet
                        {
                            MaKhachHang = kh.MaKhachHang,
                            IdlanDieuChinh = await TaoMaKhachHangMoiAsync(kh.MaKhachHang),
                            SoDienThoai = khTam.SoDienThoai ?? string.Empty,
                            Email = khTam.Email ?? string.Empty,
                            QuocTich = khTam.QuocTich ?? string.Empty,
                            NgaySinh = khTam.NgaySinh,
                            GioiTinh = khTam.GioiTinh,
                            MaLoaiIdCard = khTam.MaLoaiIdCard ?? string.Empty,
                            IdCard = khTam.IdCard ?? string.Empty,
                            NgayCapIdCard = khTam.NgayCapIdCard,
                            NoiCapIdCard = khTam.NoiCapIdCard,
                            DiaChiThuongTru = khTam.DiaChiThuongTru,
                            DiaChiLienLac = khTam.DiaChiHienNay,
                            NguoiDaiDien = khTam.NguoiDaiDien,
                            SoDienThoaiDaiDien = khTam.SoDienThoaiNguoiDaiDien,
                            NgayCapNhat = kh.NgayLap ?? DateTime.Now
                        };
                        await _context.KhDmkhachHangs.AddAsync(kh);
                        await _context.KhDmkhachHangNguons.AddAsync(recordNguon);
                        await _context.KhDmkhachHangChiTiets.AddAsync(recordCT);
                    }

                }
                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                // Thêm hub ở đây thông báo cho biết là đã có thay đổi trình trạng kinh doanh căn hộ.
                await _notifier.UpdateTTCHAsync();
                return ResultModel.Success($"Xác nhận phiếu đăng ký {entity.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[XacNhanPhieuDKAsync] Lỗi khi xác nhận phiếu đăng ký");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private static List<BhPhieuDangKiChonCanCsbh> BuildDetails(string maPhieu, List<PhieuDangKyChinhSachBanHang>? input)
        {
            var result = new List<BhPhieuDangKiChonCanCsbh>();
            if (input == null || input.Count == 0) return result;

            var i = 1;

            foreach (var item in input.Where(x => x != null))
            {
                result.Add(new BhPhieuDangKiChonCanCsbh
                {
                    MaPhieuDangKy = maPhieu,
                    MaCsbh = item.MaCS,
                    LoaiCs = (item.LoaiCS ?? string.Empty).Trim(),
                    GiaTriCk = item.GiaTriKM,
                    ThanhTienKmgiaBanDuocChon = item.ThanhTienKMGiaBanDuocChon,
                    ThanhTienKmgiaBanChinhThucDuocChon = item.ThanhTienKMGiaBanChinhThucDuocChon,
                    IsChon = item.IsChon
                });

                i++;
            }

            return result;
        }
        #endregion

        #region load droplist
        public async Task<List<DmSanGiaoDich>> GetBySanGiaoDichAsync()
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<DmSanGiaoDich>();
            try
            {
                entity = await _context.DmSanGiaoDiches.ToListAsync();
                if (entity == null)
                {
                    entity = new List<DmSanGiaoDich>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách sàn giao dịch");
            }
            return entity;
        }

        public async Task<List<BhChinhSachThanhToan>> GetChinhSachThanhToanAsync(string maDuAn)
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<BhChinhSachThanhToan>();
            try
            {
                entity = await (
                    from cs in _context.BhChinhSachThanhToans
                    where cs.MaDuAn == maDuAn && cs.IsXacNhan == true
                    select new BhChinhSachThanhToan
                    {
                        MaCstt = cs.MaCstt,
                        TenCstt = cs.TenCstt
                    }
                ).Distinct().ToListAsync();
                if (entity == null)
                {
                    entity = new List<BhChinhSachThanhToan>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách chính sách thanh toán");
            }
            return entity;
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu đăng ký   
        public async Task<string> SinhMaPhieuTuDongAsync(string prefix, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.BhPhieuDangKiChonCans
                .Where(kh => kh.MaPhieu.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaPhieu)
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

        #region Thông tin phiếu giữ chỗ chưa lên phiếu đăng ký
        public async Task<(List<PhieuGiuChoPagingDto> Data, int TotalCount)> GetPagingPGCPopupAsync(
      string? maDuAn, string? maSanGG, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", maDuAn);
            param.Add("@MaSanGD", maSanGG);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<PhieuGiuChoPagingDto>(
                "Proc_PhieuDK_PhieuGCPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Tính giá bán chính sách thanh toán=giá bán - (giá bán * tỷ lệ chiết khấu)
        //public async Task<decimal> GetGiaBanCSTT(string maCSTT, decimal giaBan)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        decimal tyLeCK = await _context.BhChinhSachThanhToans.Where(d => d.MaCstt == maCSTT).Select(d => d.TyLeChietKhau).FirstOrDefaultAsync() ?? 0;
        //        decimal giaBanCSTT = giaBan - Math.Round((giaBan * tyLeCK) / 100, 0);
        //        return giaBanCSTT;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}
        public async Task<PhieuDangKyModel> GetGiaBanCSTT(string maCSTT, decimal giaBan)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                PhieuDangKyModel pdk = new PhieuDangKyModel();
                decimal tyLeCK = await _context.BhChinhSachThanhToans.Where(d => d.MaCstt == maCSTT).Select(d => d.TyLeChietKhau).FirstOrDefaultAsync() ?? 0;
                decimal giaBanCSTT = giaBan - Math.Round((giaBan * tyLeCK) / 100, 0);
                pdk.GiaBanTheoCSTT = giaBanCSTT;
                pdk.TyLeChietKhau = tyLeCK;
                return pdk;
            }
            catch
            {
                PhieuDangKyModel pdk = new PhieuDangKyModel();
                return pdk;
            }
        }
        #endregion

        #region Hàm tăng tự động của mã khách hàng khi mỗi lần cập nhật
        public async Task<string> SinhMaKhachHangTuDongAsync(string prefix, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KhDmkhachHangs
                .Where(kh => kh.MaKhachHang.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaKhachHang)
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
        public async Task<string> TaoMaKhachHangMoiAsync(string maHienTai)
        {
            using var _context = _factory.CreateDbContext();
            var lastId = await _context.KhDmkhachHangChiTiets
                      .Where(x => x.MaKhachHang == maHienTai && x.IdlanDieuChinh.StartsWith(maHienTai + "_"))
                      .OrderByDescending(x => x.NgayCapNhat)
                      .Select(x => x.IdlanDieuChinh)
                      .FirstOrDefaultAsync();

            int nextIndex = 1;

            // Bước 2: Nếu có bản ghi trước đó, tách số ra và +1
            if (!string.IsNullOrEmpty(lastId))
            {
                var parts = lastId.Split('_');
                if (parts.Length == 2 && int.TryParse(parts[1], out int currentIndex))
                {
                    nextIndex = currentIndex + 1;
                }
            }

            // Bước 3: Trả về mã mới, padding 3 chữ số
            return $"{maHienTai}_{nextIndex.ToString("D3")}";
        }
        #endregion

        #region Xác nhận chuyển cọc
        public async Task<ResultModel> XacNhanChuyenCocPhieuDKAsync(string maPhieu, string maPhieuBooking, string maKhachHang)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                // Giao dịch mức cô lập cao để chống xác nhận trùng
                await using var tx = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

                var entity = await _context.BhPhieuDangKiChonCans.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu đăng ký");
                }
                if (entity.IsHetHieuLuc == true)
                {
                    return ResultModel.Fail("Phiếu đăng ký đã hết hiệu lực bạn không được quyền xác nhận chuyển cọc nữa");
                }
                if (entity.IsXacNhan == false)
                {
                    return ResultModel.Fail("Phiếu đăng ký này chưa được xác nhận trước đó. Bạn không thể xác nhận chuyển cọc.");
                }
                if (entity.IsXacNhanChuyenCoc == true)
                {
                    return ResultModel.Fail("Phiếu đăng ký này đã được xác nhận chuyển cọc trước đó. Bạn không thể xác nhận chuyển cọc nữa.");
                }
                //Cập nhật trạng thái xác nhận chuyển cọc
                entity.IsXacNhanChuyenCoc = true;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return ResultModel.Success($"Xác nhận chuyển cọc phiếu đăng ký {entity.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[XacNhanChuyenCocPhieuDKAsync] Lỗi khi xác nhận chuyển cọc phiếu đăng ký");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion
    }
}

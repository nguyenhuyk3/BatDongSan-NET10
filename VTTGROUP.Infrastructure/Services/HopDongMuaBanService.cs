using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.HopDongMuaBan;
using VTTGROUP.Domain.Model.KhachHang;
using VTTGROUP.Domain.Model.KhachHangTam;
using VTTGROUP.Domain.Model.PhieuDatCoc;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class HopDongMuaBanService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<HopDongMuaBanService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public HopDongMuaBanService(IDbContextFactory<AppDbContext> factory, ILogger<HopDongMuaBanService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }
        #region Hiển thị danh sách hợp đồng mua bán
        public async Task<(List<HopDongMuaBanPaginDto> Data, int TotalCount)> GetPagingAsync(
       string? maDuAn, int page, int pageSize, string? qSearch, string? trangThai, string fromDate, string toDate)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaDuAn", maDuAn);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);
            param.Add("@TrangThai", trangThai);
            param.Add("@NgayLapFrom", fromDate);
            param.Add("@NgayLapTo", toDate);

            var result = (await connection.QueryAsync<HopDongMuaBanPaginDto>(
                "Proc_HopDong_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa phiếu đặt cọc
        public async Task<ResultModel> SaveHopDongMuaBanAsync(HopDongMuaBanModel model, List<HopDongMuaBanTienDoThanhToanModel> listTDTT)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new KdHopDong
                {
                    MaHopDong = await SinhMaPhieuDCTuDongAsync("HDMB-", _context, 5),
                    SoHopDong = model.SoHopDong,

                    MaKhachHang = model.MaKhachHang ?? string.Empty,
                    IdlanDieuChinhKh = model.IDLanDieuChinhKH,

                    MaDuAn = model.MaDuAn,
                    MaDatCoc = model.MaPhieuDC,
                    MaCanHo = model.MaCanHo,
                    MaChinhSachThanhToan = model.MaChinhSachTT,

                    DienTichTimTuong = model.DienTichTimTuong,
                    DienTichLotLong = model.DienTichLotLong,
                    DienTichSanVuon = model.DienTichSanVuon,

                    GiaDat = model.GiaDat,
                    DonGiaDat = model.DonGiaDat,
                   
                    GiaCanHoTruocThue = model.GiaCanHoTruocThue,
                    TyLeThueVat = model.TyLeThueVAT,
                    GiaCanHoSauThue = model.GiaCanHoSauThue,

                    TyLeCk = model.TyLeCK,
                    GiaTriCk = model.GiaTriCK,
                    GiaBanTruocThue = model.GiaBanTruocThue,
                    GiaBanTienThue = model.GiaBanTienThue,
                    GiaBanSauThue = model.GiaBanSauThue,

                    TyLeQuyBaoTri = model.TyLeQuyBaoTri,
                    TienQuyBaoTri = model.TienQuyBaoTri,

                    MaMauIn = model.MaMauIn,
                    NgayKy = !string.IsNullOrEmpty(model.NgayKy) ? DateTime.ParseExact(model.NgayKy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    NoiDung = model.GhiChu
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;
                await _context.KdHopDongs.AddAsync(record);
                //Insert chính sách thanh toán
                if (listTDTT != null & listTDTT.Any() == true)
                {
                    List<KdHopDongTienDoThanhToan> listCT = new List<KdHopDongTienDoThanhToan>();
                    foreach (var item in listTDTT)
                    {
                        var r = new KdHopDongTienDoThanhToan();
                        r.MaHopDong = record.MaHopDong;
                        r.MaCstt = item.MaCSTT;
                        r.DotTt = item.DotTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.KyThanhToan = item.MaKyTT;
                        r.DotThamChieu = item.DotThamChieu;
                        r.SoKhoangCachNgay = item.SoKhoangCachNgay;
                        r.TyLeTt = item.TyLeThanhToan;
                        r.TyLeVat = item.TyLeThanhToanVAT;
                        r.SoTienTt = item.SoTien;
                        r.SoTienCanTruDaTt = item.SoTienCanTruDaTT;
                        r.SoTienPhaiThanhToan = item.SoTienPhaiThanhToan;
                        listCT.Add(r);
                    }
                    await _context.KdHopDongTienDoThanhToans.AddRangeAsync(listCT);
                }
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = record.MaHopDong ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "HopDongMuaBan",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = string.Empty,
                            TenNhanVien = string.Empty,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }
                    await _context.HtFileDinhKems.AddRangeAsync(listFiles);
                }
                //Insert khách hàng đồng sở hữu
                var delKHDSH = _context.KdHopDongKhachHangs.Where(d => d.MaHopDong == record.MaHopDong);
                _context.KdHopDongKhachHangs.RemoveRange(delKHDSH);
                if (model.ListKHDongSoHuu != null && model.ListKHDongSoHuu.Count > 0)
                {
                    List<KdHopDongKhachHang> listKH = new List<KdHopDongKhachHang>();
                    foreach (var item in model.ListKHDongSoHuu)
                    {
                        var r = new KdHopDongKhachHang();
                        r.MaHopDong = model.MaHopDong;
                        r.MaKhachHang = item.MaKhachHang;
                        r.IdlanDieuChinhKh = item.IDLanDieuChinh;
                        r.IskhdaiDien = item.ISKhachHangDaiDien;
                        listKH.Add(r);
                    }
                    await _context.KdHopDongKhachHangs.AddRangeAsync(listKH);
                }
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaHopDong, "Thêm hợp đồng mua bán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm hợp đồng mua bán: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(HopDongMuaBanModel model, List<HopDongMuaBanTienDoThanhToanModel> listTDTT)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.KdHopDongs.FirstOrDefaultAsync(d => d.MaHopDong.ToLower() == model.MaHopDong.ToLower());
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy hợp đồng mua bán.");
                }
                entity.NgayKy = !string.IsNullOrEmpty(model.NgayKy) ? DateTime.ParseExact(model.NgayKy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                entity.SoHopDong = model.SoHopDong;
                entity.NoiDung = model.GhiChu;
                //Insert chính sách thanh toán
                var delTDTT = _context.KdHopDongTienDoThanhToans.Where(d => d.MaHopDong == entity.MaHopDong);
                if (listTDTT != null & listTDTT.Any() == true)
                {
                    List<KdHopDongTienDoThanhToan> listCT = new List<KdHopDongTienDoThanhToan>();
                    foreach (var item in listTDTT)
                    {
                        var r = new KdHopDongTienDoThanhToan();
                        r.MaHopDong = entity.MaHopDong;
                        r.MaCstt = item.MaCSTT;
                        r.DotTt = item.DotTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.KyThanhToan = item.MaKyTT;
                        r.DotThamChieu = item.DotThamChieu;
                        r.SoKhoangCachNgay = item.SoKhoangCachNgay;
                        r.TyLeTt = item.TyLeThanhToan;
                        r.TyLeVat = item.TyLeThanhToanVAT;
                        r.SoTienTt = item.SoTien;
                        r.SoTienCanTruDaTt = item.SoTienCanTruDaTT;
                        r.SoTienPhaiThanhToan = item.SoTienPhaiThanhToan;
                        listCT.Add(r);
                    }
                    await _context.KdHopDongTienDoThanhToans.AddRangeAsync(listCT);
                }
                _context.KdHopDongTienDoThanhToans.RemoveRange(delTDTT);
                //Insert khách hàng đồng sở hữu
                var delKHDSH = _context.KdHopDongKhachHangs.Where(d => d.MaHopDong == entity.MaHopDong);
                _context.KdHopDongKhachHangs.RemoveRange(delKHDSH);
                if (model.ListKHDongSoHuu != null && model.ListKHDongSoHuu.Count > 0)
                {
                    List<KdHopDongKhachHang> listKH = new List<KdHopDongKhachHang>();
                    foreach (var item in model.ListKHDongSoHuu)
                    {
                        var r = new KdHopDongKhachHang();
                        r.MaHopDong = model.MaHopDong;
                        r.MaKhachHang = item.MaKhachHang;
                        r.IdlanDieuChinhKh = item.IDLanDieuChinh;
                        r.IskhdaiDien = item.ISKhachHangDaiDien;
                        listKH.Add(r);
                    }
                    await _context.KdHopDongKhachHangs.AddRangeAsync(listKH);
                }
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == entity.MaHopDong && d.Controller == "HopDongMuaBan").ToListAsync();

                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;

                        bool exists = UploadedFiles.Any(f =>
                            f.TenFileDinhKem == file.FileName &&
                            f.FileSize == file.FileSize
                        );
                        if (exists)
                            continue;

                        var savedPath = await SaveFileWithTickAsync(file);
                        var f = new HtFileDinhKem
                        {
                            MaPhieu = model.MaHopDong,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "HopDongMuaBan",
                            AcTion = "Edit",
                            NgayLap = DateTime.Now,
                            MaNhanVien = string.Empty,
                            TenNhanVien = string.Empty,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }
                    await _context.HtFileDinhKems.AddRangeAsync(listFiles);
                }
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaHopDong, "Cập nhật hợp đồng mua bán thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật hợp đồng mua bán: {ex.Message.ToString()}");
            }
        }
        public async Task<ResultModel> DeleteHDMBAsync(string maPhieu, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var hdmb = await _context.KdHopDongs.Where(d => d.MaHopDong == maPhieu).FirstOrDefaultAsync();
                if (hdmb == null)
                {
                    return ResultModel.Fail("Không tìm thấy hợp đồng mua bán");
                }
                _context.KdHopDongs.Remove(hdmb);

                var delPTTT = _context.KdHopDongTienDoThanhToans.Where(d => d.MaHopDong == maPhieu);
                _context.KdHopDongTienDoThanhToans.RemoveRange(delPTTT);

                var listFiles = _context.HtFileDinhKems.Where(d => d.Controller == "HopDongMuaBan" && d.MaPhieu == hdmb.MaHopDong);
                if (listFiles != null && listFiles.Any())
                {
                    foreach (var file in listFiles)
                    {
                        var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }

                    _context.HtFileDinhKems.RemoveRange(listFiles);
                }
                var delND = _context.HtDmnguoiDuyets.Where(d => d.MaPhieu == maPhieu);
                _context.HtDmnguoiDuyets.RemoveRange(delND);

                var delKHDSH = _context.KdHopDongKhachHangs.Where(d => d.MaHopDong == maPhieu);
                _context.KdHopDongKhachHangs.RemoveRange(delKHDSH);

                _context.SaveChanges();
                return ResultModel.Success($"Xóa {hdmb.MaHopDong} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePGCAsync] Lỗi khi xóa hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<HopDongMuaBanPaginDto> listHDMB, string webRootPath)
        {
            try
            {
                var ids = listHDMB?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaHopDong))
                    .Select(x => x!.MaHopDong.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B1: Lấy trước danh sách file cần xóa (vật lý) ---
                var filePaths = await _context.HtFileDinhKems
                    .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "HopDongMuaBan")
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.KdHopDongTienDoThanhToans
                    .Where(d => ids.Contains(d.MaHopDong))
                    .ExecuteDeleteAsync();

                var c2 = await _context.HtFileDinhKems
                    .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "HopDongMuaBan")
                    .ExecuteDeleteAsync();

                var c3 = await _context.HtDmnguoiDuyets
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var c4 = await _context.KdHopDongKhachHangs
                   .Where(d => ids.Contains(d.MaHopDong))
                   .ExecuteDeleteAsync();

                var cParent = await _context.KdHopDongs
                    .Where(k => ids.Contains(k.MaHopDong))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                // --- B3: Xóa file vật lý ngoài transaction ---
                int cFile = 0;
                foreach (var relPath in filePaths.Distinct().Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    try
                    {
                        // chuẩn hóa path
                        var clean = relPath!.Trim().TrimStart('/', '\\');
                        var fullPath = Path.Combine(webRootPath, clean);

                        if (File.Exists(fullPath))
                        {
                            File.Delete(fullPath);
                            cFile++;
                        }
                    }
                    catch (Exception exDel)
                    {
                        _logger.LogWarning(exDel, "[DeleteListAsync] Không xóa được file: {RelPath}", relPath);
                        // không throw – tránh rollback DB sau khi đã commit
                    }
                }

                return ResultModel.Success(
                    $"Đã xóa {cParent} phiếu, {c1} tiến độ, {c2} file đính kèm (DB), {c3} người duyệt. " +
                    $"File vật lý xóa: {cFile}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> XacNhanPhieuGiuCho(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var pgc = await _context.BhPhieuGiuChos.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (pgc == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu giữ chỗ");
                }
                pgc.IsxacNhan = true;
                _context.SaveChanges();
                return ResultModel.Success($"Xác nhận {pgc.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[XacNhanPhieuGiuCho] Lỗi khi xác nhận phiếu giữ chỗ");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin hợp đồng mua bán
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await ThongTinChungHDMB(id, string.Empty, string.Empty, string.Empty);
                if (string.IsNullOrEmpty(id))
                {
                    entity = new HopDongMuaBanModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaHopDong = await SinhMaPhieuDCTuDongAsync("HDMB-", _context, 5);
                    entity.ListKHDongSoHuu = new List<KhachHangDongSoHuuHopDong>();
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("HopDongMuaBan", entity.MaHopDong);
                    entity.MaNhanVienDP = ttnd == null ? string.Empty : ttnd.MaNhanVien;
                    entity.TrangThaiDuyetCuoi = await _baseService.BuocDuyetCuoi(entity.MaQuiTrinhDuyet);
                    if (entity.TrangThaiDuyet == 0 && entity.NguoiLap != null && entity.NguoiLap.MaNhanVien == _currentUser.MaNhanVien)
                    {
                        entity.FlagTong = true;
                    }
                    else if (entity.MaNhanVienDP == _currentUser.MaNhanVien && entity.TrangThaiDuyet != entity.TrangThaiDuyetCuoi)
                    {
                        entity.FlagTong = true;
                    }
                    entity.ListKHDongSoHuu = await (from hdkh in _context.KdHopDongKhachHangs
                                                    join kh in _context.KhDmkhachHangs on hdkh.MaKhachHang equals kh.MaKhachHang
                                                    join ct in _context.KhDmkhachHangChiTiets on hdkh.MaKhachHang equals ct.MaKhachHang
                                                    where hdkh.MaHopDong == id && hdkh.IdlanDieuChinhKh == ct.IdlanDieuChinh
                                                    select new KhachHangDongSoHuuHopDong
                                                    {
                                                        MaKhachHang = hdkh.MaKhachHang,
                                                        TenKhachHang = kh.TenKhachHang,
                                                        IdCard = ct.IdCard ?? string.Empty,
                                                        SoDienThoai = ct.SoDienThoai,
                                                        Email = ct.Email ?? string.Empty,
                                                        DiaChiThuongTru = ct.DiaChiThuongTru ?? string.Empty,
                                                        DiaChiLienLac = ct.DiaChiLienLac ?? string.Empty,
                                                        ISKhachHangDaiDien = hdkh.IskhdaiDien ?? false,
                                                        IDLanDieuChinh = hdkh.IdlanDieuChinhKh ?? string.Empty,
                                                        NguoiDaiDien = ct.NguoiDaiDien ?? string.Empty,
                                                        ChucVuNguoiDaiDien = ct.ChucVuNguoiDaiDien ?? string.Empty,
                                                        NguoiLienHe = ct.NguoiLienHe ?? string.Empty,
                                                        SoDienThoaiNguoiLienHe = ct.SoDienThoaiNguoiLienHe ?? string.Empty
                                                    }).ToListAsync();
                    var files = await _context.HtFileDinhKems.Where(d => d.Controller == "HopDongMuaBan" && d.MaPhieu == id).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType
                    }).ToListAsync();
                    entity.Files = files;
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
                param.Add("@MaChinhSachTT", maChinhSacTT);
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

        public async Task<KhachHangPhieuDatCoc> ThongTinKhachHang(string maHopDong, string maPhieuDatCoc, string maDuAn, string maChinhSacTT)
        {
            var khachHang = new KhachHangPhieuDatCoc();
            var entity = new PhieuDatCocModel();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaHopDong", maHopDong);
                param.Add("@MaPhieuDatCoc", maPhieuDatCoc);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaChinhSachTT", maChinhSacTT);
                entity = (await connection.QueryAsync<PhieuDatCocModel>(
                "Proc_HopDongMuaBan_ThongTinChung",
                param,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
                if (entity != null)
                {
                    khachHang.MaKhachHang = entity.MaKhachHang;
                    khachHang.TenKhachHang = entity.TenKhachHang;
                    khachHang.NgaySinh = entity.NgaySinh == null ? string.Empty : string.Format("{0:dd/MM/yyyy}", entity.NgaySinh);
                    khachHang.SoDienThoai = entity.SoDienThoai;
                    khachHang.IdCard = entity.IDCard;
                    khachHang.NgayCapIdCard = entity.NgayCap == null ? string.Empty : string.Format("{0:dd/MM/yyyy}", entity.NgayCap);
                    khachHang.NoiCapIdCard = entity.NoiCap;
                    khachHang.DiaChiThuongTru = entity.DiaChiThuongTru;
                    khachHang.DiaChiHienNay = entity.DiaChiHienNay;
                    khachHang.NguoiDaiDien = entity.NguoiDaiDien;
                    khachHang.ChucVuNguoiDaiDien = entity.ChucVuNguoiDaiDien;
                    khachHang.SoDienThoaiNguoiLienHe = entity.SoDienThoaiNguoiLienHe;
                    khachHang.NguoiLienHe = entity.NguoiLienHe;
                    khachHang.MaCanHo = entity.MaCanHo;
                    khachHang.TenCanHo = entity.TenCanHo;
                    khachHang.MaChinhSachTT = entity.MaChinhSachTT;
                    khachHang.TenCSTT = entity.TenChinhSachTT;
                    khachHang.IDKhachHangCT = entity.IDKhachHangCT;
                    khachHang.DienTichTimTuong = entity.DienTichTimTuong;
                    khachHang.DienTichSanVuon = entity.DienTichSanVuon;
                    khachHang.DienTichLotLong = entity.DienTichLotLong;
                    khachHang.GiaDat = entity.GiaDat;
                    khachHang.DonGiaDat = entity.DonGiaDat;
                    khachHang.GiaCanHoTruocThue = entity.GiaCanHoTruocThue;                  
                    khachHang.TyLeThueVAT = entity.TyLeThueVAT;
                    khachHang.GiaCanHoSauThue = entity.GiaCanHoSauThue;
                    khachHang.TyLeCK = entity.TyLeCK;
                    khachHang.GiaTriCK = entity.GiaTriCK;
                    khachHang.GiaBanTruocThue = entity.GiaBanTruocThue;
                    khachHang.GiaBanTienThue = entity.GiaBanTienThue;
                    khachHang.GiaBanSauThue = entity.GiaBanSauThue;
                    khachHang.TyLeQuyBaoTri = entity.TyLeQuyBaoTri;
                    khachHang.TienQuyBaoTri = entity.TienQuyBaoTri;
                    khachHang.Email = entity.Email;
                }
            }
            catch
            {
                khachHang = new KhachHangPhieuDatCoc();
            }
            return khachHang;
        }

        public async Task<List<HopDongMuaBanTienDoThanhToanModel>> GetByTienDoThanhToanAsync(string maCSTT, string maHopDong, string maPhieuDatCoc)
        {
            var entity = new List<HopDongMuaBanTienDoThanhToanModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaChinhSachTT", maCSTT);
                param.Add("@MaHopDong", maHopDong);
                param.Add("@MaPhieuDatCoc", maPhieuDatCoc);
                entity = (await connection.QueryAsync<HopDongMuaBanTienDoThanhToanModel>(
                    "Proc_HopDongMuaBan_TienDoThanhToan",
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
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách tiến độ thanh toán hợp đồng mua bán");
            }
            return entity;
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu giữ chỗ   
        public async Task<string> SinhMaPhieuDCTuDongAsync(string prefix, AppDbContext _context, int padding = 5)
        {
            _context.ChangeTracker.Clear();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KdHopDongs
                .Where(kh => kh.MaHopDong.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaHopDong)
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

        public async Task<string> SinhTuDonSoPhieuAsync(string maDuAn, string maDot)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                int coutDot = (await _context.BhPhieuGiuChos.Where(d => d.MaDuAn == maDuAn && d.DotMoBan == maDot).CountAsync()) + 1;
                string maMoi = maDuAn + "-" + maDot + "-" + coutDot.ToString();
                return maMoi;
            }
            catch
            {
                return string.Empty;
            }

        }
        #endregion

        #region Load danh sách combobox

        public async Task<List<DaDanhMucDuAn>> GetByDuAnAsync()
        {
            var entity = new List<DaDanhMucDuAn>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucDuAns.ToListAsync();
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
        public async Task<List<PhieuDangKyDatCocCSTTModel>> GetChinhSachThanhToanAsync(string maDuAn, string maPhieuDK)
        {
            var entity = new List<PhieuDangKyDatCocCSTTModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaPhieu", maPhieuDK);

                entity = (await connection.QueryAsync<PhieuDangKyDatCocCSTTModel>(
                    "Proc_HopDong_ChinhSachThanhToan",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<PhieuDangKyDatCocCSTTModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách chính sách thanh toán");
            }
            return entity;
        }

        public async Task<List<PhieuDatCocChuaLenHDModel>> GetByPhieuDatCocChuaLenHDAsync(string maDuAn)
        {
            var entity = new List<PhieuDatCocChuaLenHDModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);

                entity = (await connection.QueryAsync<PhieuDatCocChuaLenHDModel>(
                    "Proc_HopDong_ChonPhieuDatCoc",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<PhieuDatCocChuaLenHDModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách phiếu đăn ký chưa lên phiếu đặt cọc");
            }
            return entity;
        }

        public async Task<List<HtMauIn>> GetByMauInAsync(string maDuAn)
        {
            var entity = new List<HtMauIn>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.HtMauIns.Where(d => d.MaDuAn == maDuAn && d.LoaiMauIn == "HD").ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtMauIn>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách mẫu in");
            }
            return entity;
        }

        public async Task<PhieuDatCocChuaLenHDModel> GetByPhieuDatCocChuaLenHopDongAsync(string maPhieuDC)
        {
            var entity2 = new PhieuDatCocChuaLenHDModel();
            try
            {
                using var _context = _factory.CreateDbContext();
                string maDuAn = await _context.BhPhieuDatCocs.Where(d => d.MaPhieuDc == maPhieuDC).Select(d => d.MaDuAn).FirstOrDefaultAsync() ?? string.Empty;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);

                var entity = (await connection.QueryAsync<PhieuDatCocChuaLenHDModel>(
                       "Proc_HopDong_ChonPhieuDatCoc",
                       param,
                       commandType: CommandType.StoredProcedure
                   )).ToList();
                entity2 = entity.Where(d => d.MaPhieuDC == maPhieuDC).FirstOrDefault();
                if (entity2 == null)
                {
                    entity2 = new PhieuDatCocChuaLenHDModel();
                }
                entity2.MaDuAn = maDuAn;
                entity2.TenDuAn = await _context.DaDanhMucDuAns.Where(d => d.MaDuAn == maDuAn).Select(d => d.TenDuAn).FirstOrDefaultAsync() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách phiếu đăn ký chưa lên phiếu đặt cọc");
            }
            return entity2;
        }
        #endregion

        #region Thông tin khách hàng
        public async Task<(List<KhachHangPagingDto> Data, int TotalCount)> GetPagingKhachHangPopupAsync(
     string? loaiHinh, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@LoaiHinh", !string.IsNullOrEmpty(loaiHinh) ? loaiHinh : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<KhachHangPagingDto>(
                "Proc_PhieuGC_KhachHangPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<List<KhDmdoiTuongKhachHang>> GetByLoaiKhachHangPopupAsync()
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<KhDmdoiTuongKhachHang>();
            try
            {
                entity = await _context.KhDmdoiTuongKhachHangs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<KhDmdoiTuongKhachHang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại khách hàng");
            }
            return entity;
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

        #region Xác nhận ngày ký khi đã duyệt phiếu
        //public async Task<ResultModel> XacNhanNgayKyAsync(HopDongMuaBanModel model)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var entity = await _context.KdHopDongs.FirstOrDefaultAsync(d => d.MaHopDong.ToLower() == model.MaHopDong.ToLower());
        //        if (entity == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy hợp đồng mua bán.");
        //        }
        //        var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
        //        entity.NgayKy = !string.IsNullOrEmpty(model.NgayKy) ? DateTime.ParseExact(model.NgayKy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
        //        entity.IsDaKy = model.IsDaKy;
        //        entity.NgayXacNhan = DateTime.Now;
        //        entity.NguoiXacNhan = NguoiLap.MaNhanVien;
        //        List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
        //        var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == entity.MaHopDong && d.Controller == "HopDongMuaBan").ToListAsync();

        //        if (model.Files != null && model.Files.Any())
        //        {
        //            foreach (var file in model.Files)
        //            {
        //                if (string.IsNullOrEmpty(file.FileName)) continue;

        //                bool exists = UploadedFiles.Any(f =>
        //                    f.TenFileDinhKem == file.FileName &&
        //                    f.FileSize == file.FileSize
        //                );
        //                if (exists)
        //                    continue;

        //                var savedPath = await SaveFileWithTickAsync(file);
        //                var f = new HtFileDinhKem
        //                {
        //                    MaPhieu = entity.MaHopDong,
        //                    TenFileDinhKem = file.FileName,
        //                    TenFileDinhKemLuu = savedPath,
        //                    TaiLieuUrl = savedPath,
        //                    Controller = "HopDongMuaBan",
        //                    AcTion = "Edit",
        //                    NgayLap = DateTime.Now,
        //                    MaNhanVien = string.Empty,
        //                    TenNhanVien = string.Empty,
        //                    FileSize = file.FileSize,
        //                    FileType = file.ContentType,
        //                    FullDomain = file.FullDomain,
        //                };
        //                listFiles.Add(f);
        //            }
        //            await _context.HtFileDinhKems.AddRangeAsync(listFiles);
        //        }
        //        if (entity.IsDaKy == true)
        //        {
        //            string flagCNPT = await TaoPhieuCongNoPhaiThuBDSAsync(entity, _context);
        //            if (string.IsNullOrEmpty(flagCNPT))
        //            {
        //                await _baseService.TaoCongNoPTERP(entity.MaHopDong, entity.NgayKy, "", entity.MaDuAn);                      
        //            }
        //            else//Roll back lại khi bị lỗi
        //            {
        //                using var _context2 = _factory.CreateDbContext();
        //                var delPKBL = _context2.KtPhieuCongNoPhaiThus.Where(d => d.MaChungTu == model.MaHopDong);
        //                var entityCN = await _context2.KdHopDongs.FirstOrDefaultAsync(d => d.MaHopDong.ToLower() == model.MaHopDong.ToLower());
        //                entity.IsDaKy = false;
        //                _context2.KtPhieuCongNoPhaiThus.RemoveRange(delPKBL);
        //                _context2.SaveChanges();
        //                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật ngày ký hợp đồng mua bán: {flagCNPT}");
        //            }
        //            await _context.SaveChangesAsync();
        //            return ResultModel.SuccessWithId(entity.MaHopDong, "Cập nhật ngày ký hợp đồng mua bán thành công");
        //        }
        //        else
        //        {
        //            await _context.SaveChangesAsync();
        //            return ResultModel.SuccessWithId(entity.MaHopDong, "Cập nhật ngày ký hợp đồng mua bán thành công");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Lỗi khi cập nhật hợp đồng mua bán");
        //        return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật hợp đồng mua bán: {ex.Message.ToString()}");
        //    }
        //}

        public async Task<ResultModel> XacNhanNgayKyAsync(HopDongMuaBanModel model, CancellationToken ct = default)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.MaHopDong))
                return ResultModel.Fail("Thiếu thông tin hợp đồng.");

            // Stage các file đã lưu vật lý để có thể dọn nếu transaction fail
            var stagedFiles = new List<(string SavedPath, string OriginalName, string Size, string ContentType, string? FullDomain)>();

            await using var context = _factory.CreateDbContext();
            await using var tran = await context.Database.BeginTransactionAsync(ct);

            try
            {
                context.ChangeTracker.Clear();

                // 1) Load entity
                var maHD = model.MaHopDong.Trim();
                var entity = await context.KdHopDongs
                    .FirstOrDefaultAsync(d => d.MaHopDong.ToUpper() == maHD.ToUpper(), ct);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy hợp đồng mua bán.");

                // 2) Parse ngày ký an toàn (nullable)
                DateTime? ngayKy = null;
                if (!string.IsNullOrWhiteSpace(model.NgayKy))
                {
                    if (DateTime.TryParseExact(model.NgayKy.Trim(), "dd/MM/yyyy",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    {
                        ngayKy = parsed;
                    }
                    else
                    {
                        return ResultModel.Fail("Định dạng ngày ký không hợp lệ (dd/MM/yyyy).");
                    }
                }

                // 3) Lấy file đã upload trước đó (để chống trùng theo Tên + Size)
                var uploadedFiles = await context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.MaPhieu == entity.MaHopDong && d.Controller == "HopDongMuaBan")
                    .Select(d => new { d.TenFileDinhKem, d.FileSize }) // FileSize là string
                    .ToListAsync(ct);

                // 4) Stage file mới lên đĩa (nếu có)
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (file == null || string.IsNullOrWhiteSpace(file.FileName))
                            continue;

                        bool exists = uploadedFiles.Any(f =>
                            string.Equals(f.TenFileDinhKem, file.FileName, StringComparison.OrdinalIgnoreCase)
                            && f.FileSize == file.FileSize // so sánh string-string
                        );
                        if (exists) continue;

                        var savedPath = await SaveFileWithTickAsync(file); // trả về path đã lưu (tương đối)
                        stagedFiles.Add((savedPath,
                                         file.FileName,
                                         file.FileSize,                    // string, khớp entity
                                         file.ContentType ?? string.Empty, // tránh null
                                         file.FullDomain));
                    }
                }

                // 5) Cập nhật entity
                var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                entity.NgayKy = ngayKy;
                entity.IsDaKy = model.IsDaKy;
                entity.NgayXacNhan = DateTime.Now;
                entity.NguoiXacNhan = nguoiLap?.MaNhanVien;

                // 6) Ghi metadata file vào DB nếu có stage
                if (stagedFiles.Count > 0)
                {
                    var newFileEntities = stagedFiles.Select(sf => new HtFileDinhKem
                    {
                        MaPhieu = entity.MaHopDong,
                        TenFileDinhKem = sf.OriginalName,
                        TenFileDinhKemLuu = sf.SavedPath,
                        TaiLieuUrl = sf.SavedPath,
                        Controller = "HopDongMuaBan",
                        AcTion = "Edit",
                        NgayLap = DateTime.Now,
                        MaNhanVien = nguoiLap?.MaNhanVien ?? string.Empty,
                        TenNhanVien = nguoiLap?.HoVaTen ?? string.Empty,
                        FileSize = sf.Size,       // string
                        FileType = sf.ContentType,
                        FullDomain = sf.FullDomain
                    }).ToList();

                    await context.HtFileDinhKems.AddRangeAsync(newFileEntities, ct);
                }

                // 7) Nếu ĐÃ KÝ => tạo công nợ nội bộ (cùng transaction)
                if (entity.IsDaKy == true)
                {
                    string flagCNPT = await TaoPhieuCongNoPhaiThuBDSAsync(entity, context);
                    if (!string.IsNullOrEmpty(flagCNPT))
                    {
                        await tran.RollbackAsync(ct);
                        CleanupStagedFilesSafe(stagedFiles);
                        return ResultModel.Fail($"Không thể cập nhật ngày ký HĐMB: {flagCNPT}");
                    }
                }

                // 8) Commit dữ liệu nội bộ
                await context.SaveChangesAsync(ct);
                await tran.CommitAsync(ct);

                // 9) Sau COMMIT: đồng bộ ERP (best-effort, không làm hỏng dữ liệu nếu lỗi)
                if (entity.IsDaKy == true)
                {
                    try
                    {
                        await _baseService.TaoCongNoPTERP(entity.MaHopDong, entity.NgayKy, "", entity.MaDuAn);
                    }
                    catch (Exception exErp)
                    {
                        _logger.LogWarning(exErp, "[XacNhanNgayKyAsync][HĐMB] ERP sync failed for {MaHD}", entity.MaHopDong);
                        return ResultModel.SuccessWithId(entity.MaHopDong,
                            "Cập nhật ngày ký thành công, nhưng đồng bộ ERP thất bại. Vui lòng thử lại sau.");
                    }
                }

                return ResultModel.SuccessWithId(entity.MaHopDong, "Cập nhật ngày ký hợp đồng mua bán thành công");
            }
            catch (Exception ex)
            {
                try { await tran.RollbackAsync(ct); } catch { /* ignore */ }
                CleanupStagedFilesSafe(stagedFiles);
                _logger.LogError(ex, "[XacNhanNgayKyAsync][HĐMB] Lỗi khi cập nhật ngày ký");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật hợp đồng mua bán: {ex.Message}");
            }

            // ===== LOCAL HELPERS =====
            static void CleanupStagedFilesSafe(IEnumerable<(string SavedPath, string OriginalName, string Size, string ContentType, string? FullDomain)> files)
            {
                foreach (var f in files)
                {
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(f.SavedPath) && System.IO.File.Exists(f.SavedPath))
                            System.IO.File.Delete(f.SavedPath);
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }
            }
        }


        public async Task<string> TaoPhieuCongNoPhaiThuBDSAsync(KdHopDong hd, AppDbContext context)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaPhieu", hd.MaHopDong);
                param.Add("@NgayKy", hd.NgayKy);
                param.Add("@MaCongViec", "HopDongMuaBan");

                var result = (await connection.QueryAsync<CongNoPhaiThuModel>(
                    "Proc_TaoPhieuCongNoBDS",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                //KtPhieuCongNoPhaiThu r;
                //foreach (var item in result)
                //{
                //    r = new KtPhieuCongNoPhaiThu();//Trường hợp ở đây MaPhieu đã tự tăng trong Seq_PCNPT rùi nha
                //    r.MaPhieu = await SinhMaPhieuCNPTTuDongAsync("PCNPT-", context, 5);
                //    r.NgayLap = DateTime.Now;
                //    r.DuAn = item.DuAn;
                //    r.MaChungTu = item.MaChungTu;
                //    r.IdChungTu = item.IdChungTu;
                //    r.NoiDung = item.NoiDung;
                //    r.HanThanhToan = item.HanThanhToan;
                //    r.MaKhachHang = item.MaKhachHang;
                //    r.TenKhachHang = item.TenKhachHang;
                //    r.SoTien = item.SoTien;
                //    r.MaCongViec = item.MaCongViec;
                //    r.MaDoiTuong = item.MaKhachHang;
                //    await context.KtPhieuCongNoPhaiThus.AddAsync(r);
                //    context.SaveChanges();
                //}
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }

        public async Task<string> SinhMaPhieuCNPTTuDongAsync(string prefix, AppDbContext context, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KtPhieuCongNoPhaiThus
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
    }
}

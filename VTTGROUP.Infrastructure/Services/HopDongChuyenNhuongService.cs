using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.HopDongChuyenNhuong;
using VTTGROUP.Domain.Model.KhachHang;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class HopDongChuyenNhuongService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<HopDongChuyenNhuongService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public HopDongChuyenNhuongService(IDbContextFactory<AppDbContext> factory, ILogger<HopDongChuyenNhuongService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách hợp đồng chuyển nhượng
        public async Task<(List<HopDongChuyenNhuongPagingDto> Data, int TotalCount)> GetPagingAsync(
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

            var result = (await connection.QueryAsync<HopDongChuyenNhuongPagingDto>(
                "Proc_KD_ChuyenNhuong_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa hợp đồng chuyển nhượng
        public async Task<ResultModel> SavePhieuAsync(HopDongChuyenNhuongModel model, List<HopDongChuyenNhuongKhachHangDto> listKH)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new KdChuyenNhuong
                {
                    MaChuyenNhuong = await SinhMaPhieuDCTuDongAsync("HDCN-", 5),
                    MaDuAn = model.MaDuAn,
                    MaHopDong = model.MaHopDong,
                    MaSanPham = model.MaCanHo,
                    GiaTriCanHo = model.GiaTriCanHo,
                    GiaTriDaThanhToan = model.GiaTriDaThanhToan,
                    PhiBaoTri = model.PhiBaoTri,
                    PhiBaoTriDaTt = model.PhiBaoTriDaThanhToan,
                    NgayChuyenNhuong = !string.IsNullOrEmpty(model.NgayChuyenNhuong) ? DateTime.ParseExact(model.NgayChuyenNhuong, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    NoiDung = model.GhiChu
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;
                await _context.KdChuyenNhuongs.AddAsync(record);
                //Insert khách hàng nhận chuyển nhượng
                if (listKH != null & listKH.Any() == true)
                {
                    List<KdChuyenNhuongKhachHang> listCT = new List<KdChuyenNhuongKhachHang>();
                    foreach (var item in listKH)
                    {
                        var r = new KdChuyenNhuongKhachHang();
                        r.MaChuyenNhuong = record.MaChuyenNhuong;
                        r.MaKhachHang = item.MaKhachHang;
                        r.IdlanDieuChinhKh = item.IDLanDieuChinhKH;
                        r.VaiTro = item.VaiTro;
                        r.Stt = item.STT;
                        r.IsDaiDien = item.IsDaiDien;
                        listCT.Add(r);
                    }
                    await _context.KdChuyenNhuongKhachHangs.AddRangeAsync(listCT);
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
                            MaPhieu = record.MaChuyenNhuong ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "HopDongChuyenNhuong",
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
                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaChuyenNhuong, "Thêm hợp đồng chuyển nhượng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm hợp đồng mua bán");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm hợp đồng chuyển nhượng: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateByIdAsync(HopDongChuyenNhuongModel model, List<HopDongChuyenNhuongKhachHangDto> listKH)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.KdChuyenNhuongs.FirstOrDefaultAsync(d => d.MaChuyenNhuong.ToLower() == model.MaPhieu.ToLower());
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy hợp đồng chuyển nhượng.");
                }
                entity.NgayChuyenNhuong = !string.IsNullOrEmpty(model.NgayChuyenNhuong) ? DateTime.ParseExact(model.NgayChuyenNhuong, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                entity.NoiDung = model.GhiChu;
                //Insert khách hàng nhận chuyển nhượng
                var delKHDSH = _context.KdChuyenNhuongKhachHangs.Where(d => d.MaChuyenNhuong == entity.MaChuyenNhuong);
                _context.KdChuyenNhuongKhachHangs.RemoveRange(delKHDSH);
                //Insert khách hàng nhận chuyển nhượng
                if (listKH != null & listKH.Any() == true)
                {
                    List<KdChuyenNhuongKhachHang> listCT = new List<KdChuyenNhuongKhachHang>();
                    foreach (var item in listKH)
                    {
                        var r = new KdChuyenNhuongKhachHang();
                        r.MaChuyenNhuong = entity.MaChuyenNhuong;
                        r.MaKhachHang = item.MaKhachHang;
                        r.IdlanDieuChinhKh = item.IDLanDieuChinhKH;
                        r.VaiTro = item.VaiTro;
                        r.Stt = item.STT;
                        r.IsDaiDien = item.IsDaiDien;
                        listCT.Add(r);
                    }
                    await _context.KdChuyenNhuongKhachHangs.AddRangeAsync(listCT);
                }
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == entity.MaChuyenNhuong && d.Controller == "HopDongChuyenNhuong").ToListAsync();

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
                            MaPhieu = model.MaPhieu,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "HopDongChuyenNhuong",
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
                return ResultModel.SuccessWithId(entity.MaChuyenNhuong, "Cập nhật hợp đồng chuyển nhượng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hợp đồng chuyển nhượng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật hợp đồng chuyển nhượng: {ex.Message.ToString()}");
            }
        }
        public async Task<ResultModel> DeleteHDMBAsync(string maPhieu, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var hdmb = await _context.KdChuyenNhuongs.Where(d => d.MaChuyenNhuong == maPhieu).FirstOrDefaultAsync();
                if (hdmb == null)
                {
                    return ResultModel.Fail("Không tìm thấy hợp đồng chuyển nhượng");
                }
                _context.KdChuyenNhuongs.Remove(hdmb);

                var delPTTT = _context.KdChuyenNhuongKhachHangs.Where(d => d.MaChuyenNhuong == maPhieu);
                _context.KdChuyenNhuongKhachHangs.RemoveRange(delPTTT);

                var listFiles = _context.HtFileDinhKems.Where(d => d.Controller == "HopDongChuyenNhuong" && d.MaPhieu == hdmb.MaChuyenNhuong);
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

                _context.SaveChanges();
                return ResultModel.Success($"Xóa {hdmb.MaChuyenNhuong} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePGCAsync] Lỗi khi xóa hợp đồng chuyển nhượng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<HopDongChuyenNhuongPagingDto> listHDCN)
        {
            try
            {
                var ids = listHDCN?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaChuyenNhuong))
                    .Select(x => x!.MaChuyenNhuong.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.KdChuyenNhuongKhachHangs
                    .Where(d => ids.Contains(d.MaChuyenNhuong))
                    .ExecuteDeleteAsync();

                var c3 = await _context.HtDmnguoiDuyets
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var cParent = await _context.KdChuyenNhuongs
                    .Where(k => ids.Contains(k.MaChuyenNhuong))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success("Đã xóa hợp đồng chuyển nhượng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách hợp đồng chuyển nhượng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin phiếu
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (from cn in _context.KdChuyenNhuongs
                                    join hd in _context.KdHopDongs on cn.MaHopDong equals hd.MaHopDong
                                    join kh in _context.KhDmkhachHangs on hd.MaKhachHang equals kh.MaKhachHang into dtKH
                                    from kh2 in dtKH.DefaultIfEmpty()
                                    join da in _context.DaDanhMucDuAns on cn.MaDuAn equals da.MaDuAn into dtDuAn
                                    from da2 in dtDuAn.DefaultIfEmpty()
                                    join sp in _context.DaDanhMucSanPhams on cn.MaSanPham equals sp.MaSanPham into dtSP
                                    from sp2 in dtSP.DefaultIfEmpty()
                                    where cn.MaChuyenNhuong == id
                                    select new HopDongChuyenNhuongModel
                                    {
                                        MaPhieu = cn.MaChuyenNhuong,
                                        NgayLap = cn.NgayLap ?? DateTime.Now,
                                        NgayChuyenNhuong = string.Format("{0:dd/MM/yyyy}", cn.NgayChuyenNhuong),
                                        MaHopDong = cn.MaHopDong ?? string.Empty,
                                        MaDuAn = hd.MaDuAn ?? string.Empty,
                                        TenDuAn = da2.TenDuAn,
                                        MaCanHo = hd.MaCanHo ?? string.Empty,
                                        TenCanHo = sp2.TenSanPham,
                                        GhiChu = cn.NoiDung,
                                        MaQuiTrinhDuyet = cn.MaQuiTrinhDuyet ?? 0,
                                        TrangThaiDuyet = cn.TrangThaiDuyet ?? 0,
                                        GiaTriCanHo = cn.GiaTriCanHo,
                                        GiaTriDaThanhToan = cn.GiaTriDaThanhToan ?? 0,
                                        PhiBaoTri = cn.PhiBaoTri ?? 0,
                                        PhiBaoTriDaThanhToan = cn.PhiBaoTriDaTt ?? 0,
                                        MaKhachHang = hd.MaKhachHang,
                                        TenKhachHang = kh2.TenKhachHang,
                                        MaNhanVien = cn.NguoiLap,
                                    }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new HopDongChuyenNhuongModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.MaPhieu = await SinhMaPhieuDCTuDongAsync("HDCN-", 5);
                    entity.NgayLap = DateTime.Now;
                    entity.GiaTriCanHo = 0;
                    entity.GiaTriDaThanhToan = 0;
                    entity.PhiBaoTri = 0;
                    entity.PhiBaoTriDaThanhToan = 0;
                    entity.NgayChuyenNhuong = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("HopDongChuyenNhuong", entity.MaPhieu);
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
                    var files = await _context.HtFileDinhKems.Where(d => d.Controller == "HopDongChuyenNhuong" && d.MaPhieu == id).Select(d => new UploadedFileModel
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
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu hợp đồng chuyển nhượng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
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
        #endregion

        #region Hàm tăng tự động của hợp đồng chuyển nhượng   
        public async Task<string> SinhMaPhieuDCTuDongAsync(string prefix, int padding = 5)
        {
            using var _context = _factory.CreateDbContext();
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KdChuyenNhuongs
                .Where(kh => kh.MaChuyenNhuong.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap) // vẫn ổn nếu phần số padded
                .Select(kh => kh.MaChuyenNhuong)
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

        #region Thông tin hợp đồng mua bán
        public async Task<(List<HopDongChuyenNhuongMuaBanPagingDto> Data, int TotalCount)> GetPagingHopDongPopupAsync(string maDuAn, int page, int pageSize, string? qSearch)
        {
            var result = new List<HopDongChuyenNhuongMuaBanPagingDto>();
            try
            {
                qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@MaDuAn", maDuAn);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                result = (await connection.QueryAsync<HopDongChuyenNhuongMuaBanPagingDto>(
                    "Proc_HopDongChuyenNhuong_ChonHopDong",
                   param,
                   commandType: CommandType.StoredProcedure
               )).ToList();
                int total = result.FirstOrDefault()?.TotalCount ?? 0;
                return (result, total);
            }
            catch (Exception ex)
            {
                result = new List<HopDongChuyenNhuongMuaBanPagingDto>();
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu hợp đồng chuyển nhượng");
                return (result, 0);
            }
        }
        #endregion

        #region Thông tin khách hàng chuyển nhượng và nhận chuyển nhượng
        public async Task<List<HopDongChuyenNhuongKhachHangDto>> GetByKhachHangNhanChuyenNhuongAsync(string maHopDong, string maChuyenNhuong)
        {
            var entity = new List<HopDongChuyenNhuongKhachHangDto>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaHopDong", maHopDong);
                param.Add("@MaChuyenNhuong", maChuyenNhuong);

                entity = (await connection.QueryAsync<HopDongChuyenNhuongKhachHangDto>(
                    "Proc_KD_ChuyenNhuong_GetBenChuyen_ByHopDong",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<HopDongChuyenNhuongKhachHangDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách tiến khách hàng chuyển nhượng");
            }
            return entity;
        }

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
    }
}

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.ThanhLyHopDong;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ThanhLyHopDongService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<ThanhLyHopDongService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public ThanhLyHopDongService(AppDbContext context, ILogger<ThanhLyHopDongService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }


        #region Hiển thị danh sách thanh lý hợp đồng
        public async Task<(List<ThanhLyHopDongPagingDto> Data, int TotalCount)> GetPagingAsync(
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

            var result = (await connection.QueryAsync<ThanhLyHopDongPagingDto>(
                "Proc_ThanhLyHopDong_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thông tin phiếu
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                var entity = await (from tl in _context.KdThanhLyHopDongs
                                    join hd in _context.KdHopDongs on tl.MaHopDong equals hd.MaHopDong
                                    join kh in _context.KhDmkhachHangs on hd.MaKhachHang equals kh.MaKhachHang into dtKH
                                    from kh2 in dtKH.DefaultIfEmpty()
                                    join da in _context.DaDanhMucDuAns on hd.MaDuAn equals da.MaDuAn into dtDuAn
                                    from da2 in dtDuAn.DefaultIfEmpty()
                                    join sp in _context.DaDanhMucSanPhams on hd.MaCanHo equals sp.MaSanPham into dtSP
                                    from sp2 in dtSP.DefaultIfEmpty()
                                    join ptl in _context.BhHinhThucThanhLies
                                        on tl.HinhThucThanhLy equals ptl.MaHttl into g
                                    from ptl in g.DefaultIfEmpty()
                                    where tl.MaPhieuTl == id
                                    select new ThanhLyHopDongModel
                                    {
                                        MaPhieu = tl.MaPhieuTl,
                                        SoPhieuThanhLy = tl.SoPhieuTl ?? string.Empty,
                                        NgayLap = tl.NgayLap ?? DateTime.Now,
                                        NgayThanhLy = string.Format("{0:dd/MM/yyyy}", tl.NgayThanhLy),                                    
                                        NguoiThanhLy = tl.NguoiThanhLy,
                                        MaHopDong = tl.MaHopDong ?? string.Empty,
                                        HinhThucThanhLy = tl.HinhThucThanhLy,
                                        TenHinhThucThanhLy = ptl.TenHttl ?? string.Empty,
                                        MaDuAn = hd.MaDuAn ?? string.Empty,
                                        TenDuAn = da2.TenDuAn,
                                        MaCanHo = hd.MaCanHo ?? string.Empty,
                                        TenCanHo = sp2.TenSanPham,
                                        GhiChu = tl.NoiDung,
                                        MaQuiTrinhDuyet = tl.MaQuiTrinhDuyet ?? 0,
                                        TrangThaiDuyet = tl.TrangThaiDuyet ?? 0,
                                        GiaBan = hd.GiaBanSauThue,
                                        SoTienDaThu = tl.SoTienDaThu ?? 0,
                                        SoTienPhiBaoTriDaThu = tl.SoTienPhiBaoTriDaThu ?? 0,
                                        TyLeViPham = tl.TyLeViPham ?? 0,
                                        SoTienViPhamHopDong = tl.SoTienViPhamHopDong ?? 0,
                                        SoTienHoanTra = tl.SoTienHoanTra ?? 0,
                                        MaKhachHang = hd.MaKhachHang,
                                        TenKhachHang = kh2.TenKhachHang,
                                    }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new ThanhLyHopDongModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaPhieu = await SinhMaPhieuTuDongAsync("TLHD-", 5);
                    entity.GiaBan = 0;
                    entity.SoTienHoanTra = 0;
                    entity.NgayThanhLy = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.NguoiThanhLy);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("ThanhLyHopDong", entity.MaPhieu);
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
                    var files = await _context.HtFileDinhKems.Where(d => d.Controller == "ThanhLyHopDong" && d.MaPhieu == id).Select(d => new UploadedFileModel
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
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu thanh lý hợp đồng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thêm, xóa , sửa
        public async Task<ResultModel> SavePhieuAsync(ThanhLyHopDongModel model)
        {
            try
            {
                var record = new KdThanhLyHopDong
                {
                    MaPhieuTl = await SinhMaPhieuTuDongAsync("TLHD-", 5),
                    SoPhieuTl = model.SoPhieuThanhLy,
                    MaHopDong = model.MaHopDong,
                    NgayThanhLy = !string.IsNullOrEmpty(model.NgayThanhLy) ? DateTime.ParseExact(model.NgayThanhLy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now,
                    NoiDung = model.GhiChu,
                    SoTienDaThu = model.SoTienDaThu,
                    SoTienPhiBaoTriDaThu = model.SoTienPhiBaoTriDaThu,
                    TyLeViPham = model.TyLeViPham,
                    SoTienViPhamHopDong = model.SoTienViPhamHopDong,
                    SoTienHoanTra = model.SoTienHoanTra,
                    HinhThucThanhLy = model.HinhThucThanhLy
                };

                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiThanhLy = NguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;
                await _context.KdThanhLyHopDongs.AddAsync(record);

                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = record.MaPhieuTl ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "ThanhLyHopDong",
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
                return ResultModel.SuccessWithId(record.MaPhieuTl, "Thêm phiếu thanh lý hợp đồng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm phiếu thanh lý hợp đồng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu thanh lý hợp đồng: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuAsync(ThanhLyHopDongModel model)
        {
            try
            {
                var record = await _context.KdThanhLyHopDongs.FirstOrDefaultAsync(d => d.MaPhieuTl == model.MaPhieu);
                if (record == null)
                    return ResultModel.Fail($"Không tìm thấy thông tin phiếu thanh lý hợp đồng");
                record.NgayThanhLy = !string.IsNullOrEmpty(model.NgayThanhLy) ? DateTime.ParseExact(model.NgayThanhLy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                record.NoiDung = model.GhiChu;
                //  record.HinhThucThanhLy = model.HinhThucThanhLy;
                record.SoTienDaThu = model.SoTienDaThu;
                record.SoTienPhiBaoTriDaThu = model.SoTienPhiBaoTriDaThu;
                record.TyLeViPham = model.TyLeViPham;
                record.SoTienViPhamHopDong = model.SoTienViPhamHopDong;
                record.SoTienHoanTra = model.SoTienHoanTra;

                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == record.MaPhieuTl && d.Controller == "ThanhLyHopDong").ToListAsync();

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
                            MaPhieu = record.MaPhieuTl,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "ThanhLyHopDong",
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
                return ResultModel.SuccessWithId(record.MaPhieuTl, "Cập nhật phiếu thanh lý hợp đồng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu thanh lý hợp đồng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật  phiếu thanh lý hợp đồng: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteAsync(string maPhieu)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var pdc = await _context.KdThanhLyHopDongs.Where(d => d.MaPhieuTl == maPhieu).FirstOrDefaultAsync();
                if (pdc == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu thanh lý hợp đồng");
                }
                _context.KdThanhLyHopDongs.Remove(pdc);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {pdc.SoPhieuTl} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteAsync] Lỗi khi xóa phiếu thanh lý hợp đồng");
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
        public async Task<List<HopDongChuaLenThanhLyModel>> GetHopDongAsync(string maDuAn)
        {
            var entity = new List<HopDongChuaLenThanhLyModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaDuAn", maDuAn);

                entity = (await connection.QueryAsync<HopDongChuaLenThanhLyModel>(
                    "Proc_ThanhLyHD_ChonHopDong",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                if (entity == null)
                {
                    entity = new List<HopDongChuaLenThanhLyModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách hợp đồng chưa thanh lý");
            }
            return entity;
        }
        public async Task<List<BhHinhThucThanhLy>> GetHinhThucThanhLyAsync()
        {
            var entity = new List<BhHinhThucThanhLy>();
            try
            {
                entity = await _context.BhHinhThucThanhLies.ToListAsync();
                if (entity == null)
                {
                    entity = new List<BhHinhThucThanhLy>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách hình thức thanh lý");
            }
            return entity;
        }
        #endregion

        #region Hàm tăng tự động của mã phiếu thanh lý hợp đồng   
        public async Task<string> SinhMaPhieuTuDongAsync(string prefix, int padding = 5)
        {
            _context.ChangeTracker.Clear();
            var maLonNhat = await _context.KdThanhLyHopDongs
                .Where(kh => kh.MaPhieuTl.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap)
                .Select(kh => kh.MaPhieuTl)
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

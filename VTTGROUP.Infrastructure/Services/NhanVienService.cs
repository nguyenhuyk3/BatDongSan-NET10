using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.NhanVien;
using VTTGROUP.Domain.Model.PhieuGiuCho;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class NhanVienService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<NhanVienService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public NhanVienService(IDbContextFactory<AppDbContext> factory, ILogger<NhanVienService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseServer)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseServer;
        }

        #region get list
        public async Task<(List<NhanVienPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? maPhongBan, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaPhongBan", !string.IsNullOrEmpty(maPhongBan) ? maPhongBan : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhanVienPagingDto>(
                "Proc_NhanVien_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<(List<NhanVienPagingDto> Data, int TotalCount)> GetPagingBySanGDAsync(int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
            if (NguoiLap == null)
            {
                return (new List<NhanVienPagingDto>(), 0);
            }
            var maSanGD = NguoiLap.MaNhanVien;

            param.Add("@MaSanGD", maSanGD);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhanVienPagingDto>(
                "Proc_NhanVienSanGD_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<(List<NhanVienPagingDto> Data, int TotalCount)> GetPopupPagingAsync(
         string? maPhongBan, string? loaiNhanVien, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaPhongBan", !string.IsNullOrEmpty(maPhongBan) ? maPhongBan : null);
            param.Add("@MaLoaiNV", !string.IsNullOrEmpty(loaiNhanVien) ? loaiNhanVien : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhanVienPagingDto>(
                "Proc_NhanVienPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<(List<NhanVienPagingDto> Data, int TotalCount)> GetPopupNhanVienBySanGDPagingAsync(
         int page, int pageSize, string? qSearch)
        {
            var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
            var maSanGD = NguoiLap.MaNhanVien;

            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaSanGD", !string.IsNullOrEmpty(maSanGD) ? maSanGD : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhanVienPagingDto>(
                "Proc_NhanVienBySanGDPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<(List<NhanVienPagingDto> Data, int TotalCount)> GetPopupNhanVienUserPagingAsync(
         string? maPhongBan, string? loaiNhanVien, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaPhongBan", !string.IsNullOrEmpty(maPhongBan) ? maPhongBan : null);
            param.Add("@MaLoaiNV", !string.IsNullOrEmpty(loaiNhanVien) ? loaiNhanVien : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhanVienPagingDto>(
                "Proc_NhanVienUserPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa nhân viên
        public async Task<ResultModel> SaveNhanVienAsync(NhanVienModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();

                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                var maNhanVien = await GenerateMaNhanVienAsync();
                var record = new TblNhanvien
                {
                    MaNhanVien = maNhanVien,
                    HoVaTen = model.HoVaTen,
                    MaPhongBan = model.MaPhongBan,
                    MaChucVu = model.MaChucVu,
                    NoiSinh = model.NoiSinh,
                    NgaySinh = !string.IsNullOrEmpty(model.NgaySinh) ? DateTime.ParseExact(model.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    GioiTinh = model.GioiTinh,
                    MaCanCuoc = model.MaCanCuoc,
                    NgayCapCc = !string.IsNullOrEmpty(model.NgayCapCc) ? DateTime.ParseExact(model.NgayCapCc, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    NoiCapCc = model.NoiCapCc,
                    NguyenQuan = model.NguyenQuan,
                    MaDanToc = model.MaDanToc,
                    MaTrinhDoHocVan = model.MaTrinhDoHocVan,
                    TinhTrangHonNhan = model.TinhTrangHonNhan,
                    TrangThai = model.TrangThai ?? 1,
                    DiaChiThuongTru = model.DiaChiThuongTru,
                    DiaChiTamTru = model.DiaChiTamTru,
                    SoDienThoai = model.SoDienThoai,
                    SoDienThoai2 = model.SoDienThoai2,
                    Email = model.Email,
                    EmailCongTy = model.EmailCongTy,
                    UrlDaiDien = model.AnhDaiDienFile != null && model.AnhDaiDienFile.Any() ? model.AnhDaiDienFile[0].FullDomain : string.Empty,
                    UrlCccdmatTruoc = model.MatTruocFile != null && model.MatTruocFile.Any() ? model.MatTruocFile[0].FullDomain : string.Empty,
                    UrlCccdmatSau = model.MatSauFile != null && model.MatSauFile.Any() ? model.MatSauFile[0].FullDomain : string.Empty,
                    NguoiLap = NguoiLap.MaNhanVien,
                    NgayLap = DateTime.Now,
                    MaSanGiaoDich = model.MaSanGiaoDich,
                    MaDuAn = model.MaDuAn
                };

                await _context.TblNhanviens.AddAsync(record);
                await _context.SaveChangesAsync();
                if (!string.IsNullOrEmpty(model.MaNganHang))
                {
                    var nganHang = new TblNhanvienNganhang
                    {
                        MaNhanVien = maNhanVien,
                        MaNganHang = model.MaNganHang,
                        SoTaiKhoanNh = model.SoTaiKhoan,
                        MaChiNhanh = model.MaChiNhanh,
                        TenTaiKhoanNh = model.TenTaiKhoan,
                        DiaChiNganHang = model.DiaChiNganHang
                    };

                    await _context.TblNhanvienNganhangs.AddAsync(nganHang);
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
                            MaPhieu = maNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }
                if (model.MatTruocFile != null && model.MatTruocFile.Any())
                {
                    foreach (var file in model.MatTruocFile)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = maNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }
                if (model.MatSauFile != null && model.MatSauFile.Any())
                {
                    foreach (var file in model.MatSauFile)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = maNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }
                if (model.AnhDaiDienFile != null && model.AnhDaiDienFile.Any())
                {
                    foreach (var file in model.AnhDaiDienFile)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = maNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }
                if (listFiles.Any())
                    await _context.HtFileDinhKems.AddRangeAsync(listFiles);
                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithId(record.Id.ToString(), "Thêm nhân viên thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SaveNhanVienAsync] Lỗi khi Thêm nhân viên");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm nhân viên: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateNhanVienAsync(NhanVienModel model, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = await _context.TblNhanviens.FirstOrDefaultAsync(d => d.MaNhanVien == model.MaNhanVien);
                if (record == null)
                    return ResultModel.Fail($"Không tìm thấy thông tin nhân viên");

                //Xóa file dư nếu có chọn lại những file trước đó của 3 Url ảnh địa diện, căn cước mặt trước mặt sau.

                if (!string.IsNullOrEmpty(record.UrlCccdmatTruoc) && model.MatTruocFile.Any())
                {
                    if (record.UrlCccdmatTruoc != model.MatTruocFile[0].FullDomain) //Có sự thay đổi file mới cần xóa file cũ
                    {
                        var f = _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == record.MaNhanVien && d.FullDomain == record.UrlCccdmatTruoc);
                        if (f != null && f.Any())
                        {
                            foreach (var file in f)
                            {
                                var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                                if (File.Exists(fullPath)) File.Delete(fullPath);
                            }

                            _context.HtFileDinhKems.RemoveRange(f);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(record.UrlCccdmatSau) && model.MatSauFile.Any())
                {
                    if (record.UrlCccdmatSau != model.MatSauFile[0].FullDomain) //Có sự thay đổi file mới cần xóa file cũ
                    {
                        var f = _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == record.MaNhanVien && d.FullDomain == record.UrlCccdmatSau);
                        if (f != null && f.Any())
                        {
                            foreach (var file in f)
                            {
                                var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                                if (File.Exists(fullPath)) File.Delete(fullPath);
                            }

                            _context.HtFileDinhKems.RemoveRange(f);
                        }
                    }
                }

                if (!string.IsNullOrEmpty(record.UrlDaiDien) && model.AnhDaiDienFile.Any())
                {
                    if (record.UrlDaiDien != model.AnhDaiDienFile[0].FullDomain) //Có sự thay đổi file mới cần xóa file cũ
                    {
                        var f = _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == record.MaNhanVien && d.FullDomain == record.UrlDaiDien);
                        if (f != null && f.Any())
                        {
                            foreach (var file in f)
                            {
                                var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                                if (File.Exists(fullPath)) File.Delete(fullPath);
                            }

                            _context.HtFileDinhKems.RemoveRange(f);
                        }
                    }
                }

                record.HoVaTen = model.HoVaTen;
                record.MaPhongBan = model.MaPhongBan;
                record.MaChucVu = model.MaChucVu;
                record.NoiSinh = model.NoiSinh;
                record.NgaySinh = !string.IsNullOrEmpty(model.NgaySinh) ? DateTime.ParseExact(model.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                record.GioiTinh = model.GioiTinh;
                record.MaCanCuoc = model.MaCanCuoc;
                record.NgayCapCc = !string.IsNullOrEmpty(model.NgayCapCc) ? DateTime.ParseExact(model.NgayCapCc, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                record.NoiCapCc = model.NoiCapCc;
                record.NguyenQuan = model.NguyenQuan;
                record.MaDanToc = model.MaDanToc;
                record.MaTrinhDoHocVan = model.MaTrinhDoHocVan;
                record.TinhTrangHonNhan = model.TinhTrangHonNhan;
                record.TrangThai = model.TrangThai ?? 1;
                record.DiaChiThuongTru = model.DiaChiThuongTru;
                record.DiaChiTamTru = model.DiaChiTamTru;
                record.SoDienThoai = model.SoDienThoai;
                record.SoDienThoai2 = model.SoDienThoai2;
                record.Email = model.Email;
                record.EmailCongTy = model.EmailCongTy;
                record.UrlDaiDien = model.AnhDaiDienFile != null && model.AnhDaiDienFile.Any() ? model.AnhDaiDienFile[0].FullDomain : string.Empty;
                record.UrlCccdmatTruoc = model.MatTruocFile != null && model.MatTruocFile.Any() ? model.MatTruocFile[0].FullDomain : string.Empty;
                record.UrlCccdmatSau = model.MatSauFile != null && model.MatSauFile.Any() ? model.MatSauFile[0].FullDomain : string.Empty;

                var nganHang = await _context.TblNhanvienNganhangs.FirstOrDefaultAsync(d => d.MaNhanVien == record.MaNhanVien);
                if (nganHang == null)
                {
                    if (!string.IsNullOrEmpty(model.MaNganHang))
                    {
                        nganHang = new TblNhanvienNganhang
                        {
                            MaNhanVien = record.MaNhanVien,
                            MaNganHang = model.MaNganHang,
                            SoTaiKhoanNh = model.SoTaiKhoan,
                            MaChiNhanh = model.MaChiNhanh,
                            TenTaiKhoanNh = model.TenTaiKhoan,
                            DiaChiNganHang = model.DiaChiNganHang
                        };
                        await _context.TblNhanvienNganhangs.AddAsync(nganHang);
                    }
                }
                else
                {
                    nganHang.MaNganHang = model.MaNganHang;
                    nganHang.SoTaiKhoanNh = model.SoTaiKhoan;
                    nganHang.MaChiNhanh = model.MaChiNhanh;
                    nganHang.TenTaiKhoanNh = model.TenTaiKhoan;
                    nganHang.DiaChiNganHang = model.DiaChiNganHang;
                }

                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaNhanVien
                && d.Controller == "NhanVien" && d.FullDomain != record.UrlCccdmatTruoc && d.FullDomain != record.UrlCccdmatSau && d.FullDomain != record.UrlDaiDien)
                    .ToListAsync();

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
                            MaPhieu = record.MaNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }

                var UploadedMatTruocFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaNhanVien
                && d.Controller == "NhanVien" && d.FullDomain == record.UrlCccdmatTruoc).ToListAsync();
                if (model.MatTruocFile != null && model.MatTruocFile.Any())
                {
                    foreach (var file in model.MatTruocFile)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        bool exists = UploadedMatTruocFiles.Any(f =>
                            f.TenFileDinhKem == file.FileName &&
                            f.FileSize == file.FileSize
                        );
                        if (exists)
                            continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = record.MaNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }
                var UploadedMatSauFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaNhanVien
                && d.Controller == "NhanVien" && d.FullDomain == record.UrlCccdmatSau).ToListAsync();
                if (model.MatSauFile != null && model.MatSauFile.Any())
                {
                    foreach (var file in model.MatSauFile)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        bool exists = UploadedMatSauFiles.Any(f =>
                            f.TenFileDinhKem == file.FileName &&
                            f.FileSize == file.FileSize
                        );
                        if (exists)
                            continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = record.MaNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }
                var UploadedDaiDienFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaNhanVien
               && d.Controller == "NhanVien" && d.FullDomain == record.UrlDaiDien).ToListAsync();
                if (model.AnhDaiDienFile != null && model.AnhDaiDienFile.Any())
                {
                    foreach (var file in model.AnhDaiDienFile)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        bool exists = UploadedDaiDienFiles.Any(f =>
                            f.TenFileDinhKem == file.FileName &&
                            f.FileSize == file.FileSize
                        );
                        if (exists)
                            continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = record.MaNhanVien,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "NhanVien",
                            AcTion = "Create",
                            NgayLap = DateTime.Now,
                            MaNhanVien = NguoiLap.MaNhanVien,
                            TenNhanVien = NguoiLap.HoVaTen,
                            FileSize = file.FileSize,
                            FileType = file.ContentType,
                            FullDomain = file.FullDomain,
                        };
                        listFiles.Add(f);
                    }

                }
                if (listFiles.Any())
                    await _context.HtFileDinhKems.AddRangeAsync(listFiles);
                await _context.SaveChangesAsync();

                return ResultModel.Success("Cập nhật nhân viên thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateNhanVienAsync] Lỗi khi Thêm nhân viên");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật nhân viên: {ex.Message}");
            }
        }

        public async Task<ResultModel> FindGetByNhanVienAsync(string? id, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new NhanVienModel();
                if (!string.IsNullOrEmpty(id))
                {
                    var entity = await _context.TblNhanviens.FirstOrDefaultAsync(d => d.Id == Convert.ToInt32(id));
                    if (entity == null)
                        return ResultModel.Fail($"Không tìm thấy thông tin nhân viên");
                    record.Id = entity.Id;
                    record.MaNhanVien = entity.MaNhanVien;
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.NguoiLap);
                    record.NgayLap = entity.NgayLap;
                    record.HoVaTen = entity.HoVaTen;
                    record.MaPhongBan = entity.MaPhongBan;
                    record.MaChucVu = entity.MaChucVu;
                    record.NoiSinh = entity.NoiSinh;
                    record.NgaySinh = string.Format("{0:dd/MM/yyyy}", entity.NgaySinh);
                    record.GioiTinh = entity.GioiTinh;
                    record.MaCanCuoc = entity.MaCanCuoc;
                    record.NgayCapCc = string.Format("{0:dd/MM/yyyy}", entity.NgayCapCc);
                    record.NoiCapCc = entity.NoiCapCc;
                    record.NguyenQuan = entity.NguyenQuan;
                    record.MaDanToc = entity.MaDanToc;
                    record.MaTrinhDoHocVan = entity.MaTrinhDoHocVan;
                    record.TinhTrangHonNhan = entity.TinhTrangHonNhan;
                    record.TrangThai = entity.TrangThai ?? 1;
                    record.DiaChiThuongTru = entity.DiaChiThuongTru;
                    record.DiaChiTamTru = entity.DiaChiTamTru;
                    record.SoDienThoai = entity.SoDienThoai;
                    record.SoDienThoai2 = entity.SoDienThoai2;
                    record.Email = entity.Email;
                    record.EmailCongTy = entity.EmailCongTy;
                    record.UrlDaiDien = entity.UrlDaiDien;
                    record.UrlCccdmatTruoc = entity.UrlCccdmatTruoc;
                    record.UrlCccdmatSau = entity.UrlCccdmatSau;
                    record.UserId = await _context.TblUsers
                    .AsNoTracking()
                    .Where(u => u.MaNhanVien == entity.MaNhanVien)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();
                    record.MaDuAn = entity.MaDuAn;
                    record.TenDuAn = await _context.DaDanhMucDuAns
                    .AsNoTracking()
                    .Where(u => u.MaDuAn == entity.MaDuAn)
                    .Select(u => u.TenDuAn)
                    .FirstOrDefaultAsync();



                    var nganHang = await _context.TblNhanvienNganhangs.FirstOrDefaultAsync(d => d.MaNhanVien == entity.MaNhanVien);
                    if (nganHang != null)
                    {
                        record.MaNganHang = nganHang.MaNganHang;
                        record.MaChiNhanh = nganHang.MaChiNhanh;
                        record.TenTaiKhoan = nganHang.TenTaiKhoanNh;
                        record.SoTaiKhoan = nganHang.SoTaiKhoanNh;
                        record.DiaChiNganHang = nganHang.DiaChiNganHang;
                    }

                    var files = await _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == entity.MaNhanVien
                    && (d.FullDomain != entity.UrlCccdmatSau && d.FullDomain != entity.UrlCccdmatTruoc && d.FullDomain != entity.UrlDaiDien)).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType,
                        FullDomain = d.FullDomain,
                    }).ToListAsync();

                    record.Files = files;

                    var fileMatTruocs = await _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == entity.MaNhanVien
                    && d.FullDomain == entity.UrlCccdmatTruoc).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType,
                        FullDomain = d.FullDomain,
                    }).ToListAsync();
                    record.MatTruocFile = fileMatTruocs;

                    var fileMatSaus = await _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == entity.MaNhanVien
                    && d.FullDomain == entity.UrlCccdmatSau).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType,
                        FullDomain = d.FullDomain,
                    }).ToListAsync();
                    record.MatSauFile = fileMatSaus;

                    var fileDaiDiens = await _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == entity.MaNhanVien
                    && d.FullDomain == entity.UrlDaiDien).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType,
                        FullDomain = d.FullDomain,
                    }).ToListAsync();
                    record.AnhDaiDienFile = fileDaiDiens;
                }
                else
                {
                    var maNhanVien = await GenerateMaNhanVienAsync();
                    record.MaNhanVien = maNhanVien;
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                }

                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (Exception ex)
            {
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin nhân viên: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteNhanVienAsync(string maNV, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();
                var nhanVien = await _context.TblNhanviens.Where(d => d.MaNhanVien == maNV).FirstOrDefaultAsync();
                if (nhanVien == null)
                {
                    return ResultModel.Fail("Không tìm thấy nhân viên");
                }
                var user = await _context.TblUsers.Where(d => d.MaNhanVien == maNV).FirstOrDefaultAsync();
                if (user != null)
                {
                    return ResultModel.Fail("Nhân viên đã phát sinh dữ liệu, không thể xóa được!");
                }
                var listFiles = _context.HtFileDinhKems.Where(d => d.Controller == "NhanVien" && d.MaPhieu == maNV);
                if (listFiles != null && listFiles.Any())
                {
                    foreach (var file in listFiles)
                    {
                        var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }

                    _context.HtFileDinhKems.RemoveRange(listFiles);
                }

                var nganHang = await _context.TblNhanvienNganhangs.FirstOrDefaultAsync(d => d.MaNhanVien == maNV);
                if (nganHang != null)
                    _context.TblNhanvienNganhangs.Remove(nganHang);

                _context.TblNhanviens.Remove(nhanVien);
                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xóa {nhanVien.HoVaTen} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteNhanVienAsync] Lỗi khi xóa nhân viên");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<string> GenerateMaNhanVienAsync()
        {
            using var _context = _factory.CreateDbContext();
            _context.ChangeTracker.Clear();
            var lastMaNV = await _context.TblNhanviens
            .Where(x => x.MaNhanVien.StartsWith("NV-"))
            .OrderByDescending(x => x.MaNhanVien)
            .Select(x => x.MaNhanVien)
            .FirstOrDefaultAsync();

            return _baseService.GenerateNextCode("NV", lastMaNV);
        }

        public async Task<int> CheckIsCreatedUserAsync(string maNhanVien)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                return await db.TblUsers
                    .AsNoTracking()
                    .Where(u => u.MaNhanVien == maNhanVien).Select(d => d.Id).FirstOrDefaultAsync();
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region Đếm số lượng nhân viên đã tạo của sàn và dự án
        public async Task<int> GetSoLuongDaTaoNhanVienSanGDAsync(NhanVienModel? info)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                return await db.TblNhanviens.Where(d => d.MaSanGiaoDich == info.MaSanGiaoDich && d.MaDuAn == info.MaDuAn).CountAsync();
            }
            catch
            {
                return 0;
            }
        }
        #endregion

        #region Get droplist

        public async Task<List<TblNhanvienPhongban>> GetListPhongbBanAsync()
        {
            var entity = new List<TblNhanvienPhongban>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.TblNhanvienPhongbans.ToListAsync();
                if (entity == null)
                {
                    entity = new List<TblNhanvienPhongban>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách phòng ban");
            }
            return entity;
        }
        public async Task<List<TblNhanvienChucvu>> GetListChucVuAsync()
        {
            var entity = new List<TblNhanvienChucvu>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.TblNhanvienChucvus.ToListAsync();
                if (entity == null)
                {
                    entity = new List<TblNhanvienChucvu>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách chức vụ");
            }
            return entity;
        }
        public async Task<List<TblNhanvienDantoc>> GetListDanTocAsync()
        {
            var entity = new List<TblNhanvienDantoc>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.TblNhanvienDantocs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<TblNhanvienDantoc>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách dân tộc");
            }
            return entity;
        }
        public async Task<List<TblNhanvienTrinhdo>> GetListTrinhDoAsync()
        {
            var entity = new List<TblNhanvienTrinhdo>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.TblNhanvienTrinhdos.ToListAsync();
                if (entity == null)
                {
                    entity = new List<TblNhanvienTrinhdo>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách trình độ");
            }
            return entity;
        }

        public async Task<List<TblNganhang>> GetListNganHangAsync()
        {
            var entity = new List<TblNganhang>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.TblNganhangs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<TblNganhang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách ngân hàng");
            }
            return entity;
        }

        public async Task<List<DuAnTheoSanModel>> GetByDuAnAsync()
        {
            var entity = new List<DuAnTheoSanModel>();
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                entity = await (from da in _context.DaDanhMucDuAns
                                join san in _context.DmSanGiaoDichDuAns
                                    on da.MaDuAn equals san.MaDuAn
                                where san.MaSan == NguoiLap.MaSanGiaoDich
                                select new DuAnTheoSanModel
                                {
                                    MaDuAn = da.MaDuAn,
                                    TenDuAn = da.TenDuAn
                                }).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DuAnTheoSanModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách dự án");
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

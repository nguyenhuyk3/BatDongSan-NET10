using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.KhachHang;
using VTTGROUP.Domain.Model.KhachHangTam;
using VTTGROUP.Infrastructure.Database;
using ZXing;
using static Dapper.SqlMapper;

namespace VTTGROUP.Infrastructure.Services
{
    public class KhachHangTamService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<KhachHangTamService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public KhachHangTamService(IDbContextFactory<AppDbContext> factory, ILogger<KhachHangTamService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách khách hàng tạm
        public async Task<(List<KhachHangTamPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? loaiHinh, int page, int pageSize, string? qSearch, string fromDate, string toDate)
        {
            try
            {
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                NguoiLap.MaNhanVien = NguoiLap.LoaiUser == "SGG" ? NguoiLap.MaNhanVien : string.Empty;
                qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();

                param.Add("@LoaiHinh", !string.IsNullOrEmpty(loaiHinh) ? loaiHinh : null);
                param.Add("@Page", page);
                param.Add("@PageSize", pageSize);
                param.Add("@QSearch", qSearch);
                param.Add("@MaNhanVien", NguoiLap.MaNhanVien);
                param.Add("@NgayLapFrom", fromDate);
                param.Add("@NgayLapTo", toDate);

                var result = (await connection.QueryAsync<KhachHangTamPagingDto>(
                    "Proc_KhachHangTam_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;
                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị thông tin khách hàng tạm");
                var result = new List<KhachHangTamPagingDto>();
                return (result, 0);
            }
        }

        public async Task<(List<KhachHangTamPagingDto> Data, int TotalCount)> GetPagingPopupAsync(
         string? loaiHinh, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@LoaiHinh", !string.IsNullOrEmpty(loaiHinh) ? loaiHinh : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<KhachHangTamPagingDto>(
                "Proc_KhachHangTamPopup_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa khách hàng tạm
        public async Task<ResultModel> SaveKhachHangTamAsync(KhachHangTamModel model)
        {
            try
            {
                //var duAn = await _context.KhDmkhachHangTams.FirstOrDefaultAsync(d => d.MaKhachHangTam.ToLower() == model.MaKhachHangTam.ToLower());
                //if (duAn != null)
                //{
                //    return ResultModel.Fail("Mã khách hàng đã tồn tại.");
                //}
                using var _context = _factory.CreateDbContext();
                string maDoiTuong = model.MaDoiTuongKhachHang?.Split('|').FirstOrDefault() ?? string.Empty;
                var record = new KhDmkhachHangTam
                {
                    MaKhachHangTam = await GenerateMaKhachHangAsync(),
                    TenKhachHang = model.TenKhachHang ?? string.Empty,
                    MaDoiTuongKhachHang = maDoiTuong,
                    SoDienThoai = model.SoDienThoai ?? string.Empty,
                    Email = model.Email ?? string.Empty,
                    QuocTich = model.QuocTich ?? string.Empty,
                    NgaySinh = !string.IsNullOrEmpty(model.NgaySinh) ? DateTime.ParseExact(model.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    GioiTinh = model.GioiTinh.ToString(),
                    MaLoaiIdCard = model.MaLoaiIdCard ?? string.Empty,
                    IdCard = model.IdCard ?? string.Empty,
                    NgayCapIdCard = !string.IsNullOrEmpty(model.NgayCapIdCard) ? DateTime.ParseExact(model.NgayCapIdCard, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null,
                    NoiCapIdCard = model.NoiCapIdCard,
                    DiaChiThuongTru = model.DiaChiThuongTru,
                    DiaChiHienNay = model.DiaChiHienNay,
                    MaNguonKhach = model.MaNguonKhach,
                    MaDuAn = model.MaDuAn,
                    MaSanGd = model.MaSanGD,
                    NguoiDaiDien = model.NguoiDaiDien,
                    SoDienThoaiNguoiDaiDien = model.SoDienThoaiNguoiDaiDien,
                    NguoiLienHe = model.NguoiLienHe,
                    ChucVuNguoiDaiDien = model.ChucVuNguoiDaiDien,
                    NgayLap = DateTime.Now,
                    GhiChu = model.GhiChu,
                };

                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                if (NguoiLap.LoaiUser == "SGG")
                {
                    record.MaSanGd = NguoiLap.MaSanGiaoDich;
                }
                record.MaNhanVien = NguoiLap.MaNhanVien;

                var isExist = await _context.KhDmkhachHangTams.FirstOrDefaultAsync(d => d.MaLoaiIdCard == record.MaLoaiIdCard && d.IdCard == record.IdCard && d.MaSanGd == record.MaSanGd);
                if (isExist != null)
                {
                    return ResultModel.Fail("Khách hàng đã tồn tại.");
                }

                await _context.KhDmkhachHangTams.AddAsync(record);

                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                if (model.FileAnhs != null && model.FileAnhs.Any())
                {
                    foreach (var file in model.FileAnhs)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = record?.MaKhachHangTam,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "KhachHangTam",
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
                // return ResultModel.Success("Thêm khách hàng tạm thành công");
                return ResultModel.SuccessWithId(record.MaKhachHangTam, "Thêm khách hàng tạm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm khách hàng tạm");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm khách hàng tạm: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateByIdAsync(KhachHangTamModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.KhDmkhachHangTams.FirstOrDefaultAsync(d => d.MaKhachHangTam.ToLower() == model.MaKhachHangTam.ToLower());
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy khách hàng.");
                }
                entity.TenKhachHang = model.TenKhachHang ?? string.Empty;
                entity.MaDoiTuongKhachHang = model.MaDoiTuongKhachHang ?? string.Empty;
                entity.SoDienThoai = model.SoDienThoai ?? string.Empty;
                entity.Email = model.Email ?? string.Empty;
                entity.QuocTich = model.QuocTich ?? string.Empty;
                entity.NgaySinh = !string.IsNullOrEmpty(model.NgaySinh) ? DateTime.ParseExact(model.NgaySinh, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                entity.GioiTinh = model.GioiTinh.ToString();
                entity.MaLoaiIdCard = model.MaLoaiIdCard ?? string.Empty;
                entity.IdCard = model.IdCard ?? string.Empty;
                entity.NgayCapIdCard = !string.IsNullOrEmpty(model.NgayCapIdCard) ? DateTime.ParseExact(model.NgayCapIdCard, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                entity.NoiCapIdCard = model.NoiCapIdCard;
                entity.DiaChiThuongTru = model.DiaChiThuongTru;
                entity.DiaChiHienNay = model.DiaChiHienNay;
                entity.MaNguonKhach = model.MaNguonKhach;
                entity.MaDuAn = model.MaDuAn;
                //  entity.MaSanGd = model.MaSanGD;
                entity.NguoiDaiDien = model.NguoiDaiDien;
                entity.SoDienThoaiNguoiDaiDien = model.SoDienThoaiNguoiDaiDien;
                entity.NguoiLienHe = model.NguoiLienHe;
                entity.ChucVuNguoiDaiDien = model.ChucVuNguoiDaiDien;
                entity.GhiChu = model.GhiChu;

                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();

                var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaKhachHangTam
                && d.Controller == "KhachHangTam")
                    .ToListAsync();


                if (model.FileAnhs != null && model.FileAnhs.Any())
                {
                    foreach (var file in model.FileAnhs)
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
                            MaPhieu = entity.MaKhachHangTam,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "KhachHangTam",
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
                return ResultModel.Success("Cập nhật khách hàng tạm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật khách hàng tạm");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật khách hàng tạm: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteKHTamAsync(string maKhachHang, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var lch = await _context.KhDmkhachHangTams.Where(d => d.MaKhachHangTam == maKhachHang).FirstOrDefaultAsync();
                if (lch == null)
                {
                    return ResultModel.Fail("Không tìm thấy khách hàng");
                }

                var listFiles = _context.HtFileDinhKems.Where(d => d.Controller == "KhachHangTam" && d.MaPhieu == maKhachHang);
                if (listFiles != null && listFiles.Any())
                {
                    foreach (var file in listFiles)
                    {
                        var fullPath = Path.Combine(webRootPath, file.TenFileDinhKemLuu.TrimStart('/'));
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                    }

                    _context.HtFileDinhKems.RemoveRange(listFiles);
                }

                _context.KhDmkhachHangTams.Remove(lch);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {lch.TenKhachHang} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteKHTamAsync] Lỗi khi xóa khách hàng tạm");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<KhachHangTamPagingDto> listKHT, string webRootPath)
        {
            try
            {
                var ids = listKHT?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaKhachHangTam))
                    .Select(x => x!.MaKhachHangTam.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B1: Lấy trước danh sách file cần xóa (vật lý) ---
                var filePaths = await _context.HtFileDinhKems
                    .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "KhachHangTam")
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c2 = await _context.HtFileDinhKems
                    .Where(d => ids.Contains(d.MaPhieu) && d.Controller == "KhachHangTam")
                    .ExecuteDeleteAsync();

                var cParent = await _context.KhDmkhachHangTams
                    .Where(k => ids.Contains(k.MaKhachHangTam))
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
                return ResultModel.Success("Xóa khách hàng tạm thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách khách hàng tạm");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin khách hàng tạm
        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from kh in _context.KhDmkhachHangTams
                      join duan in _context.DaDanhMucDuAns on kh.MaDuAn equals duan.MaDuAn

                      join dt in _context.KhDmdoiTuongKhachHangs on kh.MaDoiTuongKhachHang equals dt.MaDoiTuongKhachHang into dtDT
                      from dt2 in dtDT.DefaultIfEmpty()

                      join lc in _context.KhDmloaiCards on kh.MaLoaiIdCard equals lc.MaLoaiIdCard into dtCard
                      from lc2 in dtCard.DefaultIfEmpty()

                      join nk in _context.KhDmnguonKhachHangs on kh.MaNguonKhach equals nk.MaNguonKhach into dtNguonKhach
                      from nk2 in dtNguonKhach.DefaultIfEmpty()

                      join sgg in _context.DmSanGiaoDiches on kh.MaSanGd equals sgg.MaSanGiaoDich into dtSGG
                      from sgg2 in dtSGG.DefaultIfEmpty()

                      join qt in _context.HtDmquocGia on kh.QuocTich equals qt.MaQuocGia into qtGroup
                      from qt in qtGroup.DefaultIfEmpty()

                      where kh.MaKhachHangTam == id
                      select new KhachHangTamModel
                      {
                          MaKhachHangTam = kh.MaKhachHangTam,
                          TenKhachHang = kh.TenKhachHang,
                          NgayLap = kh.NgayLap ?? DateTime.Now,
                          MaNhanVien = kh.MaNhanVien ?? string.Empty,
                          MaDoiTuongKhachHang = kh.MaDoiTuongKhachHang ?? string.Empty,
                          TenDoiTuongKhachHang = dt2.TenDoiTuongKhachHang,
                          SoDienThoai = kh.SoDienThoai ?? string.Empty,
                          Email = kh.Email ?? string.Empty,
                          QuocTich = kh.QuocTich,
                          TenQuocTich = qt.TenQuocGia,
                          NgaySinh = string.Format("{0:dd/MM/yyyy}", kh.NgaySinh),
                          GioiTinh = (kh.GioiTinh ?? string.Empty),
                          MaLoaiIdCard = kh.MaLoaiIdCard ?? string.Empty,
                          TenLoaiIdCard = lc2.TenLoaiIdCard,
                          IdCard = kh.IdCard ?? string.Empty,
                          NgayCapIdCard = string.Format("{0:dd/MM/yyyy}", kh.NgayCapIdCard),
                          NoiCapIdCard = kh.NoiCapIdCard ?? string.Empty,
                          DiaChiThuongTru = kh.DiaChiThuongTru ?? string.Empty,
                          DiaChiHienNay = kh.DiaChiHienNay ?? string.Empty,
                          MaNguonKhach = kh.MaNguonKhach,
                          TenNguonKhach = nk2.TenNguonKhach,
                          MaDuAn = kh.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          MaSanGD = kh.MaSanGd ?? string.Empty,
                          TenSanGD = sgg2.TenSanGiaoDich ?? string.Empty,
                          NguoiDaiDien = kh.NguoiDaiDien ?? string.Empty,
                          SoDienThoaiNguoiDaiDien = kh.SoDienThoaiNguoiDaiDien ?? string.Empty,
                          IsCheckGioiTinh = dt2.IsCheckGioiTinh ?? false,
                          IsCheckNgayCapNoiCapIdCard = dt2.IsCheckNgayCapNoiCapIdCard ?? false,
                          IsHienThiNguoiDaiDien = dt2.IsHienThiNguoiDaiDien ?? false,
                          IsHienThiNgaySinh = dt2.IsHienThiNgaySinh ?? false,
                          IsHienThiQuocTich = dt2.IsHienThiQuocTich ?? false,
                          IsHienThiDiaChiThuongTru = dt2.IsHienThiDiaChiThuongTru ?? false,
                          GhiChu = kh.GhiChu ?? string.Empty,
                          NguoiLienHe = kh.NguoiLienHe,
                          ChucVuNguoiDaiDien = kh.ChucVuNguoiDaiDien,
                      }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new KhachHangTamModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaKhachHangTam = await GenerateMaKhachHangAsync();
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                }

                var files = await _context.HtFileDinhKems.Where(d => d.Controller == "KhachHangTam" && d.MaPhieu == entity.MaKhachHangTam)
                    .Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType,
                        FullDomain = d.FullDomain,
                    }).ToListAsync();

                entity.FileAnhs = files;

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một sản phẩm trong dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Load combobox
        public async Task<List<DaDanhMucDuAn>> GetByDuAnAsync()
        {
            var entity = new List<DaDanhMucDuAn>();
            try
            {
                using var context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                if (NguoiLap.LoaiUser == "SGG")
                {
                    entity = await (
                    from kh in context.DmSanGiaoDichDuAns
                    join duan in context.DaDanhMucDuAns on kh.MaDuAn equals duan.MaDuAn
                    where kh.MaSan == NguoiLap.MaNhanVien
                    select new DaDanhMucDuAn
                    {
                        MaDuAn = kh.MaDuAn,
                        TenDuAn = duan.TenDuAn,
                    }).ToListAsync();
                }
                else
                {
                    entity = await context.DaDanhMucDuAns.ToListAsync();
                }
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
        public async Task<List<DmSanGiaoDich>> GetSanGiaoDichAsync()
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
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sàn giao dịch");
            }
            return entity;
        }
        public async Task<List<KhDmnguonKhachHang>> GetNguonKhachHangAsync()
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<KhDmnguonKhachHang>();
            try
            {
                entity = await _context.KhDmnguonKhachHangs.ToListAsync();
                if (entity == null)
                {
                    entity = new List<KhDmnguonKhachHang>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh đối tượng nguồn khách hàng");
            }
            return entity;
        }

        public async Task<List<KhDmdoiTuongKhachHang>> GetByLoaiKhachHangAsync()
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

        public async Task<KhDmdoiTuongKhachHang> GetLoaiKhachHangByIdAsync(string id)
        {
            using var _context = _factory.CreateDbContext();
            var entity = new KhDmdoiTuongKhachHang();
            try
            {
                entity = await _context.KhDmdoiTuongKhachHangs.FirstOrDefaultAsync(d => d.MaDoiTuongKhachHang == id);
                if (entity == null)
                {
                    entity = new KhDmdoiTuongKhachHang();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy loại khách hàng");
            }
            return entity;
        }
        public async Task<List<KhDmloaiCard>> GetByCardKhachHangAsync()
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<KhDmloaiCard>();
            try
            {
                entity = await _context.KhDmloaiCards.ToListAsync();
                if (entity == null)
                {
                    entity = new List<KhDmloaiCard>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách loại card");
            }
            return entity;
        }

        public async Task<List<HtDmquocGium>> GetByQuocGiaAsync()
        {
            using var _context = _factory.CreateDbContext();
            var entity = new List<HtDmquocGium>();
            try
            {
                entity = await _context.HtDmquocGia.ToListAsync();
                if (entity == null)
                {
                    entity = new List<HtDmquocGium>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách quốc gia");
            }
            return entity;
        }
        #endregion

        #region Hàm tăng tự động của mã khách hàng
        public async Task<string> GenerateMaKhachHangAsync()
        {
            using var _context = _factory.CreateDbContext();
            var lastMaNV = await _context.KhDmkhachHangTams
            .Where(x => x.MaKhachHangTam.StartsWith("KHT-"))
            .OrderByDescending(x => x.MaKhachHangTam)
            .Select(x => x.MaKhachHangTam)
            .FirstOrDefaultAsync();
            return _baseService.GenerateNextCode("KHT", lastMaNV);
        }
        #endregion

        #region Đọc thông tin từ file hình
        public async Task<ResultModel> DocThongTinCCCDAsync(string webRootPath, List<UploadedFileModel> files)
        {
            try
            {
                foreach (var f in files)
                {
                    var relPath = (f.FileNameSave ?? string.Empty)
                        .Replace('/', Path.DirectorySeparatorChar)
                        .TrimStart(Path.DirectorySeparatorChar);

                    var fullPath = Path.Combine(webRootPath, "uploads", relPath);

                    if (!System.IO.File.Exists(fullPath))
                        continue;

                    var model = ScanQrCodeFromImagePath(webRootPath, fullPath);
                    if (!string.IsNullOrWhiteSpace(model?.IdCard) && !string.IsNullOrWhiteSpace(model?.TenKhachHang))
                    {
                        return ResultModel.SuccessWithData(model, $"Đọc thông tin thành công từ file: {f.FileNameSave}");
                    }
                }

                return ResultModel.Fail("Không phát hiện mã QR hợp lệ trong bất kỳ hình nào.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DocThongTinMatTruocAsync] Lỗi khi đọc thông tin từ hình ảnh");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public KhachHangTamModel ScanQrCodeFromImagePath(string env, string imagePath)
        {
            try
            {
                var model = new KhachHangTamModel();

                var result = ScanQr(imagePath);
                if (!string.IsNullOrEmpty(result))
                {
                    model = ParseQrCccdText(result);
                }
                return model;

            }
            catch (Exception ex)
            {
                return new KhachHangTamModel();
            }
        }

        public string ScanQr(string imagePath)
        {
            var sw = Stopwatch.StartNew();
            sw.Restart();
            var dir = Path.GetDirectoryName(imagePath)!;
            var tmpPath = Path.Combine(dir, $"__qr_{Guid.NewGuid():N}.jpg");
            try
            {
                CompressImageFile(imagePath, tmpPath, 1280, 70, true);

                _logger.LogError("Load+Compress: {0} ms", sw.ElapsedMilliseconds);
                sw.Restart();

                using var fs = File.OpenRead(tmpPath);
                using var bmp = SKBitmap.Decode(fs);

                var reader = new ZXing.SkiaSharp.BarcodeReader
                {
                    Options = new ZXing.Common.DecodingOptions
                    {
                        TryHarder = false, // bắt đầu nhẹ
                        PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                        PureBarcode = false
                    },
                    AutoRotate = true,
                    TryInverted = true
                };

                var result = reader.Decode(bmp);
                if (result != null) return result.Text;
                _logger.LogError("Decode base: {0} ms", sw.ElapsedMilliseconds);

                reader.Options.TryHarder = true;
                result = reader.Decode(bmp);
                if (result != null) return result.Text;
                _logger.LogError("Decode base 1: {0} ms", sw.ElapsedMilliseconds);

                using (var enhanced = EnhanceSkBitmap(bmp, scaleMul: 3f, maxWidth: 1600, low: 100, high: 180, sharpen: 0.25f, padPct: 0.06f))
                {
                    reader.Options.TryHarder = false;
                    result = reader.Decode(enhanced);
                    if (result != null) return result.Text;

                    reader.Options.TryHarder = true;
                    result = reader.Decode(enhanced);
                    if (result != null) return result.Text;
                }

                using (var inverted = InvertColors(bmp))
                {
                    result = reader.Decode(inverted);
                    if (result != null) return result.Text;
                    _logger.LogError("Decode base 3: {0} ms", sw.ElapsedMilliseconds);
                }

                using (var sharp = Sharpen(bmp))
                {
                    result = reader.Decode(sharp);
                    if (result != null) return result.Text;
                    _logger.LogError("Decode base 4: {0} ms", sw.ElapsedMilliseconds);
                }

                return null;
            }
            finally
            {
                // 3) Dọn file tạm
                try { if (File.Exists(tmpPath)) File.Delete(tmpPath); } catch { /* ignore */ }
            }
        }


        public static void CompressImageFile(string imagePath, string destPath, int maxWidth = 1280, int quality = 70, bool overwrite = true)
        {
            using var inputStream = File.OpenRead(imagePath);
            using var original = SKBitmap.Decode(inputStream);

            if (original == null)
                throw new Exception("Không thể decode ảnh.");

            int width = original.Width;
            int height = original.Height;

            if (width > maxWidth)
            {
                float ratio = (float)maxWidth / width;
                width = maxWidth;
                height = (int)(height * ratio);
            }

            using var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
            if (resized == null)
                throw new Exception("Resize ảnh thất bại.");

            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
            File.WriteAllBytes(destPath, data.ToArray());
            // ✅ Ghi đè lại chính file gốc
            //if (overwrite)
            //{
            //    File.WriteAllBytes(imagePath, data.ToArray());
            //}
            //else
            //{
            //    var compressedPath = Path.Combine(Path.GetDirectoryName(imagePath), "compressed_" + Path.GetFileName(imagePath));
            //    File.WriteAllBytes(compressedPath, data.ToArray());
            //}
        }

        private static readonly byte[] LUT_IDENTITY = BuildIdentity();

        private static byte[] BuildIdentity()
        {
            var t = new byte[256];
            for (int i = 0; i < 256; i++) t[i] = (byte)i;
            return t;
        }

        private static byte[] BuildClampLut(int low, int high)
        {
            low = Math.Clamp(low, 0, 255);
            high = Math.Clamp(high, 0, 255);
            if (low > high) (low, high) = (high, low);

            var t = new byte[256];
            for (int i = 0; i < 256; i++)
                t[i] = (byte)(i < low ? 0 : (i > high ? 255 : i));
            return t;
        }

        private static void DrawWithClampLut(SKCanvas canvas, SKBitmap src, int x, int y, int low, int high)
        {
            var clamp = BuildClampLut(low, high);
            using var p = new SKPaint { ColorFilter = SKColorFilter.CreateTable(clamp) }; // <-- 1 mảng
            canvas.DrawBitmap(src, x, y, p);
        }
        private static SKBitmap EnhanceSkBitmap(
    SKBitmap original,
    float scaleMul = 3f,     // như bạn muốn: x3 (nhưng có chặn maxWidth)
    int maxWidth = 1600,   // tránh quá nặng trên server
    int low = 100,         // dưới -> 0
    int high = 180,        // trên -> 255
    float sharpen = 0.25f,   // 0..0.6
    float padPct = 0.06f     // quiet-zone ~6% mỗi bên
)
        {
            // 1) Upscale an toàn (x3 nhưng không vượt maxWidth)
            float scale = Math.Max(1f, Math.Min(scaleMul, maxWidth / (float)original.Width));
            int w = Math.Max(1, (int)Math.Round(original.Width * scale));
            int h = Math.Max(1, (int)Math.Round(original.Height * scale));
            var info = new SKImageInfo(w, h, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());

            var up = new SKBitmap(info);
            using (var cv = new SKCanvas(up))
            using (var p = new SKPaint { FilterQuality = SKFilterQuality.High, IsAntialias = true })
                cv.DrawBitmap(original, new SKRect(0, 0, w, h), p);

            // 2) Grayscale + tăng tương phản bằng HighContrast (không ColorMatrix)
            var gray = new SKBitmap(info);
            using (var cv1 = new SKCanvas(gray))
            using (var p1 = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateHighContrast(
                    new SKHighContrastConfig(grayscale: true,
                                             invertStyle: SKHighContrastConfigInvertStyle.NoInvert,
                                             contrast: 0.45f))
            })
                cv1.DrawBitmap(up, 0, 0, p1);
            up.Dispose();

            // 3) Clamp mức (100/180) bằng LUT **1 mảng** (an toàn)
            var clamped = new SKBitmap(info);
            using (var cv2 = new SKCanvas(clamped))
                DrawWithClampLut(cv2, gray, 0, 0, low, high);
            gray.Dispose();

            // 4) Sharpen nhẹ (native)
            SKBitmap current = clamped;
            if (sharpen > 0f)
            {
                float a = Math.Clamp(sharpen, 0f, 0.6f);
                var kernel = new float[] { 0f, -a, 0f, -a, 1f + 4f * a, -a, 0f, -a, 0f };

                var sharp = new SKBitmap(info);
                using var cv3 = new SKCanvas(sharp);
                using var p3 = new SKPaint
                {
                    ImageFilter = SKImageFilter.CreateMatrixConvolution(
                        new SKSizeI(3, 3), kernel, 1f, 0f, new SKPointI(1, 1),
                        SKShaderTileMode.Clamp, true)
                };
                cv3.DrawBitmap(clamped, 0, 0, p3);
                clamped.Dispose();
                current = sharp;
            }

            // 5) Quiet-zone trắng giúp ZXing dễ bắt hơn
            int pad = Math.Max(8, (int)(current.Width * padPct));
            var outInfo = new SKImageInfo(current.Width + pad * 2, current.Height + pad * 2,
                                          SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb());
            var finalBmp = new SKBitmap(outInfo);
            using (var c = new SKCanvas(finalBmp))
            {
                c.Clear(SKColors.White);
                c.DrawBitmap(current, pad, pad);
            }
            current.Dispose();

            return finalBmp; // nơi gọi nhớ using(...)
        }

        private static SKBitmap EnhanceSkBitmap(SKBitmap original)
        {
            int scale = 3;
            var resized = new SKBitmap(original.Width * scale, original.Height * scale);
            using var canvas = new SKCanvas(resized);
            canvas.Clear(SKColors.White);

            var paint = new SKPaint
            {
                FilterQuality = SKFilterQuality.High,
                IsAntialias = true
            };
            canvas.DrawBitmap(original, new SKRect(0, 0, resized.Width, resized.Height), paint);

            if (resized.PeekPixels() is SKPixmap pixmap)
            {
                unsafe
                {
                    byte* ptr = (byte*)pixmap.GetPixels();
                    int stride = pixmap.RowBytes;

                    for (int y = 0; y < resized.Height; y++)
                    {
                        for (int x = 0; x < resized.Width; x++)
                        {
                            byte* pixel = ptr + y * stride + x * 4;
                            byte b = pixel[0];
                            byte g = pixel[1];
                            byte r = pixel[2];

                            byte gray = (byte)(r * 0.3 + g * 0.59 + b * 0.11);
                            byte boosted = (gray > 180) ? (byte)255 : (gray < 100 ? (byte)0 : gray);

                            pixel[0] = boosted;
                            pixel[1] = boosted;
                            pixel[2] = boosted;
                        }
                    }
                }
            }

            return resized;
        }
        private static SKBitmap InvertColors(SKBitmap src)
        {

            var inverted = new SKBitmap(src.Width, src.Height);
            src.CopyTo(inverted); // copy pixel

            if (inverted.PeekPixels() is SKPixmap pixmap)
            {
                unsafe
                {
                    byte* ptr = (byte*)pixmap.GetPixels();
                    int stride = pixmap.RowBytes;

                    for (int y = 0; y < pixmap.Height; y++)
                    {
                        for (int x = 0; x < pixmap.Width; x++)
                        {
                            byte* pixel = ptr + y * stride + x * 4;
                            pixel[0] = (byte)(255 - pixel[0]); // B
                            pixel[1] = (byte)(255 - pixel[1]); // G
                            pixel[2] = (byte)(255 - pixel[2]); // R
                        }
                    }
                }
            }

            return inverted;
        }
        private static SKBitmap Sharpen(SKBitmap original)
        {
            var sharpened = new SKBitmap(original.Width, original.Height);
            using var canvas = new SKCanvas(sharpened);

            var kernel = new float[]
            {
        0, -1,  0,
        -1, 5, -1,
        0, -1,  0
            };

            var kernelSize = new SKSizeI(3, 3);
            var kernelOffset = new SKPointI(1, 1); // trung tâm kernel (3x3)
            float gain = 1f;
            float bias = 0f;

            var imageFilter = SKImageFilter.CreateMatrixConvolution(
                kernelSize,
                kernel,
                gain,
                bias,
                kernelOffset,
                SKShaderTileMode.Clamp,
                true // convolveAlpha
            );

            var paint = new SKPaint
            {
                ImageFilter = imageFilter
            };

            canvas.DrawBitmap(original, 0, 0, paint);
            return sharpened;
        }
        public static KhachHangTamModel ParseQrCccdText(string raw)
        {
            raw = raw.Trim('{', '}');
            var parts = raw.Split('|');

            if (parts.Length < 7)
                throw new Exception("QR CCCD không đúng định dạng");

            string FormatDate(string input)
            {
                if (DateTime.TryParseExact(input, "ddMMyyyy", CultureInfo.InvariantCulture,
                                           DateTimeStyles.None, out DateTime date))
                {
                    return date.ToString("dd/MM/yyyy");
                }
                return input;
            }

            return new KhachHangTamModel
            {
                IdCard = parts[0],
                TenKhachHang = parts[2],
                NgaySinh = FormatDate(parts[3]),
                GioiTinh = parts[4] == "Nam" ? "M" : "F",
                DiaChiThuongTru = parts[5],
                NgayCapIdCard = FormatDate(parts[6])
            };
        }

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

using ClosedXML.Excel;
using Dapper;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using System.Reflection;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.Block;
using VTTGROUP.Domain.Model.DMLoaiThietKe;
using VTTGROUP.Domain.Model.DuAn;
using VTTGROUP.Domain.Model.LoaiCanHo;
using VTTGROUP.Domain.Model.LoaiGoc;
using VTTGROUP.Domain.Model.Tang;
using VTTGROUP.Domain.Model.View;
using VTTGROUP.Domain.Model.ViewMatKhoi;
using VTTGROUP.Domain.Model.ViewTruc;
using VTTGROUP.Domain.Model.ViTri;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class DuAnService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<CongViecService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly string _connectionString;
        public DuAnService(IDbContextFactory<AppDbContext> factory, ILogger<CongViecService> logger, IConfiguration config, ICurrentUserService currentUser)   // 👈 thêm cái này)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _currentUser = currentUser;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }
        #region Hiển thị toàn bộ danh sách dự án
        public async Task<List<DuAnModel>> GetDuAnAsync()
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var vuViecs = await _context.DaDanhMucDuAns.Select(d => new DuAnModel
                {
                    MaDuAn = d.MaDuAn,
                    TenDuAn = d.TenDuAn,
                    DiaChi = d.DiaChi,
                    TinhThanh = d.TinhThanh,
                    XaPhuong = d.XaPhuong,
                    GhiChu = d.GhiChu,
                    TrangThai = d.TrangThai
                }).ToListAsync();
                return vuViecs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách dự án");
            }
            return new List<DuAnModel>();
        }

        public async Task<DuAnModel> GetDuAnSingleAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var vuViecs = await _context.DaDanhMucDuAns.Where(d => d.MaDuAn == id).Select(d => new DuAnModel
                {
                    MaDuAn = d.MaDuAn,
                    TenDuAn = d.TenDuAn,
                    DiaChi = d.DiaChi,
                    TinhThanh = d.TinhThanh,
                    XaPhuong = d.XaPhuong,
                    GhiChu = d.GhiChu,
                    TrangThai = d.TrangThai
                }).FirstOrDefaultAsync();
                return vuViecs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách dự án");
            }
            return new DuAnModel();
        }
        #endregion

        #region Thông tin dự án
        public async Task<ResultModel> GetByIdAsync(string id, string webRootPath)
        {
            try
            {
                using var context = _factory.CreateDbContext();

                // ====== 1. Lấy thông tin dự án ======
                var entity = await context.DaDanhMucDuAns
                    .AsNoTracking()
                    .Where(d => d.MaDuAn == id)
                    .Select(d => new DuAnModel
                    {
                        MaDuAn = d.MaDuAn,
                        TenDuAn = d.TenDuAn,
                        DiaChi = d.DiaChi,
                        TinhThanh = d.TinhThanh,
                        XaPhuong = d.XaPhuong,
                        TrangThai = d.TrangThai,
                        GhiChu = d.GhiChu,
                    })
                    .FirstOrDefaultAsync();

                if (entity == null)
                    entity = new DuAnModel { MaDuAn = id };

                // ====== 2. File đính kèm của dự án ======
                var files = await context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.Controller == "Duan" && d.MaPhieu == id)
                    .Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType
                    })
                    .ToListAsync();

                entity.Files = files;

                // ====== 3. Danh sách Block + số tầng + số trục ======
                var listBlockQuery =
                    from b in context.DaDanhMucBlocks
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new BlockModel
                    {
                        MaBlock = b.MaBlock,
                        TenBlock = b.TenBlock,
                        MaDuAn = b.MaDuAn,

                        // Đếm tầng theo Block
                        SoLuongTang = context.DaDanhMucTangs
                            .Count(t => t.MaBlock == b.MaBlock && t.MaDuAn == b.MaDuAn),

                        // Đếm trục theo Block
                        SoLuongTruc = context.DaDanhMucViewTrucs
                            .Count(v => v.MaBlock == b.MaBlock && v.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listBlock = await listBlockQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenBlock)
                    .ToListAsync();

                entity.ListBlock = listBlock;

                // ======4. Danh sách loại căn ======
                var listLoaiCanQuery =
                    from b in context.DaDanhMucLoaiCanHos
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new LoaiCanHoModel
                    {
                        MaLoaiCanHo = b.MaLoaiCanHo,
                        TenLoaiCanHo = b.TenLoaiCanHo,
                        // Đếm loại căn theo sản phẩm
                        SoLuongCanHo = context.DaDanhMucSanPhams
                            .Count(t => t.MaLoaiCan == b.MaLoaiCanHo && t.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listLoaiCan = await listLoaiCanQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenLoaiCanHo)
                    .ToListAsync();

                entity.ListLoaiCan = listLoaiCan;

                //// ======5. Danh sách loại diện tích ======
                var listLDTQuery =
                    from b in context.DaLoaiDienTiches
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new LoaiDienTichModel
                    {
                        MaLoaiDT = b.MaLoaiDt,
                        TenLoaiDT = b.TenLoaiDt,
                        HeSo = b.HeSo ?? 1,
                        // Đếm loại căn theo sản phẩm
                        SoLuongCanHo = context.DaDanhMucSanPhams
                            .Count(t => t.MaLoaiDienTich == b.MaLoaiDt && t.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listLoaiDT = await listLDTQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenLoaiDT)
                    .ToListAsync();

                entity.ListLoaiDT = listLoaiDT;

                //// ======6. Danh sách loại layout ======
                var listLoaiLayoutQuery =
                    from b in context.DaDanhMucLoaiThietKes
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new LoaiThietKeModel
                    {
                        MaLoaiThietKe = b.MaLoaiThietKe,
                        TenLoaiThietKe = b.TenLoaiThietKe,
                        // Đếm loại căn theo sản phẩm
                        SoLuongCanHo = context.DaDanhMucSanPhams
                            .Count(t => t.MaLoaiLayout == b.MaLoaiThietKe && t.MaDuAn == b.MaDuAn),
                        HinhAnhUrl = b.HinhAnh,
                        IsNew = false,
                    };

                var listLoaiLayout = await listLoaiLayoutQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenLoaiThietKe)
                    .ToListAsync();

                entity.ListLoaiLayout = listLoaiLayout;

                //// ======7. Danh sách hướng ======
                var listHuongQuery =
                    from b in context.DaDanhMucHuongs
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new HuongModel
                    {
                        MaHuong = b.MaHuong,
                        TenHuong = b.TenHuong,
                        HeSo = b.HeSo ?? 1,
                        // Đếm số lượng hướng theo trục
                        SoLuongCanHo = context.DaDanhMucViewTrucs
                            .Count(t => t.MaHuong == b.MaHuong && t.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listLoaiHuong = await listHuongQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenHuong)
                    .ToListAsync();

                entity.ListHuong = listLoaiHuong;

                //// ======8. Danh sách view mặt khối ======
                var listVMKQuery =
                    from b in context.DaDanhMucViewMatKhois
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new ViewMatKhoiModel
                    {
                        MaMatKhoi = b.MaMatKhoi,
                        TenMatKhoi = b.TenMatKhoi,
                        HeSoMatKhoi = b.HeSoMatKhoi ?? 1,
                        // Đếm số lượng hướng theo trục
                        SoLuongCanHo = context.DaDanhMucViewTrucs
                            .Count(t => t.MaViewMatKhoi == b.MaMatKhoi && t.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listVMK = await listVMKQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenMatKhoi)
                    .ToListAsync();

                entity.ListVMK = listVMK;

                //// ======9. Danh sách loại góc ======
                var listLGKQuery =
                    from b in context.DaDanhMucLoaiGocs
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new LoaiGocModel
                    {
                        MaLoaiGoc = b.MaLoaiGoc,
                        TenLoaiGoc = b.TenLoaiGoc,
                        HeSoGoc = b.HeSoGoc ?? 1,
                        // Đếm số lượng hướng theo trục
                        SoLuongVMK = context.DaDanhMucViewTrucs
                            .Count(t => t.MaLoaiGoc == b.MaLoaiGoc && t.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listLG = await listLGKQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenLoaiGoc)
                    .ToListAsync();

                entity.ListLG = listLG;

                //// ======10. Danh sách vị trí ======
                var listVTQuery =
                    from b in context.DaDanhMucViTris
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new ViTriModel
                    {
                        MaViTri = b.MaViTri,
                        TenViTri = b.TenViTri,
                        HeSoViTri = b.HeSoViTri ?? 1,
                        // Đếm số lượng hướng theo trục
                        SoLuongVMK = context.DaDanhMucViewTrucs
                            .Count(t => t.MaViTri == b.MaViTri && t.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listVT = await listVTQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenViTri)
                    .ToListAsync();

                entity.ListVT = listVT;

                //// ======11. Danh sách view ======
                var listViewQuery =
                    from b in context.DaDanhMucViews
                    where string.IsNullOrEmpty(id) || b.MaDuAn == id
                    select new ViewModel
                    {
                        MaView = b.MaView,
                        TenView = b.TenView,
                        // Đếm số lượng hướng theo trục
                        SoLuongVMK = context.DaDanhMucViewTrucs
                            .Count(t => t.MaLoaiView == b.MaView && t.MaDuAn == b.MaDuAn),
                        IsNew = false,
                    };

                var listView = await listViewQuery
                    .AsNoTracking()
                    .OrderBy(b => b.TenView)
                    .ToListAsync();

                entity.ListView = listView;

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        #endregion

        #region Them, xoa, sua du an
        public async Task<ResultModel> SaveDuAnAsync(DuAnModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var duAn = await _context.DaDanhMucDuAns.FirstOrDefaultAsync(d => d.MaDuAn == model.MaDuAn);
                if (duAn != null)
                    return ResultModel.Fail("Dự án đã tồn tại.");

                var record = new DaDanhMucDuAn
                {
                    MaDuAn = model.MaDuAn ?? string.Empty,
                    TenDuAn = model.TenDuAn ?? string.Empty,
                    DiaChi = model.DiaChi,
                    TinhThanh = model.TinhThanh,
                    XaPhuong = model.XaPhuong,
                    TrangThai = model.TrangThai ?? 1,
                    GhiChu = model.GhiChu ?? string.Empty

                };

                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();


                if (model.Files != null && model.Files.Any())
                {
                    foreach (var file in model.Files)
                    {
                        if (string.IsNullOrEmpty(file.FileName)) continue;
                        var savedPath = await SaveFileWithTickAsync(file);

                        var f = new HtFileDinhKem
                        {
                            MaPhieu = model.MaDuAn,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "Duan",
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

                await _context.DaDanhMucDuAns.AddAsync(record);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Thêm dự án thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm dự án");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm dự án: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateByIdAsync(DuAnModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucDuAns.FirstOrDefaultAsync(d => d.MaDuAn == model.MaDuAn);

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy dự án.");
                }
                entity.TenDuAn = model.TenDuAn ?? string.Empty;
                entity.DiaChi = model.DiaChi;
                entity.TinhThanh = model.TinhThanh;
                entity.XaPhuong = model.XaPhuong;
                entity.GhiChu = model.GhiChu ?? string.Empty;
                entity.TrangThai = model.TrangThai ?? 1;

                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaDuAn && d.Controller == "Duan").ToListAsync();

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
                            MaPhieu = model.MaDuAn,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "Duan",
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

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenDuAn} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteDuAnAsync(string maDuAn, string webRootPath, CancellationToken ct = default)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                // 1) Kiểm tra tồn tại
                var exist = await _context.DaDanhMucDuAns
                    .AsNoTracking()
                    .AnyAsync(d => d.MaDuAn == maDuAn, ct);
                if (!exist)
                    return ResultModel.Fail("Không tìm thấy dự án");

                // 2) Lấy danh sách file (chỉ cần path)
                var fileInfos = await _context.HtFileDinhKems
                    .AsNoTracking()
                    .Where(d => d.Controller == "Duan" && d.MaPhieu == maDuAn)
                    .Select(d => d.TenFileDinhKemLuu)
                    .ToListAsync(ct);

                // 3) Xoá DB trong transaction
                await using var tx = await _context.Database.BeginTransactionAsync(ct);
                try
                {
                    // Xoá file đính kèm (bảng)
                    await _context.HtFileDinhKems
                        .Where(d => d.Controller == "Duan" && d.MaPhieu == maDuAn)
                        .ExecuteDeleteAsync(ct);

                    // Xoá dự án
                    var rows = await _context.DaDanhMucDuAns
                        .Where(d => d.MaDuAn == maDuAn)
                        .ExecuteDeleteAsync(ct);

                    if (rows == 0)
                    {
                        await tx.RollbackAsync(ct);
                        return ResultModel.Fail("Không tìm thấy dự án");
                    }

                    await tx.CommitAsync(ct);
                }
                catch (DbUpdateException dbEx)
                {
                    await tx.RollbackAsync(ct);
                    _logger.LogError(dbEx, "[DeleteDuAnAsync] Lỗi FK khi xoá dự án {MaDuAn}", maDuAn);
                    // Thường là do đang bị tham chiếu bởi bảng khác (FK constraint)
                    return ResultModel.Fail("Không thể xoá vì dự án đang được sử dụng ở nơi khác.");
                }
                catch (Exception exTx)
                {
                    await tx.RollbackAsync(ct);
                    _logger.LogError(exTx, "[DeleteDuAnAsync] Lỗi khi xoá dự án {MaDuAn}", maDuAn);
                    return ResultModel.Fail($"Lỗi hệ thống: {exTx.Message}");
                }

                // 4) Xoá file vật lý sau khi DB đã hợp lệ
                var rootFull = Path.GetFullPath(webRootPath ?? string.Empty);
                foreach (var rel in fileInfos.Where(s => !string.IsNullOrWhiteSpace(s)))
                {
                    try
                    {
                        var safeRel = rel.Trim().TrimStart('/', '\\');
                        var fullPath = Path.GetFullPath(Path.Combine(rootFull, safeRel));

                        // Ngăn path traversal
                        if (!fullPath.StartsWith(rootFull, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("[DeleteDuAnAsync] Bỏ qua path không an toàn: {Path}", fullPath);
                            continue;
                        }

                        if (File.Exists(fullPath))
                            File.Delete(fullPath);
                    }
                    catch (Exception ioEx)
                    {
                        // Không rollback DB – chỉ log cảnh báo
                        _logger.LogWarning(ioEx, "[DeleteDuAnAsync] Lỗi khi xoá file vật lý cho dự án {MaDuAn}", maDuAn);
                    }
                }

                return ResultModel.Success($"Xoá dự án {maDuAn} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteDuAnAsync] Lỗi không xác định khi xoá dự án {MaDuAn}", maDuAn);
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
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

        #region  Get thông tin cấu hình theo dự án

        public async Task<(List<LichSuConfigFieldDto> Data, int TotalCount)> GetLichSuConfigAsyn(
         string? maDuAn, string? field, int page, int pageSize)
        {
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@TenTruong", !string.IsNullOrEmpty(field) ? field : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);

            var result = (await connection.QueryAsync<LichSuConfigFieldDto>(
                "Proc_DuAnConfig_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        public async Task<ResultModel> GetCauHinhByIdAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await
                    (from cf in _context.DaDanhMucDuAnCauHinhChungs.AsNoTracking()
                     join da in _context.DaDanhMucDuAns.AsNoTracking() on cf.MaDuAn equals da.MaDuAn
                     where cf.MaDuAn == id
                     select new CauHinhDuAnModel
                     {
                         Id = cf.Id,
                         MaDuAn = cf.MaDuAn,
                         TenDuAn = da.TenDuAn,
                         SoTienGiuCho = cf.SoTienGiuCho,
                         ThoiGianChoBookGioHangRieng = cf.ThoiGianChoBookGioHangRieng,
                         ThoiGianChoBookGioHangChung = cf.ThoiGianChoBookGioHangChung,
                         SaiSoDoanhThuChoPhepKhbh = cf.SaiSoDoanhThuChoPhepKhbh,
                         DonGiaTb = cf.DonGiaTb,
                         DonGiaDat = cf.DonGiaDat,
                         TyLeThueVat = cf.TyLeThueVat,
                         TyLeQuyBaoTri = cf.TyLeQuyBaoTri,
                         IsKichHoatGh = cf.IsKichHoatGh ?? false,
                         IsMoBanCoGia = cf.IsMoBanCoGia ?? false,
                         SoLuongUserSanGd = cf.SoLuongUserSanGd,
                         SoLuongBookingToiDa = cf.SoLuongBookingToiDa,
                         ChoPhepNhieuBookingCho1Can = cf.ChoPhepNhieuBookingCho1Can ?? false,
                         ChenhLechGiaTran = cf.ChenhLechGiaTran,
                         IsHienThiGiaTran = cf.IsHienThiGiaTran ?? false,
                         PhanSoLamTron = cf.PhanSoLamTron,
                         NgayQuaHanTungDotChoPhep = cf.NgayQuaHanTungDotChoPhep,
                         TyLeLaiQuaHan = cf.TyLeLaiQuaHan,
                         PhuongThucTinhChietKhauKM = cf.PhuongThucTinhChietKhauKm,
                     }).FirstOrDefaultAsync();

                if (entity == null)
                {
                    entity = new CauHinhDuAnModel();
                    entity.MaDuAn = id;
                    entity.TenDuAn = _context.DaDanhMucDuAns.FirstOrDefault(d => d.MaDuAn == id)?.TenDuAn ?? string.Empty;
                }

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetCauHinhByIdAsync] Lỗi khi lấy thông tin một dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateCauHinhByIdAsync(string maDuAn, string fieldName, string newValue)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucDuAnCauHinhChungs.FirstOrDefaultAsync(d => d.MaDuAn == maDuAn);

                if (entity == null)
                {
                    entity = new DaDanhMucDuAnCauHinhChung
                    {
                        MaDuAn = maDuAn,
                    };
                    _context.DaDanhMucDuAnCauHinhChungs.Add(entity);
                    await _context.SaveChangesAsync();
                }

                var prop = typeof(DaDanhMucDuAnCauHinhChung)
                            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(p => string.Equals(p.Name, fieldName, StringComparison.OrdinalIgnoreCase));
                if (prop == null) throw new Exception("Sai trường dữ liệu");

                // Lấy giá trị cũ
                var oldVal = prop.GetValue(entity)?.ToString();

                // Convert sang kiểu phù hợp
                object? convertedValue = string.IsNullOrEmpty(newValue)
                    ? null
                    : Convert.ChangeType(newValue, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

                if (oldVal != convertedValue)
                {
                    var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    var log = new HtHistoryLog
                    {
                        MaDuAn = maDuAn,
                        TenBang = "DA_DanhMucDuAn_CauHinhChung",
                        TenTruong = fieldName,
                        GiaTri = (oldVal == null) ? (convertedValue?.ToString()) : oldVal,
                        NgayCapNhat = DateTime.Now,
                        NguoiCapNhat = NguoiLap.MaNhanVien
                    };

                    _context.HtHistoryLogs.Add(log);
                    prop.SetValue(entity, convertedValue);

                    await _context.SaveChangesAsync();
                }

                return ResultModel.SuccessWithData(entity, $"Cập nhật thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[UpdateCauHinhByIdAsync] Lỗi khi cập nhật cấu hình {fieldName} dự án {maDuAn}");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateCauHinhDotByIdAsync(string maDuAn, string maDot, bool? newValue)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.DaDanhMucDotMoBans.FirstOrDefaultAsync(d => d.MaDuAn == maDuAn && d.MaDotMoBan == maDot);

                if (entity == null)
                {
                    return ResultModel.Fail($"Không có thông tin đợt mở bán này.");
                }

                if (newValue == true)
                {
                    var others = await _context.DaDanhMucDotMoBans.Where(d => d.MaDuAn == maDuAn && d.MaDotMoBan != maDot).ToListAsync();
                    if (others.Any())
                    {
                        others.ForEach(x => x.IsKichHoat = false);
                    }
                }

                entity.IsKichHoat = newValue;
                await _context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[UpdateCauHinhDotByIdAsync] Lỗi khi cập nhật cấu hình đợt {maDot} dự án {maDuAn}");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<List<DaDanhMucDotMoBan>> GetListDotMoBanByDuAnAsyn(string maDuAn)
        {
            var result = new List<DaDanhMucDotMoBan>();
            try
            {
                using var _context = _factory.CreateDbContext();
                result = await _context.DaDanhMucDotMoBans.Where(d => d.MaDuAn == maDuAn).OrderBy(c => c.ThuTuHienThi).ToListAsync();
                return result;
            }
            catch
            {
                result = new List<DaDanhMucDotMoBan>();
            }
            return result;
        }

        #endregion

        #region Thêm danh mục trong dự án

        #region Thêm, cập nhật, xóa block      
        public async Task<ResultModel> SaveBlocksAsync(
      string maDuAn,
      List<BlockModel>? blocks,
      CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu block để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaBlock))
                .Select(x => new BlockModel
                {
                    IsNew = x.IsNew,
                    MaBlock = x.MaBlock.Trim(),
                    TenBlock = x.TenBlock?.Trim(),
                    SoLuongTang = x.SoLuongTang,
                    SoLuongTruc = x.SoLuongTruc
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã block không được để trống.");

            // 2) Kiểm tra trùng mã block trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaBlock, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã block bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ block hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucBlocks
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaBlock, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những block đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaBlock))
                    .Select(b => b.MaBlock)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã block sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã block còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaBlock)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaBlock, out var entity))
                    {
                        // ✅ ĐÃ CÓ TRONG DB → UPDATE
                        entity.TenBlock = model.TenBlock;
                        // (Nếu sau này có thêm field khác thì update ở đây)

                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucBlock
                        {
                            MaDuAn = maDuAn,
                            MaBlock = model.MaBlock,
                            TenBlock = model.TenBlock,
                        };

                        await context.DaDanhMucBlocks.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá block KHÔNG có Tầng & KHÔNG có Trục
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaBlock))
                    .Select(x => x.MaBlock)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Block đã có Tầng
                    var blocksHasTang = await context.DaDanhMucTangs
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaBlock))
                        .Select(t => t.MaBlock)
                        .Distinct()
                        .ToListAsync(ct);

                    // Block đã có Trục / ViewTruc
                    var blocksHasViewTruc = await context.DaDanhMucViewTrucs
                        .Where(v => v.MaDuAn == maDuAn && codesToDelete.Contains(v.MaBlock))
                        .Select(v => v.MaBlock)
                        .Distinct()
                        .ToListAsync(ct);

                    // Những block bị chặn xoá vì đã có Tầng hoặc Trục
                    var blockedCodes = blocksHasTang
                        .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những block thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaBlock))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucBlocks.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các block: {list} vì đã có Tầng hoặc Trục.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu block thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveBlocksAsync] Lỗi lưu danh mục block cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách block.");
            }
        }

        #endregion

        #region Thêm, cập nhật, xóa loại căn hộ      
        public async Task<ResultModel> SaveLoaiCansAsync(
      string maDuAn,
      List<LoaiCanHoModel>? blocks,
      CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại căn để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaLoaiCanHo))
                .Select(x => new LoaiCanHoModel
                {
                    IsNew = x.IsNew,
                    MaLoaiCanHo = x.MaLoaiCanHo.Trim(),
                    TenLoaiCanHo = x.TenLoaiCanHo?.Trim(),
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã loại căn không được để trống.");

            // 2) Kiểm tra trùng mã loại căn trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaLoaiCanHo, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã loại căn hộ bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ loại căn hộ hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucLoaiCanHos
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaLoaiCanHo, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những loại căn hộ đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaLoaiCanHo))
                    .Select(b => b.MaLoaiCanHo)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã loại căn hộ sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã loại căn còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaLoaiCanHo)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaLoaiCanHo, out var entity))
                    {
                        // ✅ ĐÃ CÓ TRONG DB → UPDATE
                        entity.TenLoaiCanHo = model.TenLoaiCanHo;
                        // (Nếu sau này có thêm field khác thì update ở đây)

                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucLoaiCanHo
                        {
                            MaDuAn = maDuAn,
                            MaLoaiCanHo = model.MaLoaiCanHo,
                            TenLoaiCanHo = model.TenLoaiCanHo,
                        };

                        await context.DaDanhMucLoaiCanHos.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá loại căn chưa có trong sản phẩm
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaLoaiCanHo))
                    .Select(x => x.MaLoaiCanHo)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Loại căn hộ đã có sản phẩm
                    var blocksHasSanPham = await context.DaDanhMucSanPhams
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaLoaiCan))
                        .Select(t => t.MaLoaiCan)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những block bị chặn xoá vì đã có Sản phẩm
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại căn thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaLoaiCanHo))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucLoaiCanHos.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các loại căn hộ: {list} vì đã có trong Sản phẩm.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu loại căn hộ thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveLoaiCansAsync] Lỗi lưu danh mục loại căn hộ cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách loại căn hộ.");
            }
        }

        #endregion

        #region Thêm, cập nhật, xóa loại diện tích
        public async Task<ResultModel> SaveLoaiDienTichsAsync(
     string maDuAn,
     List<LoaiDienTichModel>? blocks,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại diện tích để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaLoaiDT))
                .Select(x => new LoaiDienTichModel
                {
                    IsNew = x.IsNew,
                    MaLoaiDT = x.MaLoaiDT.Trim(),
                    TenLoaiDT = x.TenLoaiDT?.Trim(),
                    HeSo = x.HeSo
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã loại diện tích không được để trống.");

            // 2) Kiểm tra trùng mã loại căn trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaLoaiDT, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã loại diện tích bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ loại căn hộ hiện có trong DB của dự án này
                var existingBlocks = await context.DaLoaiDienTiches
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaLoaiDt, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những loại diện tích đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaLoaiDT))
                    .Select(b => b.MaLoaiDT)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã loại diện tích sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã loại diện tích còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaLoaiDT)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaLoaiDT, out var entity))
                    {
                        // ✅ ĐÃ CÓ TRONG DB → UPDATE
                        entity.TenLoaiDt = model.TenLoaiDT;
                        // (Nếu sau này có thêm field khác thì update ở đây)
                        entity.HeSo = model.HeSo;
                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaLoaiDienTich
                        {
                            MaDuAn = maDuAn,
                            MaLoaiDt = model.MaLoaiDT,
                            TenLoaiDt = model.TenLoaiDT,
                            HeSo = model.HeSo
                        };

                        await context.DaLoaiDienTiches.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá loại diện tích chưa có trong sản phẩm
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaLoaiDt))
                    .Select(x => x.MaLoaiDt)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Loại căn hộ đã có sản phẩm
                    var blocksHasSanPham = await context.DaDanhMucSanPhams
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaLoaiDienTich))
                        .Select(t => t.MaLoaiDienTich)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những loại diện tích bị chặn xoá vì đã có Sản phẩm
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại căn thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaLoaiDt))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaLoaiDienTiches.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các loại diện tích: {list} vì đã có trong Sản phẩm.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu loại diện tích thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveLoaiDienTichsAsync] Lỗi lưu danh mục loại diện tích cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách loại diện tích.");
            }
        }
        #endregion

        #region Thêm, cập nhật, xóa loại playout
        public async Task<ResultModel> SaveLoaiLayoutsAsync(
     string maDuAn,
     List<LoaiThietKeModel>? blocks, string webRootPath,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại thiết kế để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaLoaiThietKe))
                .Select(x => new LoaiThietKeModel
                {
                    IsNew = x.IsNew,
                    MaLoaiThietKe = x.MaLoaiThietKe.Trim(),
                    TenLoaiThietKe = x.TenLoaiThietKe?.Trim(),
                    HinhAnhUrl = x.HinhAnhUrl,
                    FullDomain = x.FullDomain,
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã loại thiết kế không được để trống.");

            // 2) Kiểm tra trùng mã loại thiết kế trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaLoaiThietKe, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã loại thiết kế bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ loại căn hộ hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucLoaiThietKes
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaLoaiThietKe, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những loại diện tích đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaLoaiThietKe))
                    .Select(b => b.MaLoaiThietKe)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã loại thiết kế sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã loại diện tích còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaLoaiThietKe)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // Danh sách file cần xóa sau khi commit
                var filesToDelete = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaLoaiThietKe, out var entity))
                    {
                        // ✅ ĐÃ CÓ TRONG DB → UPDATE                      
                        entity.TenLoaiThietKe = model.TenLoaiThietKe;
                        // So sánh hình cũ / mới để xử lý xóa file
                        var oldImage = entity.HinhAnh;
                        var newImage = model.HinhAnhUrl;

                        if (!string.Equals(oldImage, newImage, StringComparison.OrdinalIgnoreCase))
                        {
                            // Nếu có hình cũ và đã đổi / xoá → đánh dấu xóa file cũ
                            if (!string.IsNullOrWhiteSpace(oldImage))
                            {
                                filesToDelete.Add(oldImage);
                            }

                            entity.HinhAnh = newImage;
                            entity.FullDomain = model.FullDomain;
                        }
                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucLoaiThietKe
                        {
                            MaDuAn = maDuAn,
                            MaLoaiThietKe = model.MaLoaiThietKe,
                            TenLoaiThietKe = model.TenLoaiThietKe,
                            HinhAnh = model.HinhAnhUrl,
                            FullDomain = model.FullDomain,
                        };

                        await context.DaDanhMucLoaiThietKes.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá loại thiết kế chưa có trong sản phẩm
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaLoaiThietKe))
                    .Select(x => x.MaLoaiThietKe)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Loại căn hộ đã có sản phẩm
                    var blocksHasSanPham = await context.DaDanhMucSanPhams
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaLoaiLayout))
                        .Select(t => t.MaLoaiLayout)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những loại diện tích bị chặn xoá vì đã có Sản phẩm
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại căn thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaLoaiThietKe))
                        .ToList();

                    if (blocksToDelete.Any())
                    {

                        // Đánh dấu xóa file hình của các tầng bị xoá
                        foreach (var tang in blocksToDelete)
                        {
                            if (!string.IsNullOrWhiteSpace(tang.HinhAnh))
                            {
                                filesToDelete.Add(tang.HinhAnh);
                            }
                        }

                        context.DaDanhMucLoaiThietKes.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các loại thiết kế: {list} vì đã có trong Sản phẩm.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // 6) Sau khi commit DB thành công mới xóa file vật lý
                foreach (var imageUrl in filesToDelete)
                {
                    try
                    {
                        var physicalPath = MapImageUrlToPhysicalPath(imageUrl, webRootPath);
                        if (string.IsNullOrWhiteSpace(physicalPath))
                            continue;

                        if (File.Exists(physicalPath))
                        {
                            File.Delete(physicalPath);
                        }
                    }
                    catch (Exception exDel)
                    {
                        // Không fail nghiệp vụ vì xóa file lỗi – chỉ log warning
                        _logger.LogWarning(exDel,
                            "[SaveLoaiLayoutsAsync] Không xoá được file hình loại layout: {ImageUrl}", imageUrl);
                    }
                }


                var msg =
                    $"Lưu loại thiết kế thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveLoaiLayoutsAsync] Lỗi lưu danh mục loại thiết kế cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách loại thiết kế.");
            }
        }
        #endregion

        #region Thêm, cập nhật, xóa hướng
        public async Task<ResultModel> SaveHuongsAsync(
     string maDuAn,
     List<HuongModel>? blocks,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại thiết kế để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaHuong))
                .Select(x => new HuongModel
                {
                    IsNew = x.IsNew,
                    MaHuong = x.MaHuong.Trim(),
                    TenHuong = x.TenHuong?.Trim(),
                    HeSo = x.HeSo
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã hướng không được để trống.");

            // 2) Kiểm tra trùng mã hướng trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaHuong, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã hướng bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ hướng hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucHuongs
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaHuong, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những hướng đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaHuong))
                    .Select(b => b.MaHuong)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã hướng sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã hướng còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaHuong)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaHuong, out var entity))
                    {
                        entity.TenHuong = model.TenHuong;
                        entity.HeSo = model.HeSo;
                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucHuong
                        {
                            MaDuAn = maDuAn,
                            MaHuong = model.MaHuong,
                            TenHuong = model.TenHuong,
                            HeSo = model.HeSo,
                        };

                        await context.DaDanhMucHuongs.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá mã hướng chưa có trong DA_DanhMucViewTruc
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaHuong))
                    .Select(x => x.MaHuong)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Mã hướng đã có trong trục
                    var blocksHasSanPham = await context.DaDanhMucViewTrucs
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaHuong))
                        .Select(t => t.MaHuong)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những mã hướng bị chặn xoá vì đã có trong view trục
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại căn thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaHuong))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucHuongs.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các mã hướng: {list} vì đã có trong danh mục Trục.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu mã hướng thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveHuongsAsync] Lỗi lưu danh mục hướng cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách hướng.");
            }
        }
        #endregion

        #region Thêm, cập nhật, xóa view mặt khối
        public async Task<ResultModel> SaveViewMKsAsync(
     string maDuAn,
     List<ViewMatKhoiModel>? blocks,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại thiết kế để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaMatKhoi))
                .Select(x => new ViewMatKhoiModel
                {
                    IsNew = x.IsNew,
                    MaMatKhoi = x.MaMatKhoi.Trim(),
                    TenMatKhoi = x.TenMatKhoi?.Trim(),
                    HeSoMatKhoi = x.HeSoMatKhoi
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã view mặt khối không được để trống.");

            // 2) Kiểm tra trùng mã view mặt khối trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaMatKhoi, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã mặt khối bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ hướng hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucViewMatKhois
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaMatKhoi, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những mã mặt khối đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaMatKhoi))
                    .Select(b => b.MaMatKhoi)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã view mặt khối sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã hướng còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaMatKhoi)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaMatKhoi, out var entity))
                    {
                        entity.TenMatKhoi = model.TenMatKhoi;
                        entity.HeSoMatKhoi = model.HeSoMatKhoi;
                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucViewMatKhoi
                        {
                            MaDuAn = maDuAn,
                            MaMatKhoi = model.MaMatKhoi,
                            TenMatKhoi = model.TenMatKhoi,
                            HeSoMatKhoi = model.HeSoMatKhoi,
                        };

                        await context.DaDanhMucViewMatKhois.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá mã mặt khối chưa có trong DA_DanhMucViewTruc
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaMatKhoi))
                    .Select(x => x.MaMatKhoi)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Mã mặt khối đã có trong trục
                    var blocksHasSanPham = await context.DaDanhMucViewTrucs
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaViewMatKhoi))
                        .Select(t => t.MaViewMatKhoi)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những mã hướng bị chặn xoá vì đã có trong view trục
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại căn thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaMatKhoi))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucViewMatKhois.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các mã mặt khối: {list} vì đã có trong danh mục Trục.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu mã mặt khối thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveViewMKsAsync] Lỗi lưu danh mục mặt khói cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách mặt khối.");
            }
        }
        #endregion

        #region Thêm, cập nhật, xóa góc
        public async Task<ResultModel> SaveLoaiGocsAsync(
     string maDuAn,
     List<LoaiGocModel>? blocks,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại thiết kế để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaLoaiGoc))
                .Select(x => new LoaiGocModel
                {
                    IsNew = x.IsNew,
                    MaLoaiGoc = x.MaLoaiGoc.Trim(),
                    TenLoaiGoc = x.TenLoaiGoc?.Trim(),
                    HeSoGoc = x.HeSoGoc
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã loại góc không được để trống.");

            // 2) Kiểm tra trùng mã góc trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaLoaiGoc, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã loại góc bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ loại góc hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucLoaiGocs
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaLoaiGoc, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những mã mặt khối đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaLoaiGoc))
                    .Select(b => b.MaLoaiGoc)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã góc sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã hướng còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaLoaiGoc)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaLoaiGoc, out var entity))
                    {
                        // ✅ ĐÃ CÓ TRONG DB → UPDATE                      
                        entity.TenLoaiGoc = model.TenLoaiGoc;
                        entity.HeSoGoc = model.HeSoGoc;
                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucLoaiGoc
                        {
                            MaDuAn = maDuAn,
                            MaLoaiGoc = model.MaLoaiGoc,
                            TenLoaiGoc = model.TenLoaiGoc,
                            HeSoGoc = model.HeSoGoc
                        };

                        await context.DaDanhMucLoaiGocs.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá mã loại góc chưa có trong DA_DanhMucViewTruc
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaLoaiGoc))
                    .Select(x => x.MaLoaiGoc)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Mã loại góc đã có trong trục
                    var blocksHasSanPham = await context.DaDanhMucViewTrucs
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaLoaiGoc))
                        .Select(t => t.MaLoaiGoc)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những mã hướng bị chặn xoá vì đã có trong view trục
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại căn thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaLoaiGoc))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucLoaiGocs.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các mã loại góc: {list} vì đã có trong danh mục Trục.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu mã loại góc thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveLoaiGocsAsync] Lỗi lưu danh mục loại góc cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách loại góc.");
            }
        }
        #endregion

        #region Thêm, cập nhật, xóa vị trí
        public async Task<ResultModel> SaveViTrisAsync(
     string maDuAn,
     List<ViTriModel>? blocks,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại thiết kế để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaViTri))
                .Select(x => new ViTriModel
                {
                    IsNew = x.IsNew,
                    MaViTri = x.MaViTri.Trim(),
                    TenViTri = x.TenViTri?.Trim(),
                    HeSoViTri = x.HeSoViTri
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã loại vị trí không được để trống.");

            // 2) Kiểm tra trùng mã vị trí trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaViTri, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã vị trí bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ loại vị trí hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucViTris
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaViTri, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những mã loại vị trí đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaViTri))
                    .Select(b => b.MaViTri)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã vị trí sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã vị trí còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaViTri)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaViTri, out var entity))
                    {
                        // ✅ ĐÃ CÓ TRONG DB → UPDATE                      
                        entity.TenViTri = model.TenViTri;
                        entity.HeSoViTri = model.HeSoViTri;
                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucViTri
                        {
                            MaDuAn = maDuAn,
                            MaViTri = model.MaViTri,
                            TenViTri = model.TenViTri,
                            HeSoViTri = model.HeSoViTri
                        };

                        await context.DaDanhMucViTris.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá mã loại vị trí chưa có trong DA_DanhMucViewTruc
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaViTri))
                    .Select(x => x.MaViTri)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Mã loại vị trí đã có trong trục
                    var blocksHasSanPham = await context.DaDanhMucViewTrucs
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaViTri))
                        .Select(t => t.MaViTri)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những mã vị trí bị chặn xoá vì đã có trong view trục
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại vị trí thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaViTri))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucViTris.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các mã loại vị trí: {list} vì đã có trong danh mục Trục.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu mã loại vị trí thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveLoaiGocsAsync] Lỗi lưu danh mục loại vị trí cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách loại vị trí.");
            }
        }
        #endregion

        #region Thêm, cập nhật, xóa loại view    
        public async Task<ResultModel> SaveLoaiViewAsync(
      string maDuAn,
      List<ViewModel>? blocks,
      CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại căn để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaView))
                .Select(x => new ViewModel
                {
                    IsNew = x.IsNew,
                    MaView = x.MaView.Trim(),
                    TenView = x.TenView?.Trim(),
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã loại view không được để trống.");

            // 2) Kiểm tra trùng mã loại view trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaView, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã loại view bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ loại căn hộ hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucViews
                    .Where(x => x.MaDuAn == maDuAn)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaView, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những loại căn hộ đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaView))
                    .Select(b => b.MaView)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã loại view sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã loại căn còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaView)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaView, out var entity))
                    {
                        // ✅ ĐÃ CÓ TRONG DB → UPDATE
                        entity.TenView = model.TenView;
                        // (Nếu sau này có thêm field khác thì update ở đây)

                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucView
                        {
                            MaDuAn = maDuAn,
                            MaView = model.MaView,
                            TenView = model.TenView,
                        };

                        await context.DaDanhMucViews.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá loại view chưa có trong view trục
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaView))
                    .Select(x => x.MaView)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Loại view đã có trong view trục
                    var blocksHasSanPham = await context.DaDanhMucViewTrucs
                        .Where(t => t.MaDuAn == maDuAn && codesToDelete.Contains(t.MaLoaiView))
                        .Select(t => t.MaLoaiView)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những view bị chặn xoá vì đã có View trục
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại view thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaView))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucViews.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các loại view: {list} vì đã có trong View trục.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu loại view mặt khối thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveLoaiCansAsync] Lỗi lưu danh mục loại view mặt khối cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách loại view mặt khối.");
            }
        }

        #endregion

        #region Thêm, cập nhật, xóa Tầng theo Block
        public async Task<ResultModel> SaveTangsAsync(
     string maDuAn, string maBlock,
     List<TangModel>? blocks, string webRootPath,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại thiết kế để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaTang))
                .Select(x => new TangModel
                {
                    IsNew = x.IsNew,
                    MaTang = x.MaTang.Trim(),
                    TenTang = x.TenTang?.Trim(),
                    HeSo = x.HeSo,
                    STTTang = x.STTTang,
                    HinhAnhUrl = x.HinhAnhUrl,
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã tầng không được để trống.");

            // 2) Kiểm tra trùng mã tầng trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaTang, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã tầng bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ tầng hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucTangs
                    .Where(x => x.MaDuAn == maDuAn && x.MaBlock == maBlock)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaTang, StringComparer.OrdinalIgnoreCase);

                // Danh sách file cần xóa sau khi commit
                var filesToDelete = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những tầng đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaTang))
                    .Select(b => b.MaTang)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã tầng sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }

                // Tập mã tầng còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaTang)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaTang, out var entity))
                    {
                        // UPDATE
                        entity.TenTang = model.TenTang;
                        entity.HeSoTang = model.HeSo;
                        entity.Stttang = model.STTTang;

                        // So sánh hình cũ / mới để xử lý xóa file
                        var oldImage = entity.HinhAnh;
                        var newImage = model.HinhAnhUrl;

                        if (!string.Equals(oldImage, newImage, StringComparison.OrdinalIgnoreCase))
                        {
                            // Nếu có hình cũ và đã đổi / xoá → đánh dấu xóa file cũ
                            if (!string.IsNullOrWhiteSpace(oldImage))
                            {
                                filesToDelete.Add(oldImage);
                            }

                            entity.HinhAnh = newImage;
                        }

                        updateCount++;
                    }
                    else
                    {
                        // INSERT
                        var newEntity = new DaDanhMucTang
                        {
                            MaDuAn = maDuAn,
                            MaBlock = maBlock,
                            MaTang = model.MaTang,
                            TenTang = model.TenTang,
                            HeSoTang = model.HeSo,
                            Stttang = model.STTTang,
                            HinhAnh = model.HinhAnhUrl
                        };

                        await context.DaDanhMucTangs.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá mã tầng chưa có trong DA_DanhMucSanPham
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaTang))
                    .Select(x => x.MaTang)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Mã tầng đã có trong danh mục sản phẩm
                    var blocksHasSanPham = await context.DaDanhMucSanPhams
                        .Where(t => t.MaDuAn == maDuAn && t.MaBlock == maBlock && codesToDelete.Contains(t.MaTang))
                        .Select(t => t.MaTang)
                        .Distinct()
                        .ToListAsync(ct);

                    var blockedCodes = blocksHasSanPham
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những tầng thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaTang))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        // Đánh dấu xóa file hình của các tầng bị xoá
                        foreach (var tang in blocksToDelete)
                        {
                            if (!string.IsNullOrWhiteSpace(tang.HinhAnh))
                            {
                                filesToDelete.Add(tang.HinhAnh);
                            }
                        }

                        context.DaDanhMucTangs.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các mã tầng: {list} vì đã có trong danh mục Sản phẩm.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                // 6) Sau khi commit DB thành công mới xóa file vật lý
                foreach (var imageUrl in filesToDelete)
                {
                    try
                    {
                        var physicalPath = MapImageUrlToPhysicalPath(imageUrl, webRootPath);
                        if (string.IsNullOrWhiteSpace(physicalPath))
                            continue;

                        if (File.Exists(physicalPath))
                        {
                            File.Delete(physicalPath);
                        }
                    }
                    catch (Exception exDel)
                    {
                        // Không fail nghiệp vụ vì xóa file lỗi – chỉ log warning
                        _logger.LogWarning(exDel,
                            "[SaveTangsAsync] Không xoá được file hình tầng: {ImageUrl}", imageUrl);
                    }
                }

                var msg =
                    $"Lưu mã tầng thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveTangsAsync] Lỗi lưu danh mục Tầng cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách Tầng.");
            }
        }

        private string? MapImageUrlToPhysicalPath(string? url, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // Nếu lỡ lưu absolute URL thì lấy phần Path
            var path = url;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) && uri.IsAbsoluteUri)
            {
                path = uri.AbsolutePath;
            }

            // Bỏ ~/ hoặc / ở đầu
            if (path.StartsWith("~/")) path = path[2..];
            if (path.StartsWith("/")) path = path[1..];

            // (optional) chỉ cho phép xóa trong thư mục uploads/tang
            if (!path.StartsWith("uploads/tang/", StringComparison.OrdinalIgnoreCase))
                return null;

            // Ghép với wwwroot
            var physical = Path.Combine(webRootPath, path.Replace('/', Path.DirectorySeparatorChar));
            return physical;
        }
        #endregion

        #region Thêm, cập nhật, xóa Trục theo Block và dự án
        public async Task<ResultModel> SaveTrucsAsync(
     string maDuAn, string maBlock,
     List<ViewTrucModel>? blocks,
     CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (blocks == null || blocks.Count == 0)
                return ResultModel.Fail("Không có dữ liệu loại thiết kế để lưu.");

            // 1) Chuẩn hoá dữ liệu từ UI
            var normalizedBlocks = blocks
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.MaTruc))
                .Select(x => new ViewTrucModel
                {
                    IsNew = x.IsNew,
                    MaTruc = x.MaTruc.Trim(),
                    TenTruc = x.TenTruc?.Trim(),
                    HeSoTruc = x.HeSoTruc,
                    ThuTuHienThi = x.ThuTuHienThi,
                    MaLoaiGoc = x.MaLoaiGoc,
                    MaView = x.MaView,
                    MaMatKhoi = x.MaMatKhoi,
                    MaViTri = x.MaViTri,
                    MaHuong = x.MaHuong
                })
                .ToList();

            if (normalizedBlocks.Count == 0)
                return ResultModel.Fail("Mã trục không được để trống.");

            // 2) Kiểm tra trùng mã trục trên giao diện
            var dupCodes = normalizedBlocks
                .GroupBy(x => x.MaTruc, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (dupCodes.Any())
            {
                var dupList = string.Join(", ", dupCodes);
                return ResultModel.Fail($"Mã trục bị trùng trên giao diện: {dupList}.");
            }

            try
            {
                await using var context = await _factory.CreateDbContextAsync(ct);
                await using var tx = await context.Database.BeginTransactionAsync(ct);

                // 3) Lấy toàn bộ hướng hiện có trong DB của dự án này
                var existingBlocks = await context.DaDanhMucViewTrucs
                    .Where(x => x.MaDuAn == maDuAn && x.MaBlock == maBlock)
                    .ToListAsync(ct);

                var existingMap = existingBlocks
                    .ToDictionary(x => x.MaTruc, StringComparer.OrdinalIgnoreCase);

                // NEW: Kiểm tra những trục đang được coi là "mới" nhưng mã đã tồn tại trong DB
                var conflictedNewBlocks = normalizedBlocks
                    .Where(b => b.IsNew && existingMap.ContainsKey(b.MaTruc))
                    .Select(b => b.MaTruc)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (conflictedNewBlocks.Any())
                {
                    var list = string.Join(", ", conflictedNewBlocks);
                    return ResultModel.Fail(
                        $"Các mã trục sau đã tồn tại trong dự án, không thể thêm mới: {list}.");
                }
                // Hết phần NEW

                // Tập mã tầng còn trên UI
                var uiCodeSet = normalizedBlocks
                    .Select(x => x.MaTruc)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                var insertCount = 0;
                var updateCount = 0;
                var deleteCount = 0;
                string? cannotDeleteMessage = null;

                // 4) Upsert: UPDATE + INSERT theo list UI
                foreach (var model in normalizedBlocks)
                {
                    if (existingMap.TryGetValue(model.MaTruc, out var entity))
                    {
                        entity.TenTruc = model.TenTruc;
                        entity.HeSoTruc = model.HeSoTruc;
                        entity.ThuTuHienThi = model.ThuTuHienThi;
                        entity.MaLoaiGoc = model.MaLoaiGoc;
                        entity.MaViTri = model.MaViTri;
                        entity.MaHuong = model.MaHuong;
                        entity.MaLoaiView = model.MaView;
                        entity.MaViewMatKhoi = model.MaMatKhoi;
                        updateCount++;
                    }
                    else
                    {
                        // ✅ CHƯA CÓ → INSERT
                        var newEntity = new DaDanhMucViewTruc
                        {
                            MaDuAn = maDuAn,
                            MaBlock = maBlock,
                            MaTruc = model.MaTruc,
                            TenTruc = model.TenTruc,
                            HeSoTruc = model.HeSoTruc,
                            ThuTuHienThi = model.ThuTuHienThi,
                            MaLoaiGoc = model.MaLoaiGoc,
                            MaViTri = model.MaViTri,
                            MaHuong = model.MaHuong,
                            MaLoaiView = model.MaView,
                            MaViewMatKhoi = model.MaMatKhoi,
                        };

                        await context.DaDanhMucViewTrucs.AddAsync(newEntity, ct);
                        insertCount++;
                    }
                }

                // 5) DELETE: chỉ xoá mã trục chưa có trong DA_DanhMucSanPham
                var codesToDelete = existingBlocks
                    .Where(x => !uiCodeSet.Contains(x.MaTruc))
                    .Select(x => x.MaTruc)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (codesToDelete.Any())
                {
                    // Mã tầng đã có trong danh mục sản phẩm
                    var blocksHasSanPham = await context.DaDanhMucSanPhams
                        .Where(t => t.MaDuAn == maDuAn && t.MaBlock == maBlock && codesToDelete.Contains(t.MaTruc))
                        .Select(t => t.MaTruc)
                        .Distinct()
                        .ToListAsync(ct);


                    // Những mã trục bị chặn xoá vì đã có trong sản phẩm
                    var blockedCodes = blocksHasSanPham
                        // .Union(blocksHasViewTruc, StringComparer.OrdinalIgnoreCase)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    // Những loại căn thực sự được phép xoá
                    var deletableCodes = codesToDelete
                        .Where(c => !blockedCodes.Contains(c))
                        .ToList();

                    var blocksToDelete = existingBlocks
                        .Where(x => deletableCodes.Contains(x.MaTruc))
                        .ToList();

                    if (blocksToDelete.Any())
                    {
                        context.DaDanhMucViewTrucs.RemoveRange(blocksToDelete);
                        deleteCount = blocksToDelete.Count;
                    }

                    if (blockedCodes.Any())
                    {
                        var list = string.Join(", ", blockedCodes);
                        cannotDeleteMessage =
                            $"Không xoá được các mã trục: {list} vì đã có trong danh mục Sản phẩm.";
                    }
                }

                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                var msg =
                    $"Lưu mã trục thành công. Thêm mới: {insertCount}, " +
                    $"cập nhật: {updateCount}, xoá: {deleteCount}.";

                if (!string.IsNullOrEmpty(cannotDeleteMessage))
                {
                    msg += " " + cannotDeleteMessage;
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[SaveTrucsAsync] Lỗi lưu danh mục Trục cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Có lỗi xảy ra khi lưu danh sách Trục.");
            }
        }
        #endregion

        #endregion

        #region Hiển thị toàn bộ danh sách tầng theo dự án và theo Block
        public async Task<List<TangModel>> GetTangAsync(string maDuAn, string maBlock, string? qSearch = null)
        {
            try
            {
                await using var context = _factory.CreateDbContext();

                maDuAn = (maDuAn ?? "").Trim();
                maBlock = (maBlock ?? "").Trim();
                qSearch = qSearch?.Trim();

                var query =
                    from tang in context.DaDanhMucTangs.AsNoTracking()

                    join block in context.DaDanhMucBlocks.AsNoTracking()
                        on new { tang.MaDuAn, tang.MaBlock }
                        equals new { block.MaDuAn, block.MaBlock }
                        into blockJoin
                    from block2 in blockJoin.DefaultIfEmpty()

                    where
                        (string.IsNullOrEmpty(maBlock) || tang.MaBlock == maBlock) &&
                        (string.IsNullOrEmpty(maDuAn) || tang.MaDuAn == maDuAn) &&
                        (
                            string.IsNullOrEmpty(qSearch) ||
                            EF.Functions.Like(tang.MaTang, $"%{qSearch}%") ||
                            EF.Functions.Like(tang.TenTang, $"%{qSearch}%") ||
                            EF.Functions.Like(tang.MaBlock, $"%{qSearch}%") ||
                            (block2 != null && EF.Functions.Like(block2.TenBlock, $"%{qSearch}%"))
                        )
                    select new TangModel
                    {
                        MaTang = tang.MaTang,
                        TenTang = tang.TenTang,
                        MaBlock = tang.MaBlock,
                        TenBlock = block2 != null ? block2.TenBlock : "",
                        HeSo = tang.HeSoTang ?? 1,
                        STTTang = tang.Stttang ?? 1,
                        IsNew = false,
                        HinhAnhUrl = tang.HinhAnh ?? string.Empty
                    };

                query = query.OrderBy(x => x.MaBlock).ThenBy(x => x.STTTang);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Tầng");
                return new List<TangModel>();
            }
        }

        #endregion

        #region Import, download file mẫu theo tầng
        public async Task<byte[]> GenerateTemplateWithDataAsync(string templatePath, string maDuAn, CancellationToken ct = default)
        {
            using var _context = _factory.CreateDbContext();
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);

            // Set lại sheet "Tang" là active
            workbook.Worksheet("Tang").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }

        public async Task<ResultModel> ImportFromExcelAsync(
      IBrowserFile file,
      string maDuAn, string maBlock,
      CancellationToken ct = default)
        {
            try
            {
                if (file is null) return ResultModel.Fail("File trống.");
                if (string.IsNullOrWhiteSpace(maDuAn)) return ResultModel.Fail("Thiếu mã dự án.");

                await using var _context = _factory.CreateDbContext();

                // 1) Đọc file vào memory stream
                await using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await inputStream.CopyToAsync(memoryStream, ct);
                memoryStream.Position = 0;

                // 2) Đọc dữ liệu từ Excel (CHỈ đọc, không đụng DB)
                var items = ReadTangFromExcel(memoryStream, maDuAn, maBlock);

                // 3) Làm sạch & chuẩn hoá
                items = items
                    .Where(x => !string.IsNullOrWhiteSpace(x.MaBlock)
                             && !string.IsNullOrWhiteSpace(x.MaTang)
                             && !string.IsNullOrWhiteSpace(x.TenTang))
                    .Select(x =>
                    {
                        x.MaBlock = maBlock;
                        x.MaTang = x.MaTang!.Trim();
                        x.TenTang = x.TenTang!.Trim();
                        x.MaDuAn = maDuAn;
                        x.HeSoTang ??= 1;
                        x.STTTang ??= 1;
                        return x;
                    })
                    .ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu hợp lệ.");

                // 4) Thống kê trùng ngay trong file (theo MaTang, không phân biệt hoa/thường)
                var duplicateInFile = items
                    .GroupBy(x => x.MaTang!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                // Giữ bản đầu tiên cho mỗi MaTang (bỏ trùng nội bộ file)
                items = items
                    .GroupBy(x => x.MaTang!, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

                // 5) Kiểm tra MaBlock tồn tại theo MaDuAn (STRICT: nếu thiếu → fail)
                var existingBlocks = new HashSet<string>(
                    await _context.DaDanhMucBlocks
                        .AsNoTracking()
                        .Where(b => b.MaDuAn == maDuAn && b.MaBlock != null)
                        .Select(b => b.MaBlock!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                var invalidByBlock = items.Where(x => !existingBlocks.Contains(x.MaBlock!)).ToList();
                if (invalidByBlock.Count > 0)
                {
                    var preview = string.Join(", ",
                        invalidByBlock.Select(x => x.MaBlock).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                    return ResultModel.Fail(
                        $"Có {invalidByBlock.Count} dòng có MaBlock không tồn tại trong dự án '{maDuAn}'. " +
                        $"Một số mã: {preview}{(invalidByBlock.Count > 10 ? ", ..." : "")}");
                }

                // 6) Lấy các MaTang đã tồn tại TRONG TOÀN HỆ THỐNG (system-wide)
                var existingAllTangSet = new HashSet<string>(
                    await _context.DaDanhMucTangs
                        .AsNoTracking()
                        .Where(t => t.MaTang != null && t.MaDuAn == maDuAn && t.MaBlock == maBlock)
                        .Select(t => t.MaTang!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                // 6b) (Tuỳ chọn) Lấy các MaTang đã tồn tại theo dự án và block để báo cáo dễ hiểu
                var existingTangInProject = new HashSet<string>(
                    await _context.DaDanhMucTangs
                        .AsNoTracking()
                        .Where(t => t.MaDuAn == maDuAn && t.MaTang != null && t.MaBlock == maBlock)
                        .Select(t => t.MaTang!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                // 7) Phân loại: trùng trong DB (toàn hệ thống) vs thực sự mới
                var duplicatesInDbSystem = items.Where(x => existingAllTangSet.Contains(x.MaTang!)).ToList();
                var newItems = items.Where(x => !existingAllTangSet.Contains(x.MaTang!)).ToList();

                if (newItems.Count == 0)
                {
                    // Ưu tiên hiện ví dụ trong phạm vi dự án cho dễ hiểu
                    var previewProj = string.Join(", ",
                        duplicatesInDbSystem.Where(x => existingTangInProject.Contains(x.MaTang!))
                            .Select(x => x.MaTang).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));

                    // Nếu trong dự án không có ví dụ, lấy đại từ system
                    var previewSystem = string.Join(", ",
                        duplicatesInDbSystem.Select(x => x.MaTang).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));

                    var preview = string.IsNullOrWhiteSpace(previewProj) ? previewSystem : previewProj;

                    return ResultModel.Fail(
                        $"Tất cả MaTang trong file đều đã tồn tại trong hệ thống. " +
                        (duplicatesInDbSystem.Count > 0
                            ? $"Ví dụ: {preview}{(duplicatesInDbSystem.Count > 10 ? ", ..." : "")}"
                            : ""));
                }

                // 8) Thêm hàng loạt (chỉ những mã mới system-wide)
                var entities = newItems.Select(item => new DaDanhMucTang
                {
                    MaBlock = item.MaBlock,
                    MaTang = item.MaTang,
                    TenTang = item.TenTang,
                    HeSoTang = item.HeSoTang,
                    Stttang = item.STTTang,
                    MaDuAn = item.MaDuAn
                }).ToList();

                await _context.DaDanhMucTangs.AddRangeAsync(entities, ct);
                var affected = await _context.SaveChangesAsync(ct);

                // 9) Ghép message thân thiện
                var msg = $"Import thành công {entities.Count} tầng (đã ghi {affected} bản ghi).";
                if (duplicatesInDbSystem.Count > 0)
                {
                    var dupDbPreview = string.Join(", ",
                        duplicatesInDbSystem.Select(x => x.MaTang).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                    msg += $" Bỏ qua {duplicatesInDbSystem.Count} dòng vì MaTang đã tồn tại trong DB (toàn hệ thống)"
                           + (duplicatesInDbSystem.Count > 10 ? $" (ví dụ: {dupDbPreview}, ...)." : $" (ví dụ: {dupDbPreview}).");
                }
                if (duplicateInFile.Count > 0)
                {
                    var dupFilePreview = string.Join(", ", duplicateInFile.Take(10));
                    msg += $" Trong file cũng có {duplicateInFile.Count} mã bị lặp nội bộ"
                         + (duplicateInFile.Count > 10 ? $" (ví dụ: {dupFilePreview}, ...)." : $" (ví dụ: {dupFilePreview}).");
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi import Excel");
                return ResultModel.Fail($"Lỗi import: {ex.Message}");
            }
        }
        public List<TangImportModel> ReadTangFromExcel(Stream stream, string maDuAn, string maBlock)
        {
            // CHỈ đọc Excel, không dùng DbContext ở đây
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });

            var table = dataSet.Tables["Tang"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'Tang' trong file Excel.");

            var list = new List<TangImportModel>();

            foreach (DataRow r in table.Rows)
            {
                // Cột 0=MaTang, 1=TenTang, 2=HeSoTang, 3=STTTang          
                string? maTang = r[0]?.ToString()?.Trim();
                string? tenTang = r[1]?.ToString()?.Trim();

                decimal? heSoTang = null;
                if (decimal.TryParse(r[2]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hst))
                    heSoTang = hst;

                int? sttTang = null;
                if (int.TryParse(r[3]?.ToString()?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stt))
                    sttTang = stt;

                // Nếu cả 3 đều rỗng → bỏ qua dòng trống
                if (string.IsNullOrWhiteSpace(maBlock)
                 && string.IsNullOrWhiteSpace(maTang)
                 && string.IsNullOrWhiteSpace(tenTang))
                    continue;

                list.Add(new TangImportModel
                {
                    MaBlock = maBlock,
                    MaTang = maTang,
                    TenTang = tenTang,
                    HeSoTang = heSoTang ?? 1,
                    STTTang = sttTang ?? 1,
                    MaDuAn = maDuAn
                });
            }

            return list;
        }
        #endregion

        #region Load danh sách combobx hướng
        public async Task<List<DaDanhMucHuong>> GetByHuogTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucHuong>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucHuongs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucHuong>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách hướng theo dự án");
            }
            return entity;
        }

        public async Task<List<DaDanhMucView>> GetByMatKhoiTheoDuAnAsync(string maDuAn, string maBlock)
        {
            var entity = new List<DaDanhMucView>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucViews.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucView>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách mặt khối theo dự án và block");
            }
            return entity;
        }

        public async Task<List<DaDanhMucViewMatKhoi>> GetByViewTheoDuAnAsync(string maDuAn)
        {
            var entity = new List<DaDanhMucViewMatKhoi>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await _context.DaDanhMucViewMatKhois.Where(d => d.MaDuAn == maDuAn).ToListAsync();
                if (entity == null)
                {
                    entity = new List<DaDanhMucViewMatKhoi>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách view theo dự án");
            }
            return entity;
        }
        #endregion

        #region Hiển thị toàn bộ danh sách trục theo dự án và theo Block
        public async Task<List<ViewTrucModel>> GetTrucAsync(string maDuAn, string maBlock)
        {
            try
            {
                using var context = _factory.CreateDbContext();

                var query =
                    from truc in context.DaDanhMucViewTrucs.AsNoTracking()

                        // Block (LEFT JOIN theo cả MaDuAn + MaBlock)
                    join b in context.DaDanhMucBlocks.AsNoTracking()
                        on new { truc.MaDuAn, truc.MaBlock }
                        equals new { b.MaDuAn, b.MaBlock } into jb
                    from block in jb.DefaultIfEmpty()

                        // Loại góc (LEFT JOIN)
                    join lg in context.DaDanhMucLoaiGocs.AsNoTracking()
                        on new { truc.MaDuAn, MaLoaiGoc = truc.MaLoaiGoc }
                        equals new { lg.MaDuAn, lg.MaLoaiGoc } into jlg
                    from loaiGoc in jlg.DefaultIfEmpty()

                        // View mặt khối (LEFT JOIN)
                    join v in context.DaDanhMucViews.AsNoTracking()
                        on new { truc.MaDuAn, MaView = truc.MaLoaiView }
                        equals new { v.MaDuAn, MaView = v.MaView } into jv
                    from view in jv.DefaultIfEmpty()

                        // View (LEFT JOIN)
                    join mk in context.DaDanhMucViewMatKhois.AsNoTracking()
                        on new { truc.MaDuAn, MaMK = truc.MaViewMatKhoi }
                        equals new { mk.MaDuAn, MaMK = mk.MaMatKhoi } into jvmk
                    from mk in jvmk.DefaultIfEmpty()

                        // Vị trí (LEFT JOIN)
                    join vt in context.DaDanhMucViTris.AsNoTracking()
                        on new { truc.MaDuAn, MaViTri = truc.MaViTri }
                        equals new { vt.MaDuAn, MaViTri = vt.MaViTri } into jvt
                    from vt in jvt.DefaultIfEmpty()

                        // Hướng (LEFT JOIN)
                    join h in context.DaDanhMucHuongs.AsNoTracking()
                        on new { truc.MaDuAn, MaHuong = truc.MaHuong }
                        equals new { h.MaDuAn, MaHuong = h.MaHuong } into jh
                    from h in jh.DefaultIfEmpty()

                    where (string.IsNullOrEmpty(maBlock) || truc.MaBlock == maBlock)
                       && (string.IsNullOrEmpty(maDuAn) || truc.MaDuAn == maDuAn)

                    select new ViewTrucModel
                    {
                        MaTruc = truc.MaTruc,
                        TenTruc = truc.TenTruc,

                        MaBlock = truc.MaBlock,
                        TenBlock = block != null ? block.TenBlock : null,

                        HeSoTruc = truc.HeSoTruc ?? 1,
                        ThuTuHienThi = truc.ThuTuHienThi ?? 1,
                        IsNew = false,

                        // 🔹 Thêm các trường yêu cầu
                        MaLoaiGoc = truc.MaLoaiGoc,
                        TenLoaiGoc = loaiGoc != null ? loaiGoc.TenLoaiGoc : null,

                        MaView = truc.MaLoaiView,
                        TenView = view != null ? view.TenView : null,

                        MaMatKhoi = truc.MaViewMatKhoi,
                        TenMatKhoi = mk != null ? mk.TenMatKhoi : null,

                        MaViTri = truc.MaViTri,
                        TenViTri = vt != null ? vt.TenViTri : null,

                        MaHuong = truc.MaHuong,
                        TenHuong = h != null ? h.TenHuong : null,
                    };

                // Sắp xếp
                query = query.OrderBy(x => x.ThuTuHienThi)
                             .ThenBy(x => x.MaTruc);

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách trục");
                return new List<ViewTrucModel>();
            }
        }

        #endregion

        #region Import, download file mẫu theo trục    
        public async Task<byte[]> GenerateTemplateWithDataTrucAsync(string templatePath, string maDuAn, string maBlock, CancellationToken ct = default)
        {
            using var _context = _factory.CreateDbContext();
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);

            var matKhoiCHSheet = workbook.Worksheet("MatKhoiCanHo");
            var listMKCH = await _context.DaDanhMucViews.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            int row = 2;
            foreach (var item in listMKCH)
            {
                matKhoiCHSheet.Cell(row, 1).Value = item.MaView;
                matKhoiCHSheet.Cell(row, 2).Value = item.TenView;
                row++;
            }

            var blockSheetHuong = workbook.Worksheet("Huong");
            var listHuong = await _context.DaDanhMucHuongs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listHuong)
            {
                blockSheetHuong.Cell(row, 1).Value = item.MaHuong;
                blockSheetHuong.Cell(row, 2).Value = item.TenHuong;
                row++;
            }

            var blockSheetVCH = workbook.Worksheet("ViewCanHo");
            var listVCH = await _context.DaDanhMucViewMatKhois.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listVCH)
            {
                blockSheetVCH.Cell(row, 1).Value = item.MaMatKhoi;
                blockSheetVCH.Cell(row, 2).Value = item.TenMatKhoi;
                row++;
            }

            var blockSheetLoaiGoc = workbook.Worksheet("LoaiGoc");
            var listLoaiGoc = await _context.DaDanhMucLoaiGocs.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listLoaiGoc)
            {
                blockSheetLoaiGoc.Cell(row, 1).Value = item.MaLoaiGoc;
                blockSheetLoaiGoc.Cell(row, 2).Value = item.TenLoaiGoc;
                row++;
            }

            var blockSheetVT = workbook.Worksheet("ViTri");
            var listVT = await _context.DaDanhMucViTris.Where(d => d.MaDuAn == maDuAn).ToListAsync();
            // Bắt đầu từ dòng 2 (vì dòng 1 là header)
            row = 2;
            foreach (var item in listVT)
            {
                blockSheetVT.Cell(row, 1).Value = item.MaViTri;
                blockSheetVT.Cell(row, 2).Value = item.TenViTri;
                row++;
            }

            // Set lại sheet "SanPham" là active
            workbook.Worksheet("DanhMucTruc").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }

        public async Task<ResultModel> ImportFromExcelTrucAsync(IBrowserFile file, string maDuAn, string maBlock, CancellationToken ct = default)
        {
            try
            {
                if (file is null) return ResultModel.Fail("File trống.");
                if (string.IsNullOrWhiteSpace(maDuAn)) return ResultModel.Fail("Thiếu mã dự án.");
                if (string.IsNullOrWhiteSpace(maBlock)) return ResultModel.Fail("Thiếu block.");

                await using var _context = _factory.CreateDbContext();

                await using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var ms = new MemoryStream();
                await inputStream.CopyToAsync(ms, ct);
                ms.Position = 0;

                var items = ReadViewTrucFromExcel(ms, maDuAn, maBlock);

                // --- B1: Chuẩn hoá chuỗi ---
                items = items.Select(x =>
                {
                    x.MaTruc = x.MaTruc?.Trim();
                    x.TenTruc = x.TenTruc?.Trim();
                    x.HeSoTruc = x.HeSoTruc ?? 1;
                    x.STTTang = x.STTTang ?? 1;
                    x.MaView = string.IsNullOrWhiteSpace(x.MaView) ? null : x.MaView.Trim();
                    x.MaHuong = string.IsNullOrWhiteSpace(x.MaHuong) ? null : x.MaHuong.Trim();
                    x.MaMatKhoi = string.IsNullOrWhiteSpace(x.MaMatKhoi) ? null : x.MaMatKhoi.Trim();
                    x.MaLoaiGoc = string.IsNullOrWhiteSpace(x.MaLoaiGoc) ? null : x.MaLoaiGoc.Trim();
                    x.MaViTri = string.IsNullOrWhiteSpace(x.MaViTri) ? null : x.MaViTri.Trim();
                    return x;
                }).ToList();

                if (items.Count == 0)
                    return ResultModel.Fail("File không có dữ liệu.");

                // --- B2: Kiểm tra bắt buộc (MaBlock, MaTruc, TenTruc, MaView) ---
                var requiredErrors = new List<string>();
                foreach (var it in items)
                {
                    var miss = new List<string>();
                    if (string.IsNullOrWhiteSpace(it.MaTruc)) miss.Add("MaTruc");
                    if (string.IsNullOrWhiteSpace(it.TenTruc)) miss.Add("TenTruc");
                    if (string.IsNullOrWhiteSpace(it.MaView)) miss.Add("MaView");

                    if (miss.Count > 0)
                        requiredErrors.Add($"- Dòng {it.RowIndex}: thiếu {string.Join(", ", miss)}");
                }
                if (requiredErrors.Count > 0)
                {
                    var preview = string.Join(Environment.NewLine, requiredErrors.Take(15));
                    return ResultModel.Fail("Thiếu dữ liệu bắt buộc:\n" + preview + (requiredErrors.Count > 15 ? "\n... nữa." : ""));
                }

                // --- B3: Trùng mã trong file (MaTruc) ---
                var duplicateInFile = items
                    .GroupBy(x => x.MaTruc!, StringComparer.OrdinalIgnoreCase)
                    .Where(g => g.Count() > 1)
                    .Select(g => $"- {g.Key}: dòng {string.Join(", ", g.Select(i => i.RowIndex))}")
                    .ToList();
                if (duplicateInFile.Count > 0)
                    return ResultModel.Fail("Trùng MaTruc ngay trong file:\n" + string.Join("\n", duplicateInFile));

                //// --- B4: Tồn tại Block theo dự án (bắt buộc) ---
                //var existingBlocks = new HashSet<string>(
                //    await _context.DaDanhMucBlocks.AsNoTracking()
                //        .Where(b => b.MaDuAn == maDuAn && b.MaBlock != null)
                //        .Select(b => b.MaBlock!)
                //        .ToListAsync(ct),
                //    StringComparer.OrdinalIgnoreCase);
                //var invalidBlockRows = items.Where(x => !existingBlocks.Contains(x.MaBlock!)).ToList();
                //if (invalidBlockRows.Count > 0)
                //{
                //    var p = string.Join(", ", invalidBlockRows.Select(x => $"{x.MaBlock}(dòng {x.RowIndex})").Take(15));
                //    return ResultModel.Fail($"MaBlock không tồn tại trong dự án {maDuAn}: {p}" + (invalidBlockRows.Count > 15 ? "..." : ""));
                //}

                // --- B5: Tồn tại Mặt Khối (bắt buộc) ---
                var existingMatKhoi = new HashSet<string>(
                    await _context.DaDanhMucViewMatKhois.AsNoTracking()
                        .Where(m => m.MaMatKhoi != null && m.MaDuAn == maDuAn)
                        .Select(m => m.MaMatKhoi!)
                        .ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);
                var invalidMatKhoiRows = items.Where(x => !existingMatKhoi.Contains(x.MaMatKhoi!) && !string.IsNullOrWhiteSpace(x.MaMatKhoi)).ToList();
                if (invalidMatKhoiRows.Count > 0)
                {
                    var p = string.Join(", ", invalidMatKhoiRows.Select(x => $"{x.MaMatKhoi}(dòng {x.RowIndex})").Take(15));
                    return ResultModel.Fail($"MaView không tồn tại: {p}" + (invalidMatKhoiRows.Count > 15 ? "..." : ""));
                }

                // --- B6: (Optional) Kiểm tra tồn tại nếu có nhập:MaHuong/ MaLoaiGoc / MaView / MaViTri ---
                var failOptional = new List<string>();

                var inputView = items.Select(x => x.MaView).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputView.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucViews.AsNoTracking().Where(x => x.MaView != null && x.MaDuAn == maDuAn && inputView.Contains(x.MaView!)).Select(x => x.MaView!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputView.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MatKhoi không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }

                var inputHuong = items.Select(x => x.MaHuong).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputHuong.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucHuongs.AsNoTracking().Where(x => x.MaHuong != null && x.MaDuAn == maDuAn && inputHuong.Contains(x.MaHuong!)).Select(x => x.MaHuong!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputHuong.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MaHuong không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }

                var inputLoaiGoc = items.Select(x => x.MaLoaiGoc).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputLoaiGoc.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucLoaiGocs.AsNoTracking().Where(x => x.MaLoaiGoc != null && x.MaDuAn == maDuAn && inputLoaiGoc.Contains(x.MaLoaiGoc!)).Select(x => x.MaLoaiGoc!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputLoaiGoc.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MaLoaiGoc không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }

                var inputViTri = items.Select(x => x.MaViTri).Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (inputViTri.Count > 0)
                {
                    var exists = new HashSet<string>(
                        await _context.DaDanhMucViTris.AsNoTracking().Where(x => x.MaViTri != null && x.MaDuAn == maDuAn && inputViTri.Contains(x.MaViTri!)).Select(x => x.MaViTri!).ToListAsync(ct),
                        StringComparer.OrdinalIgnoreCase);
                    var miss = inputViTri.Where(x => !exists.Contains(x!)).ToList();
                    if (miss.Count > 0) failOptional.Add($"MaViTri không tồn tại: {string.Join(", ", miss.Take(15))}{(miss.Count > 15 ? "..." : "")}");
                }

                if (failOptional.Count > 0)
                    return ResultModel.Fail(string.Join("\n", failOptional));

                // --- B7: Trùng MaTruc trong DB (system-wide) ---
                var existingTrucSet = new HashSet<string>(
                    await _context.DaDanhMucViewTrucs.AsNoTracking().Where(t => t.MaTruc != null && t.MaDuAn == maDuAn && t.MaBlock == maBlock).Select(t => t.MaTruc!).ToListAsync(ct),
                    StringComparer.OrdinalIgnoreCase);

                var duplicatesInDbSystem = items.Where(x => existingTrucSet.Contains(x.MaTruc!)).ToList();
                var newItems = items.Where(x => !existingTrucSet.Contains(x.MaTruc!)).ToList();

                if (newItems.Count == 0)
                {
                    var p = string.Join(", ", duplicatesInDbSystem.Select(x => $"{x.MaTruc}(dòng {x.RowIndex})").Distinct(StringComparer.OrdinalIgnoreCase).Take(15));
                    return ResultModel.Fail("Tất cả MaTruc trong file đã tồn tại trong hệ thống: " + p + (duplicatesInDbSystem.Count > 15 ? "..." : ""));
                }

                // --- B8: Insert ---
                var entities = newItems.Select(item => new DaDanhMucViewTruc
                {
                    MaDuAn = maDuAn,
                    MaBlock = maBlock!,
                    MaTruc = item.MaTruc!,
                    TenTruc = item.TenTruc!,
                    HeSoTruc = item.HeSoTruc ?? 1,
                    ThuTuHienThi = item.STTTang ?? 1,
                    MaLoaiGoc = item.MaLoaiGoc,
                    MaLoaiView = item.MaView,// bắt buộc
                    MaViewMatKhoi = item.MaMatKhoi!,
                    MaViTri = item.MaViTri,
                    MaHuong = item.MaHuong!,
                }).ToList();

                await _context.DaDanhMucViewTrucs.AddRangeAsync(entities, ct);
                var affected = await _context.SaveChangesAsync(ct);

                // --- B9: Message ---
                var msg = $"Import thành công {entities.Count} Trục (đã ghi {affected} bản ghi).";
                if (duplicatesInDbSystem.Count > 0)
                {
                    var preview = string.Join(", ", duplicatesInDbSystem.Select(x => x.MaTruc).Distinct(StringComparer.OrdinalIgnoreCase).Take(10));
                    msg += $" Bỏ qua {duplicatesInDbSystem.Count} dòng do MaTruc đã tồn tại (ví dụ: {preview}{(duplicatesInDbSystem.Count > 10 ? ", ..." : ")")}";
                }

                return ResultModel.Success(msg);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi import Excel");
                return ResultModel.Fail($"Lỗi import: {ex.Message}");
            }
        }


        public List<ViewTrucImportModel> ReadViewTrucFromExcel(Stream stream, string maDuAn, string maBlock)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });

            var table = dataSet.Tables["DanhMucTruc"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'DanhMucTruc' trong file Excel.");

            var list = new List<ViewTrucImportModel>();
            int row = 2; // header=1

            foreach (DataRow r in table.Rows)
            {
                string? maTruc = r[0]?.ToString()?.Trim();
                string? tenTruc = r[1]?.ToString()?.Trim();
                decimal? heSoTruc = null;
                if (decimal.TryParse(r[2]?.ToString()?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var hst))
                    heSoTruc = hst;
                int? sttTruc = null;
                if (int.TryParse(r[3]?.ToString()?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var stt))
                    sttTruc = stt;

                string? maLoaiView = r[4]?.ToString()?.Trim();
                string? maHuong = r[5]?.ToString()?.Trim();
                string? maViewMatKhoi = r[6]?.ToString()?.Trim();
                string? maLoaiGoc = r[7]?.ToString()?.Trim();
                string? maViTri = r[8]?.ToString()?.Trim();

                list.Add(new ViewTrucImportModel
                {
                    MaDuAn = maDuAn,
                    MaBlock = maBlock,
                    MaTruc = maTruc,
                    TenTruc = tenTruc,
                    HeSoTruc = heSoTruc ?? 1,
                    STTTang = sttTruc ?? 1,
                    MaView = maLoaiView,
                    MaHuong = maHuong ?? string.Empty,
                    MaMatKhoi = maViewMatKhoi ?? string.Empty,
                    MaLoaiGoc = maLoaiGoc,
                    MaViTri = maViTri ?? string.Empty,
                    RowIndex = row
                });
                row++;
            }
            return list;
        }
        #endregion

        #region Danh sách cấu hình số lượng booking cho từng sàn giao dịch       
        public async Task<List<SanGiaoDichBookingDto>> GetSanGiaoDichBookingByDuAnAsync(
    string maDuAn,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
                return new List<SanGiaoDichBookingDto>();

            var duAnKey = maDuAn.Trim();

            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                // LEFT JOIN để: nếu map có mà master thiếu vẫn không crash (TenSan = "")
                var query =
                    from map in db.DmSanGiaoDichDuAns.AsNoTracking()
                    join san in db.DmSanGiaoDiches.AsNoTracking()
                        on map.MaSan equals san.MaSanGiaoDich into sj
                    from san in sj.DefaultIfEmpty()
                    where map.MaDuAn == duAnKey
                    select new SanGiaoDichBookingDto
                    {
                        MaSanGiaoDich = map.MaSan ?? "",                 // ưu tiên từ map
                        TenSanGiaoDich = san != null ? (san.TenSanGiaoDich ?? "") : "",
                        SoLuongBooking = map.SoLuongBookCanHo ?? 1
                    };

                // Chống duplicate nếu data bẩn (lỡ trùng (MaDuAn, MaSan))
                var list = await query.ToListAsync(ct);

                return list
                    .GroupBy(x => x.MaSanGiaoDich, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .OrderBy(x => x.TenSanGiaoDich)
                    .ToList();
            }
            catch (OperationCanceledException)
            {
                // bị cancel do user đổi dự án/đi trang khác -> coi như bình thường
                return new List<SanGiaoDichBookingDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[GetSanGiaoDichBookingByDuAnAsync] Failed. MaDuAn={MaDuAn}", duAnKey);

                // UI không sập
                return new List<SanGiaoDichBookingDto>();
            }
        }
        #endregion

        #region Cập nhật số lượng booking sàn giao dịch
        public async Task<ResultModel> UpdateSoLuongBookingSGGAsync(
     string maDuAn,
     string maSanGG,
     int? soLuongBooking,
     CancellationToken ct = default)
        {
            // 1) Guard input
            if (string.IsNullOrWhiteSpace(maDuAn))
                return ResultModel.Fail("Thiếu mã dự án.");

            if (string.IsNullOrWhiteSpace(maSanGG))
                return ResultModel.Fail("Thiếu mã sàn giao dịch.");

            var duAnKey = maDuAn.Trim();
            var sanKey = maSanGG.Trim();

            // 2) Validate business (tuỳ bạn có cho null không)
            if (soLuongBooking is null)
                return ResultModel.Fail("Số lượng booking không hợp lệ.");

            if (soLuongBooking < 0)
                return ResultModel.Fail("Số lượng booking phải >= 0.");

            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                // 3) Load entity cần update (tracking)
                var entity = await db.DmSanGiaoDichDuAns
                    .FirstOrDefaultAsync(x => x.MaDuAn == duAnKey && x.MaSan == sanKey, ct);

                if (entity is null)
                    return ResultModel.Fail("Không tìm thấy sàn giao dịch trong dự án.");

                // 4) No-op optimization (khỏi save nếu không đổi)
                var newValue = soLuongBooking.Value;
                if (entity.SoLuongBookCanHo == newValue)
                    return ResultModel.SuccessWithData(entity, "Không có thay đổi.");

                entity.SoLuongBookCanHo = newValue;

                // 5) Save
                await db.SaveChangesAsync(ct);

                return ResultModel.SuccessWithData(entity, "Cập nhật thành công!");
            }
            catch (OperationCanceledException)
            {
                return ResultModel.Fail("Thao tác đã bị huỷ.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex,
                    "[UpdateSoLuongBookingSGGAsync] Concurrency conflict. MaDuAn={MaDuAn}, MaSan={MaSan}",
                    duAnKey, sanKey);

                return ResultModel.Fail("Dữ liệu đã thay đổi bởi người khác. Vui lòng tải lại và thử lại.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[UpdateSoLuongBookingSGGAsync] Error. MaDuAn={MaDuAn}, MaSan={MaSan}, SoLuongBooking={SoLuong}",
                    duAnKey, sanKey, soLuongBooking);

                // Không trả ex.Message ra UI (an toàn + chuyên nghiệp)
                return ResultModel.Fail("Lỗi hệ thống: Không thể cập nhật số lượng booking.");
            }
        }

        #endregion

        #region Thêm sàn giao dịch vào dự án
        public async Task<ResultModel> AddSGDToDuAnAsync(string maDuAn, List<string> maSanList)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(maDuAn))
                {
                    return ResultModel.Fail("Thiếu mã dự án.");
                }
                if (maSanList == null || maSanList.Count == 0)
                {
                    return ResultModel.Fail("Không có sàn giao dịch để thêm.");
                }

                await using var _context = _factory.CreateDbContext();
                // Lấy các mapping hiện có để tránh duplicate
                var existing = await _context.DmSanGiaoDichDuAns
                    .Where(x => x.MaDuAn == maDuAn && maSanList.Contains(x.MaSan))
                    .Select(x => x.MaSan)
                    .ToListAsync();
                var toInsert = maSanList
                    .Where(m => !existing.Contains(m, StringComparer.OrdinalIgnoreCase))
                    .Select(m => new DmSanGiaoDichDuAn
                    {
                        MaDuAn = maDuAn,
                        MaSan = m,
                        SoLuongBookCanHo = 1
                    })
                    .ToList();

                if (toInsert.Any())
                {
                    await _context.DmSanGiaoDichDuAns.AddRangeAsync(toInsert);
                    await _context.SaveChangesAsync();
                }

                return ResultModel.Success($"Đã thêm {toInsert.Count} sàn giao dịch cho dự án {maDuAn}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AddSGDToDuAnAsync] Lỗi khi thêm mapping SGG cho dự án {MaDuAn}", maDuAn);

                return ResultModel.Fail("Lỗi hệ thống: không thể thêm sàn giao dịch.");
            }
        }
        #endregion

        #region Xóa sàn giao dịch ra khỏi dự án
        public async Task<ResultModel> RemoveSGDFromDuAnAsync(string maDuAn, string maSanGiaoDich, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn))
            {
                return ResultModel.Fail("Thiếu mã dự án.");
            }
            if (string.IsNullOrWhiteSpace(maSanGiaoDich))
            {
                return ResultModel.Fail("Thiếu mã sàn giao dịch.");
            }

            try
            {
                await using var context = _factory.CreateDbContext();
                var mapping = await context.DmSanGiaoDichDuAns
                    .FirstOrDefaultAsync(x => x.MaDuAn == maDuAn && x.MaSan == maSanGiaoDich, ct);

                if (mapping == null)
                {
                    return ResultModel.Fail("Không tìm thấy sàn giao dịch trong dự án.");
                }

                context.DmSanGiaoDichDuAns.Remove(mapping);
                await context.SaveChangesAsync(ct);

                return ResultModel.Success("Xóa sàn giao dịch khỏi dự án thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[RemoveSGDFromDuAnAsync] Lỗi khi xoá sàn giao dịch {MaSan} khỏi dự án {MaDuAn}", maSanGiaoDich, maDuAn);

                return ResultModel.Fail("Lỗi hệ thống: không thể xoá sàn giao dịch khỏi dự án.");
            }
        }
        #endregion
    }
}

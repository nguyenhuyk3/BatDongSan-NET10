using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.PhieuDatCoc;
using VTTGROUP.Domain.Model.ThanhLyDatCoc;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ThanhLyDatCocService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<ThanhLyDatCocService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public ThanhLyDatCocService(IDbContextFactory<AppDbContext> factory, ILogger<ThanhLyDatCocService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Hiển thị danh sách thanh lý đặt cọc
        public async Task<(List<ThanhLyDatCocPagingDto> Data, int TotalCount)> GetPagingAsync(
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

            var result = (await connection.QueryAsync<ThanhLyDatCocPagingDto>(
                "Proc_ThanhLyDatCoc_GetPaging",
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
                using var _context = _factory.CreateDbContext();
                var entity = await (from tl in _context.BhPhieuThanhLyDatCocs
                                    join ptl in _context.BhHinhThucThanhLies
                                        on tl.HinhThucThanhLy equals ptl.MaHttl into g
                                    from ptl in g.DefaultIfEmpty()
                                    where tl.MaPhieuThanhLy == id
                                    select new ThanhLyDatCocModel
                                    {
                                        MaPhieu = tl.MaPhieuThanhLy,
                                        SoPhieuThanhLy = tl.SoPhieuThanhLy,
                                        NgayLap = tl.NgayLap ?? DateTime.Now,
                                        NguoiThanhLy = tl.NguoiThanhLy,
                                        NgayThanhLy = string.Format("{0:dd/MM/yyyy}", tl.NgayThanhLy),
                                        MaPhieuDatCoc = tl.MaPhieuDatCoc,
                                        PhiPhat = tl.PhiPhat ?? 0,
                                        HinhThucThanhLy = tl.HinhThucThanhLy,
                                        TenHinhThucThanhLy = ptl.TenHttl,
                                        LyDoThanhLy = tl.LyDoThanhLy,
                                        SoTienDaThanhToan = 0,
                                        GhiChu = tl.GhiChu,
                                        MaQuiTrinhDuyet = tl.MaQuiTrinhDuyet ?? 0,
                                        TrangThaiDuyet = tl.TrangThaiDuyet ?? 0,
                                        SoTienHoanTra = tl.TienHoanLai ?? 0,
                                    }).FirstOrDefaultAsync();
                if (entity == null)
                {
                    entity = new ThanhLyDatCocModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.MaPhieu = await SinhMaPhieuTuDongAsync("TLDC-", _context, 5);
                    entity.GiaBan = 0;
                    entity.PhiPhat = 0;
                    entity.SoTienDaThanhToan = 0;
                    entity.SoTienHoanTra = 0;
                    entity.NgayThanhLy = string.Format("{0:dd/MM/yyyy}", DateTime.Now);
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.NguoiThanhLy);
                    var ttnd = await _baseService.ThongTinNguoiDuyet("ThanhLyDatCoc", entity.MaPhieu);
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
                    var files = await _context.HtFileDinhKems.Where(d => d.Controller == "ThanhLyDatCoc" && d.MaPhieu == id).Select(d => new UploadedFileModel
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
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin phiếu đặc cọc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thêm, xóa , sửa
        public async Task<ResultModel> SavePhieuAsync(ThanhLyDatCocModel model, List<PhieuTLDCTienDoThanhToanModel> listTDTT)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new BhPhieuThanhLyDatCoc
                {
                    MaPhieuThanhLy = await SinhMaPhieuTuDongAsync("TLDC-", _context, 5),
                    SoPhieuThanhLy = model.SoPhieuThanhLy,
                    MaPhieuDatCoc = model.MaPhieuDatCoc,
                    NgayThanhLy = !string.IsNullOrEmpty(model.NgayThanhLy) ? DateTime.ParseExact(model.NgayThanhLy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now,
                    GhiChu = model.GhiChu,
                    LyDoThanhLy = model.LyDoThanhLy,
                    PhiPhat = model.PhiPhat,
                    HinhThucThanhLy = model.HinhThucThanhLy,
                    TienHoanLai = model.SoTienHoanTra,
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiThanhLy = NguoiLap.MaNhanVien;
                record.NgayLap = DateTime.Now;
                await _context.BhPhieuThanhLyDatCocs.AddAsync(record);
                //Insert tiến độ thanh toán chi tiết
                if (listTDTT != null & listTDTT.Any() == true)
                {
                    List<BhPhieuThanhLyDatCocChiTiet> listCT = new List<BhPhieuThanhLyDatCocChiTiet>();
                    foreach (var item in listTDTT)
                    {
                        var r = new BhPhieuThanhLyDatCocChiTiet();
                        r.MaPhieuTl = record.MaPhieuThanhLy;
                        r.MaCstt = item.MaCSTT;
                        r.DotTt = item.DotTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.MaKyTt = item.MaKyTT;
                        r.SoTienThanhToan = item.SoTienThanhToan;
                        r.SoTienDaTt = item.SoTienDaTT;
                        r.SoTienConLai = item.SoTienConLai;
                        listCT.Add(r);
                    }
                    await _context.BhPhieuThanhLyDatCocChiTiets.AddRangeAsync(listCT);
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
                            MaPhieu = record.MaPhieuThanhLy ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "ThanhLyDatCoc",
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
                return ResultModel.SuccessWithId(record.MaPhieuThanhLy, "Thêm phiếu thanh lý đặt cọc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm phiếu thanh lý đặt cọc");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu thanh lý đặt cọc: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuAsync(ThanhLyDatCocModel model, List<PhieuTLDCTienDoThanhToanModel> listTDTT)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = await _context.BhPhieuThanhLyDatCocs.FirstOrDefaultAsync(d => d.MaPhieuThanhLy == model.MaPhieu);
                if (record == null)
                    return ResultModel.Fail($"Không tìm thấy thông tin phiếu thanh lý đặt cọc");
                record.NgayThanhLy = !string.IsNullOrEmpty(model.NgayThanhLy) ? DateTime.ParseExact(model.NgayThanhLy, "dd/MM/yyyy", CultureInfo.InvariantCulture) : DateTime.Now;
                record.LyDoThanhLy = model.LyDoThanhLy;
                record.GhiChu = model.GhiChu;
                record.PhiPhat = model.PhiPhat;
                record.TienHoanLai = model.SoTienHoanTra;
                record.HinhThucThanhLy = model.HinhThucThanhLy;
                record.SoPhieuThanhLy = model.SoPhieuThanhLy;
                //Insert tiến độ thanh toán chi tiết
                var delTDTT = _context.BhPhieuThanhLyDatCocChiTiets.Where(d => d.MaPhieuTl == record.MaPhieuThanhLy);
                if (listTDTT != null & listTDTT.Any() == true)
                {
                    List<BhPhieuThanhLyDatCocChiTiet> listCT = new List<BhPhieuThanhLyDatCocChiTiet>();
                    foreach (var item in listTDTT)
                    {
                        var r = new BhPhieuThanhLyDatCocChiTiet();
                        r.MaPhieuTl = record.MaPhieuThanhLy;
                        r.MaCstt = item.MaCSTT;
                        r.DotTt = item.DotTT;
                        r.NoiDungTt = item.NoiDungTT;
                        r.MaKyTt = item.MaKyTT;
                        r.SoTienThanhToan = item.SoTienThanhToan;
                        r.SoTienDaTt = item.SoTienDaTT;
                        r.SoTienConLai = item.SoTienConLai;
                        listCT.Add(r);
                    }
                    await _context.BhPhieuThanhLyDatCocChiTiets.AddRangeAsync(listCT);
                }
                _context.BhPhieuThanhLyDatCocChiTiets.RemoveRange(delTDTT);
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await _context.HtFileDinhKems.Where(d => d.MaPhieu == record.MaPhieuThanhLy && d.Controller == "ThanhLyDatCoc").ToListAsync();
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
                            MaPhieu = record.MaPhieuThanhLy,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "ThanhLyDatCoc",
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
                return ResultModel.SuccessWithId(record.MaPhieuThanhLy, "Cập nhật phiếu thanh lý đặt cọc thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật  phiếu thanh lý đặt cọc");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật  phiếu thanh lý đặt cọc: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteAsync(string maPhieu)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var pdc = await _context.BhPhieuThanhLyDatCocs.Where(d => d.MaPhieuThanhLy == maPhieu).FirstOrDefaultAsync();
                if (pdc == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu thanh lý đặt cọc");
                }
                _context.BhPhieuThanhLyDatCocs.Remove(pdc);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {pdc.MaPhieuThanhLy} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteAsync] Lỗi khi xóa phiếu thanh lý đặt cọc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<ThanhLyDatCocPagingDto> listKeHoach)
        {
            try
            {
                var ids = listKeHoach?
            .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
            .Select(x => x.MaPhieu!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                var targetIds = ids;
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.BhPhieuThanhLyDatCocs
                .Where(d => targetIds.Contains(d.MaPhieuThanhLy))
                .ExecuteDeleteAsync();

                var c4 = await _context.HtDmnguoiDuyets
                    .Where(d => targetIds.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success($"Xóa danh sách thanh lý đặt cọc thành công");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách thanh lý đặt cọc");
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

        public async Task<List<BhPhieuDatCoc>> GetPhieuDatCocAsync(string maDuAn)
        {
            var entity = new List<BhPhieuDatCoc>();
            try
            {
                using var _context = _factory.CreateDbContext();
                entity = await (from pdc in _context.BhPhieuDatCocs
                                join ptl in _context.BhPhieuThanhLyDatCocs
                                    on pdc.MaPhieuDc equals ptl.MaPhieuDatCoc into g
                                from ptl in g.DefaultIfEmpty()
                                where ptl == null && pdc.MaDuAn == maDuAn
                                select pdc).ToListAsync();
                if (entity == null)
                {
                    entity = new List<BhPhieuDatCoc>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách phiếu đặt cọc chưa thanh lý");
            }
            return entity;
        }
        public async Task<List<BhHinhThucThanhLy>> GetHinhThucThanhLyAsync()
        {
            var entity = new List<BhHinhThucThanhLy>();
            try
            {
                using var _context = _factory.CreateDbContext();
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

        #region Hàm tăng tự động của mã phiếu thanh lý đặt cọc   
        public async Task<string> SinhMaPhieuTuDongAsync(string prefix, AppDbContext _context, int padding = 5)
        {
            var maLonNhat = await _context.BhPhieuThanhLyDatCocs
                .Where(kh => kh.MaPhieuThanhLy.StartsWith(prefix))
                .OrderByDescending(kh => kh.NgayLap)
                .Select(kh => kh.MaPhieuThanhLy)
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

        #region Thông tin chung phiếu thanh lý từ phiếu đăng ký rùi từ đặt cọc đi qua
        public async Task<ResultModel> GetByIdPhieuTLAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = new PhieuDatCocModel();
                if (id.StartsWith("PDK"))
                {
                    var thongTinPDK = await _context.BhPhieuDangKiChonCans.Where(d => d.MaPhieu == id).FirstOrDefaultAsync();
                    entity = await ThongTinChungPDC(id, string.Empty, thongTinPDK.MaDuAn, thongTinPDK.MaChinhSachTt);
                }
                else
                {
                    entity = await ThongTinChungPDC(string.Empty, id, string.Empty, string.Empty);
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdPhieuTLAsync] Lỗi khi lấy thông tin phiếu đăng ký hoặc đặt cọc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<PhieuDatCocModel> ThongTinChungPDC(string maPhieuDangKy, string maPhieuDatCoc, string maDuAn, string maChinhSacTT)
        {
            var entity = new PhieuDatCocModel();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaPhieuDangKy", maPhieuDangKy);
                param.Add("@MaPhieuDatCoc", maPhieuDatCoc);
                param.Add("@MaDuAn", maDuAn);
                param.Add("@MaChinhSacTT", maChinhSacTT);
                entity = (await connection.QueryAsync<PhieuDatCocModel>(
                "Proc_PhieuDatCoc_ThongTinChung",
                param,
                commandType: CommandType.StoredProcedure)).FirstOrDefault();
            }
            catch
            {
                entity = new PhieuDatCocModel();
            }
            return entity;
        }
        #endregion

        #region Thông tin tiến độ thanh toán
        public async Task<List<PhieuTLDCTienDoThanhToanModel>> TienDoThanhToan(string maPhieuDatCoc, string maPhieu)
        {
            var entity = new List<PhieuTLDCTienDoThanhToanModel>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaPhieuDC", maPhieuDatCoc);
                param.Add("@MaPhieu", maPhieu);
                entity = (await connection.QueryAsync<PhieuTLDCTienDoThanhToanModel>(
                "Proc_ThanhLyDatCoc_ChiTiet",
                param,
                commandType: CommandType.StoredProcedure)).ToList();
            }
            catch
            {
                entity = new List<PhieuTLDCTienDoThanhToanModel>();
            }
            return entity;
        }
        #endregion
    }
}

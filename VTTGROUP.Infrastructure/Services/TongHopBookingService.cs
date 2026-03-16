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
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.KeHoachBanHang;
using VTTGROUP.Domain.Model.PhieuGiuCho;
using VTTGROUP.Domain.Model.TongHopBooking;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class TongHopBookingService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<TongHopBookingService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public TongHopBookingService(IDbContextFactory<AppDbContext> factory, ILogger<TongHopBookingService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Danh sách tổng hợp phiếu booking
        public async Task<(List<TongHopBookingPagingDto> Data, int TotalCount)> GetPagingAsync(
     string? maDuAn, string? maSanGG, int page, int pageSize, string? qSearch, string? trangThai, string fromDate, string toDate)
        {
            try
            {
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                if (NguoiLap.LoaiUser == "SGG")
                {
                    maSanGG = NguoiLap.MaNhanVien;
                }
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

                var result = (await connection.QueryAsync<TongHopBookingPagingDto>(
                    "Proc_PhieuTongHopBooking_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;

                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sanh sách phiếu tổng hợp booking");
                var result = new List<TongHopBookingPagingDto>();
                return (result, 0);
            }
        }
        #endregion

        #region Thêm, xóa, sửa phiếu tổng hợp booking
        public async Task<ResultModel> SavePhieuAsync(TongHopBookingModel? model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                var maPhieu = await SinhMaPhieuTuDongAsync("THBK-", _context, 5);
                var record = new KdPhieuTongHopBooking();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.MaPhieu = maPhieu;
                record.MaDuAn = model.MaDuAn;
                record.MaSanGiaoDich = model.MaSanGiaoDich;
                record.NgayLap = DateTime.Now;
                record.NgayThu = !string.IsNullOrEmpty(model.NgayThu) ? DateTime.ParseExact(model.NgayThu, "dd/MM/yyyy", CultureInfo.InvariantCulture) : null;
                record.NoiDung = model.NoiDung;
                await _context.KdPhieuTongHopBookings.AddAsync(record);

                if (model.ListCT.Any())
                {
                    List<KdPhieuTongHopBookingPhieuBooking> listPGC = new List<KdPhieuTongHopBookingPhieuBooking>();
                    foreach (var item in model.ListCT)
                    {
                        var r = new KdPhieuTongHopBookingPhieuBooking
                        {
                            MaPhieuTh = record.MaPhieu,
                            MaBooking = item.MaBooking,
                            SoTien = item.SoTien,
                            GhiChu = item.GhiChu
                        };
                        listPGC.Add(r);
                    }
                    await _context.KdPhieuTongHopBookingPhieuBookings.AddRangeAsync(listPGC);
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
                            MaPhieu = record.MaPhieu ?? string.Empty,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "TongHopBooking",
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
                return ResultModel.SuccessWithId(record.MaPhieu, "Thêm tổng hợp booking thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm tổng hợp booking");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm tổng hợp booking: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuTHBKAsync(TongHopBookingModel? model)
        {
            if (model == null)
                return ResultModel.Fail("Dữ liệu tổng hợp booking không hợp lệ.");

            try
            {
                using var context = _factory.CreateDbContext();

                // 1. Lấy thông tin header
                var entity = await context.KdPhieuTongHopBookings
                    .FirstOrDefaultAsync(d => d.MaPhieu == model.MaPhieu);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy thông tin tổng hợp booking.");

                // 2. Cập nhật thông tin header
                entity.NgayThu = string.IsNullOrWhiteSpace(model.NgayThu)
                    ? null
                    : DateTime.ParseExact(model.NgayThu, "dd/MM/yyyy", CultureInfo.InvariantCulture);

                entity.NoiDung = model.NoiDung;

                // Đảm bảo ListCT không null
                var newDetails = (model.ListCT ?? new List<TongHopBookingCTModel>()).ToList();

                // 3. Lấy các chi tiết hiện tại trong DB
                var currentDetails = await context.KdPhieuTongHopBookingPhieuBookings
                    .Where(d => d.MaPhieuTh == entity.MaPhieu)
                    .ToListAsync();

                // 4. Đồng bộ PHIẾU GIỮ CHỖ (BH_PhieuGiuCho)
                // --------------------------------------------------------------------
                // Booking hiện có trong DB (trước khi sửa)
                var currentBookingCodes = currentDetails
                    .Select(x => x.MaBooking)            // MaBooking = mã PGC-xxxxx
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

                // Booking sau khi user chỉnh trên màn hình
                var newBookingCodes = newDetails
                    .Select(x => x.MaBooking)            // MaBooking = mã PGC-xxxxx
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

                // Những booking đã bị xóa khỏi tổng hợp booking
                var removedBookingCodes = currentBookingCodes
                    .Except(newBookingCodes)
                    .ToList();

                if (removedBookingCodes.Any())
                {
                    // Chỉ xoá những phiếu giữ chỗ thuộc Tổng hợp này
                    var phieuGiuChoToDelete = await context.BhPhieuGiuChos
                        .Where(x =>
                            x.MaPhieuTh == entity.MaPhieu &&     // thuộc THBK này
                            removedBookingCodes.Contains(x.MaPhieu)) // mã PGC đã bị xoá khỏi danh sách
                        .ToListAsync();

                    if (phieuGiuChoToDelete.Any())
                    {
                        context.BhPhieuGiuChos.RemoveRange(phieuGiuChoToDelete);
                    }
                }

                // 5. Ghi lại chi tiết tổng hợp booking
                // --------------------------------------------------------------------
                // Cách đơn giản: xoá hết chi tiết cũ và thêm lại từ ListCT
                context.KdPhieuTongHopBookingPhieuBookings.RemoveRange(currentDetails);

                if (newDetails.Any())
                {
                    var detailEntities = new List<KdPhieuTongHopBookingPhieuBooking>();

                    int stt = 1;
                    foreach (var item in newDetails)
                    {
                        var detail = new KdPhieuTongHopBookingPhieuBooking
                        {
                            MaPhieuTh = entity.MaPhieu,
                            MaBooking = item.MaBooking,
                            SoTien = item.SoTien,
                            GhiChu = item.GhiChu,
                            // Nếu bảng có cột STT thì set luôn:
                            // SoTT = stt++
                        };

                        detailEntities.Add(detail);
                        stt++;
                    }

                    await context.KdPhieuTongHopBookingPhieuBookings.AddRangeAsync(detailEntities);
                }

                //7. Đính kèm file
                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == entity.MaPhieu && d.Controller == "TongHopBooking").ToListAsync();

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
                            Controller = "TongHopBooking",
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
                    await context.HtFileDinhKems.AddRangeAsync(listFiles);
                }

                // 8. Lưu thay đổi
                await context.SaveChangesAsync();

                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật tổng hợp booking thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Lỗi khi cập nhật tổng hợp booking {MaPhieu}", model?.MaPhieu);

                return ResultModel.Fail("Lỗi hệ thống: Không thể cập nhật tổng hợp booking.");
            }
        }

        //public async Task<ResultModel> DeletePTHAsync(string maPhieu, string webRootPath)
        //{
        //    try
        //    {
        //        using var _context = _factory.CreateDbContext();
        //        var pdc = await _context.KdPhieuTongHopBookings.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
        //        if (pdc == null)
        //        {
        //            return ResultModel.Fail("Không tìm thấy phiếu tổng hợp");
        //        }

        //        var delPTTT = _context.KdPhieuTongHopBookingPhieuBookings.Where(d => d.MaPhieuTh == maPhieu);
        //        _context.KdPhieuTongHopBookings.Remove(pdc);
        //        _context.KdPhieuTongHopBookingPhieuBookings.RemoveRange(delPTTT);

        //        var delPGC = _context.BhPhieuGiuChos.Where(d => d.MaPhieuTh == maPhieu);
        //        _context.BhPhieuGiuChos.RemoveRange(delPGC);

        //        var delND = _context.HtDmnguoiDuyets.Where(d => d.MaPhieu == maPhieu);
        //        _context.HtDmnguoiDuyets.RemoveRange(delND);
        //        _context.SaveChanges();
        //        return ResultModel.Success($"Xóa {pdc.MaPhieu} thành công");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[DeletePTHAsync] Lỗi khi xóa phiếu tổng hợp");
        //        return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
        //    }
        //}

        public async Task<ResultModel> DeletePTHAsync(string maPhieu, string webRootPath)
        {
            if (string.IsNullOrWhiteSpace(maPhieu))
                return ResultModel.Fail("Mã phiếu không hợp lệ.");

            try
            {
                await using var context = _factory.CreateDbContext();

                // 1. Lấy phiếu tổng hợp
                var phieu = await context.KdPhieuTongHopBookings
                    .FirstOrDefaultAsync(d => d.MaPhieu == maPhieu);

                if (phieu == null)
                    return ResultModel.Fail("Không tìm thấy phiếu tổng hợp.");

                // 2. Lấy toàn bộ liên quan (chi tiết, PGC, người duyệt, file đính kèm)
                var chiTietList = await context.KdPhieuTongHopBookingPhieuBookings
                    .Where(d => d.MaPhieuTh == maPhieu)
                    .ToListAsync();

                var phieuGiuChoList = await context.BhPhieuGiuChos
                    .Where(d => d.MaPhieuTh == maPhieu)
                    .ToListAsync();

                var nguoiDuyetList = await context.HtDmnguoiDuyets
                    .Where(d => d.MaPhieu == maPhieu)
                    .ToListAsync();

                // Nếu muốn giới hạn theo Controller/Action thì thêm điều kiện
                var fileDinhKemList = await context.HtFileDinhKems
                    .Where(f => f.MaPhieu == maPhieu && f.Controller == "TongHopBooking")
                    .ToListAsync();

                // 3. Gom đường dẫn file vật lý để lát nữa xoá trên ổ đĩa
                var filePaths = new List<string>();

                foreach (var f in fileDinhKemList)
                {
                    // Ưu tiên TaiLieuURL, nếu null thì dùng TenFileDinhKemLuu
                    var mainPath = !string.IsNullOrWhiteSpace(f.TenFileDinhKemLuu)
                        ? f.TenFileDinhKemLuu
                        : f.TenFileDinhKemLuu;

                    if (!string.IsNullOrWhiteSpace(mainPath))
                    {
                        var fullPath = BuildPhysicalPath(webRootPath, mainPath);
                        if (!string.IsNullOrWhiteSpace(fullPath))
                            filePaths.Add(fullPath);
                    }                 
                }

                // 4. Xoá dữ liệu trong DB (theo đúng thứ tự phụ thuộc)
                context.KdPhieuTongHopBookingPhieuBookings.RemoveRange(chiTietList);
                context.BhPhieuGiuChos.RemoveRange(phieuGiuChoList);
                context.HtDmnguoiDuyets.RemoveRange(nguoiDuyetList);
                context.HtFileDinhKems.RemoveRange(fileDinhKemList);
                context.KdPhieuTongHopBookings.Remove(phieu);

                await context.SaveChangesAsync();

                // 5. Xoá file vật lý (best-effort, nếu lỗi chỉ log, không fail nghiệp vụ)
                foreach (var path in filePaths.Distinct())
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    catch (Exception exFile)
                    {
                        _logger.LogWarning(exFile,
                            "[DeletePTHAsync] Lỗi khi xoá file đính kèm: {Path}", path);
                        // Không throw để tránh làm fail cả hàm chỉ vì không xoá được file
                    }
                }

                return ResultModel.Success($"Xoá phiếu tổng hợp {maPhieu} thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePTHAsync] Lỗi khi xoá phiếu tổng hợp {MaPhieu}", maPhieu);
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }      

        public async Task<ResultModel> DeleteListAsync(
    List<TongHopBookingPagingDto> listTHBK,
    string webRootPath)
        {
            if (listTHBK == null || listTHBK.Count == 0)
                return ResultModel.Success("Không có dòng nào được chọn để xoá.");

            try
            {
                // 1. Lấy danh sách mã phiếu được chọn
                var ids = listTHBK
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
                    .Select(x => x!.MaPhieu.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var context = _factory.CreateDbContext();

                // 2. Lấy trước danh sách file đính kèm để xoá file vật lý sau khi xoá DB
                var fileDinhKemList = await context.HtFileDinhKems
                    .Where(f => ids.Contains(f.MaPhieu) && f.Controller == "TongHopBooking")
                    .ToListAsync();

                var filePaths = new List<string>();

                foreach (var f in fileDinhKemList)
                {
                    // File chính
                    var mainPath = !string.IsNullOrWhiteSpace(f.TenFileDinhKemLuu)
                        ? f.TenFileDinhKemLuu
                        : f.TenFileDinhKemLuu;

                    if (!string.IsNullOrWhiteSpace(mainPath))
                    {
                        var fullPath = BuildPhysicalPath(webRootPath, mainPath);
                        if (!string.IsNullOrWhiteSpace(fullPath))
                            filePaths.Add(fullPath);
                    }                 
                }

                // 3. Transaction xoá dữ liệu trong DB
                await using var tx = await context.Database.BeginTransactionAsync();

                // Chi tiết booking
                var cDetail = await context.KdPhieuTongHopBookingPhieuBookings
                    .Where(d => ids.Contains(d.MaPhieuTh))
                    .ExecuteDeleteAsync();

                // Người duyệt
                var cApprover = await context.HtDmnguoiDuyets
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                // Phiếu giữ chỗ
                var cPhieuGiuCho = await context.BhPhieuGiuChos
                    .Where(d => ids.Contains(d.MaPhieuTh))
                    .ExecuteDeleteAsync();

                // File đính kèm
                var cFiles = await context.HtFileDinhKems
                    .Where(f => ids.Contains(f.MaPhieu))
                    .ExecuteDeleteAsync();

                // Header
                var cParent = await context.KdPhieuTongHopBookings
                    .Where(k => ids.Contains(k.MaPhieu))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                // 4. Xoá file vật lý (best-effort)
                foreach (var path in filePaths.Distinct())
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Delete(path);
                        }
                    }
                    catch (Exception exFile)
                    {
                        _logger.LogWarning(exFile,
                            "[DeleteListAsync] Lỗi khi xoá file đính kèm: {Path}",
                            path);
                        // Không throw để tránh fail cả nghiệp vụ chỉ vì lỗi xoá file
                    }
                }

                return ResultModel.Success(
                    $"Đã xoá {cParent} phiếu, {cDetail} chi tiết booking, {cApprover} người duyệt, {cPhieuGiuCho} phiếu giữ chỗ, {cFiles} file đính kèm.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "[DeleteListAsync] Lỗi khi xoá danh sách phiếu tổng hợp booking");

                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        /// <summary>
        /// Chuẩn hoá path: ghép webRootPath + relativePath (uploads/xxxx)
        /// </summary>
        private static string? BuildPhysicalPath(string webRootPath, string relativePath)
        {
            if (string.IsNullOrWhiteSpace(webRootPath) || string.IsNullOrWhiteSpace(relativePath))
                return null;

            // Bỏ ~, / đầu và chuẩn hoá slash
            var clean = relativePath.Trim().TrimStart('~', '/', '\\');
            clean = clean.Replace('/', Path.DirectorySeparatorChar)
                         .Replace('\\', Path.DirectorySeparatorChar);

            return Path.Combine(webRootPath, clean);
        }
        #endregion

        #region Thông tin tổng hợp phiếu booking
        public async Task<ResultModel> FindGetByPhieuAsync(string? id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new TongHopBookingModel();
                if (!string.IsNullOrEmpty(id))
                {
                    record = await (
                      from sp in _context.KdPhieuTongHopBookings
                      join duan in _context.DaDanhMucDuAns on sp.MaDuAn equals duan.MaDuAn

                      join san in _context.DmSanGiaoDiches on sp.MaSanGiaoDich equals san.MaSanGiaoDich into sanGroup
                      from san in sanGroup.DefaultIfEmpty()

                      where sp.MaPhieu == id
                      select new TongHopBookingModel
                      {
                          MaPhieu = sp.MaPhieu,
                          NgayLap = sp.NgayLap,
                          MaDuAn = sp.MaDuAn,
                          TenDuAn = duan.TenDuAn,
                          NoiDung = sp.NoiDung,
                          MaNhanVien = sp.NguoiLap,
                          TenSanGiaoDich = san.TenSanGiaoDich ?? string.Empty,
                          MaSanGiaoDich = san.MaSanGiaoDich,
                          MaQuiTrinhDuyet = sp.MaQuiTrinhDuyet ?? 0,
                          TrangThaiDuyet = sp.TrangThaiDuyet ?? 0,
                          NgayThu = string.Format("{0:dd/MM/yyyy}", sp.NgayThu),
                      }).FirstOrDefaultAsync();
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);
                    record.ListCT = await (from gh in _context.KdPhieuTongHopBookingPhieuBookings
                                           join bk in _context.BhPhieuGiuChos on gh.MaBooking equals bk.MaPhieu into dtBooking
                                           from bk2 in dtBooking.DefaultIfEmpty()
                                           join kh in _context.KhDmkhachHangTams on bk2.MaKhachHangTam equals kh.MaKhachHangTam into dtDong
                                           from kh2 in dtDong.DefaultIfEmpty()

                                           join dtkh in _context.KhDmdoiTuongKhachHangs on kh2.MaDoiTuongKhachHang equals dtkh.MaDoiTuongKhachHang into dtDTKH
                                           from dtkh2 in dtDTKH.DefaultIfEmpty()

                                           join lc in _context.KhDmloaiCards on kh2.MaLoaiIdCard equals lc.MaLoaiIdCard into dtLC
                                           from lc2 in dtLC.DefaultIfEmpty()
                                           where gh.MaPhieuTh == id
                                           select new TongHopBookingCTModel
                                           {
                                               MaPhieuTH = gh.MaPhieuTh,
                                               MaBooking = gh.MaBooking,
                                               MaKhachHang = bk2.MaKhachHangTam,
                                               TenKhachHang = kh2.TenKhachHang,
                                               TenDoiTuongKH = dtkh2.TenDoiTuongKhachHang,
                                               TenLoaiIDCard = lc2.TenLoaiIdCard,
                                               IDCard = kh2.IdCard,
                                               SoTien = gh.SoTien,
                                               GhiChu = gh.GhiChu
                                           }).ToListAsync();
                    var ttnd = await _baseService.ThongTinNguoiDuyet("TongHopBooking", record.MaPhieu);
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
                    var files = await _context.HtFileDinhKems.Where(d => d.Controller == "TongHopBooking" && d.MaPhieu == id).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType
                    }).ToListAsync();
                    record.Files = files;
                }
                else
                {
                    record.MaPhieu = await SinhMaPhieuTuDongAsync("THBK-", _context, 5);
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    record.NgayLap = DateTime.Now;
                    if (record.NguoiLap.LoaiUser == "SGG")
                    {
                        record.MaSanGiaoDich = record.NguoiLap.MaSanGiaoDich;
                        record.TenSanGiaoDich = record.NguoiLap.TenSanGiaoDich;
                    }
                }
                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (Exception ex)
            {
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin tổng hợp booking: {ex.Message}");
            }
        }
        #endregion

        #region Hàm tăng tự động của phiếu tổng hợp booking 
        public async Task<string> SinhMaPhieuTuDongAsync(string prefix, AppDbContext _context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KdPhieuTongHopBookings
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

        #region Danh sách combobox 
        public async Task<List<DmSanGiaoDich>> GetSanGiaoDichAsync()
        {
            var entity = new List<DmSanGiaoDich>();
            try
            {
                using var _context = _factory.CreateDbContext();
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
        public async Task<List<DuAnTheoSanModel>> GetByDuAnTheoSanAsync()
        {
            var entity = new List<DuAnTheoSanModel>();
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                if (NguoiLap.LoaiUser == "SGG")
                {
                    entity = await (from da in _context.DaDanhMucDuAns
                                    join san in _context.DmSanGiaoDichDuAns
                                        on da.MaDuAn equals san.MaDuAn
                                    where san.MaSan == NguoiLap.MaNhanVien
                                    select new DuAnTheoSanModel
                                    {
                                        MaDuAn = da.MaDuAn,
                                        TenDuAn = da.TenDuAn
                                    }).ToListAsync();
                }
                else
                {
                    entity = await _context.DaDanhMucDuAns.Select(d => new DuAnTheoSanModel
                    {
                        MaDuAn = d.MaDuAn,
                        TenDuAn = d.TenDuAn
                    }).ToListAsync();
                    if (entity == null)
                    {
                        entity = new List<DuAnTheoSanModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách dự án");
            }
            return entity;
        }
        #endregion

        #region Thông tin phiếu giữ chỗ chưa lên tổng hợp booking
        public async Task<(List<TongHopBookingPhieuGiuChoModel> Data, int TotalCount)> GetPagingPGCPopupAsync(
      string? maDuAn, string? maSanGG, int page, int pageSize, string? qSearch)
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

                var result = (await connection.QueryAsync<TongHopBookingPhieuGiuChoModel>(
                    "Proc_TongHopBooking_PhieuGCPopup_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;

                return (result, total);
            }
            catch
            {
                var result = new List<TongHopBookingPhieuGiuChoModel>();
                return (result, 0);
            }
        }

        #endregion

        #region Import, download file mẫu        
        public async Task<byte[]> GenerateTemplateWithDataAsync(string templatePath)
        {
            // Copy file template từ wwwroot vào memory stream
            using var memoryStream = new MemoryStream(File.ReadAllBytes(templatePath));
            using var workbook = new XLWorkbook(memoryStream);
            // Set lại sheet "SanPham" là active
            //  workbook.Worksheet("Sheet1").SetTabActive();

            // Ghi ra stream
            using var outputStream = new MemoryStream();
            workbook.SaveAs(outputStream);
            return outputStream.ToArray();
        }
        public async Task<ResultModel> ImportFromExcelAsync(IBrowserFile file, string maSanGD, string maNhanVien, string maDuAn, string MaTongHopBK)
        {
            try
            {
                using var inputStream = file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024);
                using var memoryStream = new MemoryStream();
                await inputStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // Reset lại vị trí đọc

                var items = await ReadPhieuGiuChoFromExcel(memoryStream, maSanGD, maNhanVien, maDuAn, MaTongHopBK);
                if (items.Count == 0)
                    return ResultModel.Fail("Không có phiếu giữ chỗ nào được import vào. Vì đã được lên tổng hợp rùi.");
                using var _context = _factory.CreateDbContext();
                var dataTHBK = await (from gh in _context.KdPhieuTongHopBookingPhieuBookings
                                      join bk in _context.BhPhieuGiuChos on gh.MaBooking equals bk.MaPhieu into dtBooking
                                      from bk2 in dtBooking.DefaultIfEmpty()
                                      join kh in _context.KhDmkhachHangTams on bk2.MaKhachHangTam equals kh.MaKhachHangTam into dtDong
                                      from kh2 in dtDong.DefaultIfEmpty()

                                      join dtkh in _context.KhDmdoiTuongKhachHangs on kh2.MaDoiTuongKhachHang equals dtkh.MaDoiTuongKhachHang into dtDTKH
                                      from dtkh2 in dtDTKH.DefaultIfEmpty()

                                      join lc in _context.KhDmloaiCards on kh2.MaLoaiIdCard equals lc.MaLoaiIdCard into dtLC
                                      from lc2 in dtLC.DefaultIfEmpty()
                                      where gh.MaPhieuTh == MaTongHopBK
                                      select new TongHopBookingCTModel
                                      {
                                          MaPhieuTH = gh.MaPhieuTh,
                                          MaBooking = gh.MaBooking,
                                          MaKhachHang = bk2.MaKhachHangTam,
                                          TenKhachHang = kh2.TenKhachHang,
                                          TenDoiTuongKH = dtkh2.TenDoiTuongKhachHang,
                                          TenLoaiIDCard = lc2.TenLoaiIdCard,
                                          IDCard = kh2.IdCard,
                                          SoTien = gh.SoTien,
                                          GhiChu = bk2.NoiDung
                                      }).ToListAsync();
                return ResultModel.SuccessWithData(dataTHBK, "Import file thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi định dạng file Excel");
                return ResultModel.Fail($"Lỗi định dạng file: {ex.Message}");
            }
        }

        public async Task<List<TongHopBookingCTModel>> ReadPhieuGiuChoFromExcel(Stream stream, string maSanGD, string maNhanVien, string maDuAn, string maTongHopBK)
        {
            using var _context = _factory.CreateDbContext();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var reader = ExcelReaderFactory.CreateReader(stream);
            var dataset = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            });
            var table = dataset.Tables["Sheet1"];
            if (table == null)
                throw new Exception("Không tìm thấy sheet 'Sheet1' trong file Excel.");

            //Delete tất cả phiếu tổng hợp booking tạo tự động khi import vô 
            var delPTTT = _context.KdPhieuTongHopBookingPhieuBookings.Where(d => d.MaPhieuTh == maTongHopBK);
            _context.KdPhieuTongHopBookingPhieuBookings.RemoveRange(delPTTT);

            var delPGC = _context.BhPhieuGiuChos.Where(d => d.MaPhieuTh == maTongHopBK);
            _context.BhPhieuGiuChos.RemoveRange(delPGC);

            var list = new List<TongHopBookingCTModel>();
            foreach (DataRow row in table.Rows)
            {
                string? doiTuongKH = row[0]?.ToString()?.Trim().ToUpper();

                // Cột thứ 1 (index 2)
                string? tenKH = row[1]?.ToString()?.Trim();

                // Cột thứ 2 (index 3)
                string? loaiID = row[2]?.ToString()?.Trim().ToUpper();

                // Cột thứ 3 (index 4)
                string? soIdCard = row[3]?.ToString()?.Trim().ToUpper();

                // Cột thứ 4 (index 6)
                string? ghiChu = row[4]?.ToString()?.Trim();

                if (string.IsNullOrWhiteSpace(doiTuongKH) || string.IsNullOrWhiteSpace(tenKH) || string.IsNullOrWhiteSpace(loaiID) || string.IsNullOrWhiteSpace(soIdCard))
                {
                    continue;
                }
                //Kiểm tra mã đối tượng khách hàng
                var flagDTKH = await _context.KhDmdoiTuongKhachHangs.Where(d => d.MaDoiTuongKhachHang.ToUpper() == doiTuongKH).FirstOrDefaultAsync();
                if (flagDTKH == null)
                {
                    continue;
                }
                //Kiểm tra mã loại idCard
                var flagIDCard = await _context.KhDmloaiCards.Where(d => d.MaLoaiIdCard.ToUpper() == loaiID).FirstOrDefaultAsync();
                if (flagIDCard == null)
                {
                    continue;
                }
                //Insert khách hàng tạm
                string maKHT = await InsertKhachHangTam(maNhanVien, tenKH, doiTuongKH ?? string.Empty, loaiID, soIdCard, maDuAn, maSanGD, _context);
                //Insert phiếu giữ chỗ
                string maPhieuGC = await InsertPGC(maTongHopBK, ghiChu ?? string.Empty, maKHT, maNhanVien, maDuAn, maSanGD, _context);
                if (!string.IsNullOrEmpty(maTongHopBK))//Trường hợp edit
                {
                    //Kiểm tra phiếu giữ chỗ này đã lên tổng hợp booking hay chưa
                    var flagTHBK = await _context.KdPhieuTongHopBookingPhieuBookings.Where(d => d.MaBooking == maPhieuGC).FirstOrDefaultAsync();
                    if (flagTHBK == null)
                    {
                        KdPhieuTongHopBookingPhieuBooking phieuTH = new KdPhieuTongHopBookingPhieuBooking();
                        phieuTH.MaPhieuTh = maTongHopBK;
                        phieuTH.MaBooking = maPhieuGC;
                        phieuTH.SoTien = _context.DaDanhMucDuAnCauHinhChungs.Where(d => d.MaDuAn == maDuAn).Select(d => d.SoTienGiuCho).FirstOrDefault() ?? 0;
                        phieuTH.GhiChu = ghiChu;
                        //"Tổng hợp booking tự sinh khi import file";
                        await _context.KdPhieuTongHopBookingPhieuBookings.AddAsync(phieuTH);
                    }
                }
                _context.SaveChanges();
                TongHopBookingCTModel ct = new TongHopBookingCTModel();
                ct.MaPhieuTH = maPhieuGC;
                list.Add(ct);
            }
            return list;
        }
        //Insert khách hàng tạm
        public async Task<string> InsertKhachHangTam(string maNhanVien, string tenKH, string doiTuongKH, string maLoaiIdCard, string idCard, string maDuAn, string maSanGD, AppDbContext _context)
        {
            var flagKHT = await _context.KhDmkhachHangTams.Where(d => d.MaLoaiIdCard.ToUpper() == maLoaiIdCard & d.IdCard.ToUpper() == idCard && d.MaSanGd == maSanGD).FirstOrDefaultAsync();
            if (flagKHT == null)
            {
                KhDmkhachHangTam kht = new KhDmkhachHangTam();
                kht.MaKhachHangTam = await GenerateMaKhachHangAsync(_context);
                kht.TenKhachHang = tenKH;
                kht.NgayLap = DateTime.Now;
                kht.MaNhanVien = maNhanVien;
                kht.MaDoiTuongKhachHang = doiTuongKH;
                kht.MaLoaiIdCard = maLoaiIdCard;
                kht.IdCard = idCard;
                kht.MaDuAn = maDuAn;
                kht.MaSanGd = maSanGD;
                kht.GhiChu = "Khách hàng tạm tự sinh khi import file tổng hợp booking";
                await _context.KhDmkhachHangTams.AddAsync(kht);
                return kht.MaKhachHangTam;
            }
            else
            {
                return flagKHT.MaKhachHangTam;
            }
        }

        public async Task<string> InsertPGC(string maPhieuTh, string ghiChu, string maKhachHangTam, string maNhanVien, string maDuAn, string maSanGD, AppDbContext _context)
        {
            BhPhieuGiuCho pgc = new BhPhieuGiuCho();
            pgc.MaPhieu = await GeneratePGCAsync(_context);
            pgc.NguoiLap = maNhanVien;
            pgc.NgayLap = DateTime.Now;
            // pgc.IsxacNhan = true;
            pgc.MaDuAn = maDuAn;
            pgc.MaSanMoiGioi = maSanGD;
            pgc.MaKhachHangTam = maKhachHangTam;
            pgc.SoTienGiuCho = _context.DaDanhMucDuAnCauHinhChungs.Where(d => d.MaDuAn == maDuAn).Select(d => d.SoTienGiuCho).FirstOrDefault() ?? 0;
            pgc.DotMoBan = _context.DaDanhMucDotMoBans.Where(d => d.MaDuAn == maDuAn && d.IsKichHoat == true).Select(d => d.MaDotMoBan).FirstOrDefault();
            pgc.MaPhieuTh = maPhieuTh;
            pgc.NoiDung = ghiChu;
            //"Phiếu giữ chỗ tự sinh khi import file tổng hợp booking";
            await _context.BhPhieuGiuChos.AddAsync(pgc);
            return pgc.MaPhieu;
        }

        public async Task<string> GenerateMaKhachHangAsync(AppDbContext _context)
        {
            var lastMaNV = await _context.KhDmkhachHangTams
            .Where(x => x.MaKhachHangTam.StartsWith("KHT-"))
            .OrderByDescending(x => x.MaKhachHangTam)
            .Select(x => x.MaKhachHangTam)
            .FirstOrDefaultAsync();
            return _baseService.GenerateNextCode("KHT", lastMaNV);
        }

        public async Task<string> GeneratePGCAsync(AppDbContext _context)
        {
            var lastMaNV = await _context.BhPhieuGiuChos
            .Where(x => x.MaPhieu.StartsWith("PGC-"))
            .OrderByDescending(x => x.NgayLap)
            .Select(x => x.MaPhieu)
            .FirstOrDefaultAsync();
            return _baseService.GenerateNextCode("PGC", lastMaNV);
        }
        #endregion

        #region Nhân viên sàn giao dịch gửi duyệt sẽ đẩy đi trực tiếp tới 1 qui trình duyệt mới nhất và sẽ tự động đẩy người duyệt mặc định trong qui trình duyệt ra
        public async Task<string> DuyetTheoQuiTrinhSGGBatKy(string maCongViec, string maPhieu, string maNhanVien)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                int idQuiTrinh = (int?)await _context.HtQuyTrinhDuyets.Where(d => d.MaCongViec == maCongViec).OrderByDescending(d => d.Id).Select(d => d.Id).FirstOrDefaultAsync() ?? 0;
                if (idQuiTrinh == 0)
                {
                    return "ChuaTonTai";
                }
                else
                {
                    using var context = _factory.CreateDbContext();
                    maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                    using var connection = new SqlConnection(_connectionString);
                    var param = new DynamicParameters();
                    param.Add("@MaCongViec", maCongViec);
                    param.Add("@MaPhieu", maPhieu);
                    param.Add("@MaNhanVien", maNhanVien);
                    param.Add("@IdQuyTrinh", idQuiTrinh);
                    param.Add("@NoiDung", string.Empty);
                    var result = (await connection.QueryAsync<ThongTinPhieuDuyetModel>(
                           "Proc_DuyetPhieuKeTiep",
                           param,
                           commandType: CommandType.StoredProcedure
                       )).FirstOrDefault();
                    if (result != null)
                    {
                        var capNhatPhieu = await _context.KdPhieuTongHopBookings.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                        capNhatPhieu.TrangThaiDuyet = result.TrangThai;
                        capNhatPhieu.MaQuiTrinhDuyet = idQuiTrinh;
                        await _context.SaveChangesAsync();
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
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

    }
}

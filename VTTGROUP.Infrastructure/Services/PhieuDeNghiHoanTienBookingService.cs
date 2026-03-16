using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Globalization;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.HopDongChuyenNhuong;
using VTTGROUP.Domain.Model.PhieuDeNghiHoanTienBooking;
using VTTGROUP.Domain.Model.PhieuGiuCho;
using VTTGROUP.Domain.Model.TongHopBooking;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhieuDeNghiHoanTienBookingService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<PhieuDeNghiHoanTienBookingService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;

        public PhieuDeNghiHoanTienBookingService(IDbContextFactory<AppDbContext> factory, ILogger<PhieuDeNghiHoanTienBookingService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Danh sách phiếu đề nghị hoàn tiền booking
        public async Task<(List<PhieuDeNghiHoanTienBookingPagingDto> Data, int TotalCount)> GetPagingAsync(
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

                var result = (await connection.QueryAsync<PhieuDeNghiHoanTienBookingPagingDto>(
                    "Proc_PhieuDeNghiHoanTienBooking_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;

                return (result, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi hiển thị sanh sách phiếu đề nghị hoàn tiền booking");
                var result = new List<PhieuDeNghiHoanTienBookingPagingDto>();
                return (result, 0);
            }
        }
        #endregion

        #region Thông tin tổng hợp phiếu booking
        public async Task<ResultModel> FindGetByPhieuAsync(string? id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new PhieuDeNghiHoanTienBookingModel();
                if (!string.IsNullOrEmpty(id))
                {
                    record = await (
                      from sp in _context.KdPhieuDeNghiHoanTienBookings
                      join duan in _context.DaDanhMucDuAns on sp.MaDuAn equals duan.MaDuAn

                      join san in _context.DmSanGiaoDiches on sp.MaSanGiaoDich equals san.MaSanGiaoDich into sanGroup
                      from san in sanGroup.DefaultIfEmpty()

                      where sp.MaPhieu == id
                      select new PhieuDeNghiHoanTienBookingModel
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
                      }).FirstOrDefaultAsync();
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(record.MaNhanVien);
                    record.ListCT = await (from gh in _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings
                                           join th in _context.KdPhieuTongHopBookingPhieuBookings on gh.MaPhieuTongHopThu equals th.MaPhieuTh into dtTH
                                           from th in dtTH.DefaultIfEmpty()

                                           join bk in _context.BhPhieuGiuChos on gh.MaPhieuBooking equals bk.MaPhieu into dtBooking
                                           from bk2 in dtBooking.DefaultIfEmpty()

                                           join kh in _context.KhDmkhachHangTams on bk2.MaKhachHangTam equals kh.MaKhachHangTam into dtDong
                                           from kh2 in dtDong.DefaultIfEmpty()

                                           join dtkh in _context.KhDmdoiTuongKhachHangs on kh2.MaDoiTuongKhachHang equals dtkh.MaDoiTuongKhachHang into dtDTKH
                                           from dtkh2 in dtDTKH.DefaultIfEmpty()

                                           join lc in _context.KhDmloaiCards on kh2.MaLoaiIdCard equals lc.MaLoaiIdCard into dtLC
                                           from lc2 in dtLC.DefaultIfEmpty()
                                           where gh.MaPhieuHoanTien == id
                                           select new PhieuDeNghiHoanTienBookingCTModel
                                           {
                                               MaPhieu = gh.MaPhieuHoanTien,
                                               MaPhieuTHThu = th.MaPhieuTh,
                                               MaBooking = gh.MaPhieuBooking,
                                               MaKhachHang = bk2.MaKhachHangTam,
                                               TenKhachHang = kh2.TenKhachHang,
                                               TenDoiTuongKH = dtkh2.TenDoiTuongKhachHang,
                                               TenLoaiIDCard = lc2.TenLoaiIdCard,
                                               IDCard = kh2.IdCard,
                                               SoTien = gh.SoTien,
                                               GhiChu = bk2.NoiDung
                                           })
                       .Distinct()   // loại trùng hẳn
                       .ToListAsync();
                    var ttnd = await _baseService.ThongTinNguoiDuyet("PDNHT", record.MaPhieu);
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
                }
                else
                {
                    record.MaPhieu = await SinhMaPhieuTuDongAsync("PDNHTBK-", _context, 5);
                    record.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    record.NgayLap = DateTime.Now;
                    if (record.NguoiLap.LoaiUser == "SGG")
                    {
                        record.MaSanGiaoDich = record.NguoiLap.MaNhanVien;
                        record.TenSanGiaoDich = record.NguoiLap.HoVaTen;
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

        #region Thêm, xóa, sửa phiếu đề nghị hoàn tiền booking
        public async Task<ResultModel> SavePhieuAsync(PhieuDeNghiHoanTienBookingModel? model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                var maPhieu = await SinhMaPhieuTuDongAsync("PDNHTBK-", _context, 5);
                var record = new KdPhieuDeNghiHoanTienBooking();
                record.NguoiLap = NguoiLap.MaNhanVien;
                record.MaPhieu = maPhieu;
                record.MaDuAn = model.MaDuAn;
                record.MaSanGiaoDich = model.MaSanGiaoDich;
                record.NgayLap = DateTime.Now;
                record.NoiDung = model.NoiDung;
                await _context.KdPhieuDeNghiHoanTienBookings.AddAsync(record);

                if (model.ListCT.Any())
                {
                    List<KdPhieuDeNghiHoanTienBookingSoPhieuBooking> listPGC = new List<KdPhieuDeNghiHoanTienBookingSoPhieuBooking>();
                    foreach (var item in model.ListCT)
                    {
                        var r = new KdPhieuDeNghiHoanTienBookingSoPhieuBooking
                        {
                            MaPhieuHoanTien = record.MaPhieu,
                            MaPhieuTongHopThu = item.MaPhieuTHThu,
                            MaPhieuBooking = item.MaBooking,                            
                            SoTien = item.SoTien,
                            GhiChu = item.GhiChu
                        };
                        listPGC.Add(r);
                    }
                    await _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings.AddRangeAsync(listPGC);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaPhieu, "Thêm phiếu đề nghị hoàn tiền booking thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi Thêm phiếu đề nghị hoàn tiền booking");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm phiếu đề nghị hoàn tiền booking: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuAsync(PhieuDeNghiHoanTienBookingModel? model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.KdPhieuDeNghiHoanTienBookings.FirstOrDefaultAsync(d => d.MaPhieu == model.MaPhieu);
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy thông tin phiếu đề nghị hoàn tiền booking");

                entity.NoiDung = model.NoiDung;

                var del = await _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings.Where(d => d.MaPhieuHoanTien == entity.MaPhieu).ToListAsync();
                _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings.RemoveRange(del);

                if (model.ListCT.Any())
                {
                    List<KdPhieuDeNghiHoanTienBookingSoPhieuBooking> listPGC = new List<KdPhieuDeNghiHoanTienBookingSoPhieuBooking>();
                    foreach (var item in model.ListCT)
                    {
                        var r = new KdPhieuDeNghiHoanTienBookingSoPhieuBooking
                        {
                            MaPhieuHoanTien = entity.MaPhieu,
                            MaPhieuTongHopThu = item.MaPhieuTHThu,
                            MaPhieuBooking = item.MaBooking,
                            SoTien = item.SoTien,
                            GhiChu = item.GhiChu
                        };
                        listPGC.Add(r);
                    }
                    await _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings.AddRangeAsync(listPGC);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.MaPhieu, "Cập nhật phiếu đề nghị hoàn tiền booking thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phiếu đề nghị hoàn tiền booking");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể cập nhật phiếu đề nghị hoàn tiền booking: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeletePTHAsync(string maPhieu, string webRootPath)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var pdc = await _context.KdPhieuDeNghiHoanTienBookings.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (pdc == null)
                {
                    return ResultModel.Fail("Không tìm thấy phiếu đề nghị hoàn tiền booking");
                }

                var delPTTT = _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings.Where(d => d.MaPhieuHoanTien == maPhieu);
                _context.KdPhieuDeNghiHoanTienBookings.Remove(pdc);
                _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings.RemoveRange(delPTTT);

                var delND = _context.HtDmnguoiDuyets.Where(d => d.MaPhieu == maPhieu);
                _context.HtDmnguoiDuyets.RemoveRange(delND);
                _context.SaveChanges();
                return ResultModel.Success($"Xóa {pdc.MaPhieu} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePTHAsync] Lỗi khi xóa phiếu đề nghị hoàn tiền booking");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteListAsync(List<PhieuDeNghiHoanTienBookingPagingDto> listHTBK)
        {
            try
            {
                var ids = listHTBK?
                    .Where(x => x?.IsSelected == true && !string.IsNullOrWhiteSpace(x.MaPhieu))
                    .Select(x => x!.MaPhieu.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList() ?? new List<string>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                await using var _context = _factory.CreateDbContext();

                // --- B2: Transaction xóa dữ liệu DB ---
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings
                    .Where(d => ids.Contains(d.MaPhieuBooking))
                    .ExecuteDeleteAsync();

                var c3 = await _context.HtDmnguoiDuyets
                    .Where(d => ids.Contains(d.MaPhieu))
                    .ExecuteDeleteAsync();

                var cParent = await _context.KdPhieuDeNghiHoanTienBookings
                    .Where(k => ids.Contains(k.MaPhieu))
                    .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success("Đã xóa phiếu đề nghị hoàn tiền booking thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi  xóa phiếu đề nghị hoàn tiền booking thành công");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Thông tin phiếu tổng hợp đã duyệt và chưa lên đề nghị hoàn tiền booking
        public async Task<(List<PhieuDeNghiHoanTienTongHopBookingModel> Data, int TotalCount)> GetPagingTongHopBookingPopupAsync(
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

                var result = (await connection.QueryAsync<PhieuDeNghiHoanTienTongHopBookingModel>(
                    "Proc_DeNghiHoanTienBooking_TongHopBooking_GetPaging",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();

                int total = result.FirstOrDefault()?.TotalCount ?? 0;

                return (result, total);
            }
            catch
            {
                var result = new List<PhieuDeNghiHoanTienTongHopBookingModel>();
                return (result, 0);
            }
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

        #region Hàm tăng tự động của phiếu tổng hợp booking 
        public async Task<string> SinhMaPhieuTuDongAsync(string prefix, AppDbContext _context, int padding = 5)
        {
            // B1: Tìm mã mới nhất có tiền tố (ORDER theo phần số giảm dần)
            var maLonNhat = await _context.KdPhieuDeNghiHoanTienBookings
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

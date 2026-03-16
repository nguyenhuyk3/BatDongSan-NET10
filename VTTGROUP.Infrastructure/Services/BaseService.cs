using Dapper;
using Microsoft.AspNetCore.Components;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.CanHo;
using VTTGROUP.Domain.Model.CongTrinh;
using VTTGROUP.Domain.Model.DuAn;
using VTTGROUP.Domain.Model.Hubs;
using VTTGROUP.Domain.Model.KeHoachBanHang;
using VTTGROUP.Domain.Model.PhieuDatCoc;
using VTTGROUP.Domain.Model.QuyenVaVuViecTheoNhanVien;
using VTTGROUP.Domain.Model.ThongTinCongTy;
using VTTGROUP.Infrastructure.Database;
using VTTGROUP.Infrastructure.Database2;

namespace VTTGROUP.Infrastructure.Services
{
    public interface IBaseService
    {
        string GenerateNextCode(string prefix, string? lastCode);
        string GenerateNextCodeERP(string preString, string maxValue, int length);
        Task AddEmailToSend(string email, string subject, string bodyHtml, string nguoiLap);
        Task<bool> KiemTraQuyenVuViec(string maCongViec, string maVuViec);
        Task<List<CongViecVaVuViecCuaUserName>> GetQuyenVuViecChiTietAsync(string maCongViec, string maVuViec = null);
        Task<(List<DanhMucDuAnPagingDto> Data, int TotalCount)> DuAnPagingAsync(int page, int pageSize, string? qSearch);
        Task CheckPermissionAsync(string menu, string vuViec);
        Task<bool> CheckPermissionTraVeAsync(string menu, string vuViec);
        Task<List<HtTrangThaiDuyet>> GetListTrangThaiDuyetAsync();
        Task<string> MaNhanVienHienTai();
        Task<List<QuiTrinhDuyetDto>> GetQuyTrinhDuyetAsyn(string maCongViec, string maPhieu, string maCongTrinh, string maNhanVien, int? idQuyTrinh);
        Task<bool> DuyetPhieuTheoQuiTrinhDong(string maCongViec, string maPhieu, int? idQuyTrinh, string maNhanVien, string noiDung, int flag);
        Task<HtDmnguoiDuyet> ThongTinNguoiDuyet(string maCongViec, string maPhieu);
        Task<int> BuocDuyetCuoi(int idQuiTrinh);
        Task<bool> TraLaiPhieu(string maCongViec, string maPhieu, string noiDung);
        Task<List<LichSuDuyetPhieuDto>> GetLichSuDuyetPhieuAsyn(string maCongViec, string maPhieu);
        Task<List<DmSanGiaoDich>> GetListSGGAsyn(string maDuAn);
        Task<string> NhanVienThuocSGG();
        Task<List<CongViecDuyetModel>> GetDanhSachPhieuDuyet();
        Task<SoDoCanHoModel> ThongTinChiTietSanPham(string maDuAn, string maSanPham);
        Task<string> HienTrangKinhDoanhSanPham(string maDuAn, string maSanPham);
        Task<ThongTinCongTyModel> ThongTinCongTyAsync();
        Task<CongTrinhSettingsModal> ThongTinCongTrinhSan();
        Task<string> TaoCongNoPTERP(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh);
        Task<string> TaoCongNoPTraERP(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh);
        Task<string> TaoPhieuCongNoPhaiThuBDSAsync(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh);
        Task<string> TaoPhieuCongNoPhaiTraBDSAsync(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh);
        Task<string> DuyetTheoQuiTrinhSGGBatKy(string maCongViec, string maPhieu, string maNhanVien);
        Task<ResultModel> CanShowDangKyAsync(string maDuAn,
                          string maCanHo,
                          string? maGioHang,
                          CancellationToken ct = default);

    }
    public class BaseService : IBaseService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly IDbContextFactory<ErpBinhPhuCoDbContext> _factoryERP;
        private readonly string _connectionString;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IJSRuntime _js;
        private readonly NavigationManager _nav;
        private readonly INotificationSender _notifier;
        private readonly CongTrinhSettingsModal _settings;
        public BaseService(IDbContextFactory<AppDbContext> factory, IDbContextFactory<ErpBinhPhuCoDbContext> factoryERP, IConfiguration config, ICurrentUserService currentUser, IJSRuntime js, NavigationManager nav, INotificationSender notifier, IOptions<CongTrinhSettingsModal> options)
        {
            _factory = factory;
            _factoryERP = factoryERP;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _js = js;
            _nav = nav;
            _notifier = notifier;
            _settings = options.Value;
        }
        public string GenerateNextCode(string prefix, string? lastCode)
        {
            int nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode) && lastCode.StartsWith(prefix + "-"))
            {
                var numberPart = lastCode.Substring(prefix.Length + 1); // Bỏ prefix và dấu '-'
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    nextNumber = currentNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber:D5}";
        }

        public string GenerateNextCodeERP(string preString, string maxValue, int length)
        {
            string yearCurrent = DateTime.Now.Year.ToString().Substring(2, 2);
            string monthCurrent = DateTime.Now.Month.ToString(); // "4"
            //khi thang hien tai nho hon 9 thi cong them "0" vao
            if (Convert.ToInt32(monthCurrent) <= 9)
            {
                monthCurrent = "0" + monthCurrent;
            }
            //Khi tham so select o database la null khoi tao so dau tien
            if (String.IsNullOrEmpty(maxValue))
            {
                string ret = "1";
                while (ret.Length < length)
                {
                    ret = "0" + ret;
                }
                return preString + yearCurrent + monthCurrent + "-" + ret;
            }
            else
            {
                string preStringMax = maxValue.Substring(0, maxValue.IndexOf("-") - 4);
                string maxNumber = maxValue.Substring(maxValue.IndexOf("-") + 1);
                string monthYear = maxValue.Substring(maxValue.IndexOf("-") - 4, 4);
                string monthDb = monthYear.Substring(2, 2); //as "04"

                string stringTemp = maxNumber;
                //Khi thang trong gia tri max bang voi thang create thi cong len cho 1
                if (monthDb == monthCurrent)
                {
                    int strToInt = Convert.ToInt32(maxNumber);
                    maxNumber = Convert.ToString(strToInt + 1);
                    while (maxNumber.Length < stringTemp.Length)
                        maxNumber = "0" + maxNumber;
                }
                else //reset
                {
                    maxNumber = "1";
                    while (maxNumber.Length < stringTemp.Length)
                    {
                        maxNumber = "0" + maxNumber;
                    }
                }

                return preString + yearCurrent + monthCurrent + "-" + maxNumber;
            }
        }

        public async Task AddEmailToSend(string email, string subject, string bodyHtml, string nguoiLap)
        {
            using var context = _factory.CreateDbContext();
            var item = new HtSendEmail
            {
                Email = email,
                TieuDe = subject,
                NoiDung = bodyHtml,
                TrangThai = false,
                NguoiLap = nguoiLap,
                NgayLap = DateTime.Now
            };

            context.HtSendEmails.Add(item);
            await context.SaveChangesAsync();
        }

        public async Task<(List<DanhMucDuAnPagingDto> Data, int TotalCount)> DuAnPagingAsync(int page, int pageSize, string? qSearch)
        {
            using var context = _factory.CreateDbContext();
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<DanhMucDuAnPagingDto>(
                "Proc_DSDuAn_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        //Hàm lấy quyền của một vụ việc nào đó của User
        public async Task<bool> KiemTraQuyenVuViec(string maCongViec, string maVuViec)
        {
            try
            {
                string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                using var context = _factory.CreateDbContext();
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaNhanVien", maNhanVien);
                param.Add("@MaCongViec", maCongViec);
                param.Add("@MaVuViec", maVuViec);

                var result = (await connection.QueryAsync<object>(
                    "Proc_KiemTraQuyenVuViec",
                    param,
                    commandType: CommandType.StoredProcedure
                ));
                // Nếu có kết quả → có quyền → trả true
                return result.Count() == 0 ? false : true;
                // return true;
            }
            catch
            {
                return false;
            }
        }
        //Hàm lấy chi tiết quyền
        public async Task<List<CongViecVaVuViecCuaUserName>> GetQuyenVuViecChiTietAsync(string maCongViec, string maVuViec = null)
        {
            string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@MaNhanVien", maNhanVien);
            param.Add("@MaCongViec", maCongViec);
            param.Add("@MaVuViec", maVuViec);

            var result = await connection.QueryAsync<CongViecVaVuViecCuaUserName>(
                "Proc_KiemTraQuyenNhanVienTheoCongViec",
                param,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }

        public async Task CheckPermissionAsync(string menu, string vuViec)
        {
            // 1️⃣ Chưa load user → thử load (an toàn)
            if (!_currentUser.IsLoaded)
            {
                try
                {
                    await _currentUser.EnsureUserLoadedFromJSAsync(_js);
                }
                catch (Microsoft.JSInterop.JSDisconnectedException)
                {
                    return; // circuit chết thì thôi
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
            }

            // 2️⃣ Vẫn chưa có user → điều hướng login
            if (!_currentUser.IsLoaded)
            {
                _nav.NavigateTo("/login", true);
                return;
            }

            // 3️⃣ Có user nhưng không có quyền
            var coQuyen = await KiemTraQuyenVuViec(menu, vuViec);
            if (!coQuyen)
            {
                _nav.NavigateTo("/not-authorized");
                return;
            }
            // 4️⃣ OK → cho chạy tiếp
        }


        //public async Task CheckPermissionAsync(string menu, string vuViec)
        //{
        //    if (!_currentUser.IsLoaded)
        //    {
        //        await _currentUser.EnsureUserLoadedFromJSAsync(_js);
        //        if (!_currentUser.IsLoaded)
        //        {
        //            throw new Exception("Không thể xác định người dùng hiện tại.");
        //        }
        //    }

        //    var coQuyen = await KiemTraQuyenVuViec(menu, vuViec);
        //    if (!coQuyen)
        //    {
        //        _nav.NavigateTo("/not-authorized");
        //    }
        //}
        //public async Task<bool> CheckPermissionTraVeAsync(string menu, string vuViec)
        //{
        //    if (!_currentUser.IsLoaded)
        //    {
        //        await _currentUser.EnsureUserLoadedFromJSAsync(_js);
        //        if (!_currentUser.IsLoaded)
        //        {
        //            return false;
        //            //throw new Exception("Không thể xác định người dùng hiện tại.");
        //        }
        //    }

        //    var coQuyen = await KiemTraQuyenVuViec(menu, vuViec);
        //    if (!coQuyen)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        public async Task<bool> CheckPermissionTraVeAsync(string menu, string vuViec)
        {
            // Chưa có user -> coi như không có quyền (chưa login)
            if (!_currentUser.IsLoaded)
            {
                try
                {
                    await _currentUser.EnsureUserLoadedFromJSAsync(_js);
                }
                catch (Microsoft.JSInterop.JSDisconnectedException)
                {
                    return false; // circuit rớt thì thôi
                }
                catch (ObjectDisposedException)
                {
                    return false;
                }
            }

            if (!_currentUser.IsLoaded)
                return false; // KHÔNG throw

            return await KiemTraQuyenVuViec(menu, vuViec);
        }


        public async Task<List<HtTrangThaiDuyet>> GetListTrangThaiDuyetAsync()
        {
            List<HtTrangThaiDuyet> listTTD = new List<HtTrangThaiDuyet>();
            try
            {
                using var context = _factory.CreateDbContext();
                listTTD = await context.HtTrangThaiDuyets.ToListAsync();

            }
            catch
            {
                listTTD = new List<HtTrangThaiDuyet>();
            }
            return listTTD;
        }
        //Hàm lấy mã nhân viên hiện tại
        public async Task<string> MaNhanVienHienTai()
        {
            try
            {
                string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                return maNhanVien;
            }
            catch
            {
                return string.Empty;
            }
        }

        //Hàm lấy mã nhân viên hiện tại
        public async Task<string> NhanVienThuocSGG()
        {
            try
            {
                using var context = _factory.CreateDbContext();
                string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                string loaiSGG = await context.TblUsers.Where(d => d.MaNhanVien == maNhanVien).Select(d => d.LoaiUser).FirstOrDefaultAsync() ?? "NV";
                return (loaiSGG ?? "NV");
            }
            catch
            {
                return string.Empty;
            }
        }

        #region Duyệt theo qui trình động
        //Hàm lấy danh sách qui trình duyệt
        public async Task<List<QuiTrinhDuyetDto>> GetQuyTrinhDuyetAsyn(string maCongViec, string maPhieu, string maCongTrinh, string maNhanVien, int? idQuyTrinh)
        {
            var result = new List<QuiTrinhDuyetDto>();
            try
            {
                using var context = _factory.CreateDbContext();
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaCongViec", maCongViec);
                param.Add("@MaPhieu", maPhieu);
                param.Add("@MaCongTrinh", maCongTrinh);
                param.Add("@MaNhanVien", maNhanVien);
                param.Add("@IdQuyTrinh", idQuyTrinh);

                result = (await connection.QueryAsync<QuiTrinhDuyetDto>(
                   "Proc_GetQuyTrinhDuyetChiTiet",
                   param,
                   commandType: CommandType.StoredProcedure
               )).ToList();
                return result;
            }
            catch
            {
                result = new List<QuiTrinhDuyetDto>();
            }
            return result;
        }
        //Duyệt qui trình động dùng chung hệ thống
        public async Task<bool> DuyetPhieuTheoQuiTrinhDong(string maCongViec, string maPhieu, int? idQuyTrinh, string maNhanVien, string noiDung, int flag)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaCongViec", maCongViec);
                param.Add("@MaPhieu", maPhieu);
                param.Add("@MaNhanVien", maNhanVien);
                param.Add("@IdQuyTrinh", idQuyTrinh);
                param.Add("@NoiDung", noiDung);
                var result = (await connection.QueryAsync<ThongTinPhieuDuyetModel>(
                       "Proc_DuyetPhieuKeTiep",
                       param,
                       commandType: CommandType.StoredProcedure
                   )).FirstOrDefault();
                if (result != null)
                {
                    //Cập nhật lại trạng thái duyệt
                    CapNhatLaiTrangThaiDuyet(maCongViec, maPhieu, result.TrangThai, idQuyTrinh, flag);
                    await _notifier.SendAsync(result.MaNhanVien, "Bạn có phiếu cần duyệt mới");
                }
                return true;
            }
            catch (Exception ex)
            {
                //using var context = _factory.CreateDbContext();
                //var log = new HtGhiNhanLog
                //{
                //    NoiDung = ex.Message.ToString(),
                //    Controller = "BaseService_CapNhatLaiTrangThaiDuyet: tạo phiếu từ tổng hợp booking qua bên ERP"
                //};
                //await context.HtGhiNhanLogs.AddAsync(log);
                //await context.SaveChangesAsync();

                return false;
            }
        }
        //Trả lại phiếu duyệt trước đó
        public async Task<bool> TraLaiPhieu(string maCongViec, string maPhieu, string noiDung)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                // B1: Lấy 2 bước duyệt gần nhất
                var buocDuyet = await context.HtDmnguoiDuyets
                    .Where(x => x.MaPhieu == maPhieu && x.MaCongViec == maCongViec)
                    .OrderByDescending(x => x.Id)
                    .Take(2)
                    .ToListAsync();

                if (buocDuyet.Count < 2) return false;

                var buocCuoi = buocDuyet[0];
                var buocTruocDo = buocDuyet[1];

                //Insert vô HtDmnguoiDuyets 1 dòng trước đó
                HtDmnguoiDuyet nd = new HtDmnguoiDuyet();
                nd.MaPhieu = maPhieu;
                nd.MaCongViec = maCongViec;
                nd.MaNhanVien = buocTruocDo.MaNhanVien;
                nd.TrangThai = buocTruocDo.TrangThai;
                nd.NgayDuyet = DateTime.Now;
                nd.TrangThaiTraLai = 1;
                nd.NoiDung = noiDung;
                await context.HtDmnguoiDuyets.AddAsync(nd);
                await context.SaveChangesAsync();
                //Cập nhật lại trạng thái duyệt
                CapNhatLaiTrangThaiDuyet(maCongViec, maPhieu, buocTruocDo.TrangThai, 0, 1);
                await _notifier.SendAsync(buocTruocDo.MaNhanVien, "Bạn có phiếu duyệt bị trả lại");
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async void CapNhatLaiTrangThaiDuyet(string maCongViec, string maPhieu, int? trangThai, int? maQuTrinhDuyet, int flag)
        {
            using var context = _factory.CreateDbContext();
            using var contextERP = _factoryERP.CreateDbContext();

            if (maCongViec == "KHBH")//Kế hoạch báng hàng
            {
                var phieu = await context.BhKeHoachBanHangs.Where(d => d.MaPhieuKh == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
            }
            else if (maCongViec == "PhieuDuyetGia")
            {
                var phieu = await context.BhPhieuDuyetGia.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
            }
            else if (maCongViec == "GioHang")
            {
                var phieu = await context.BhGioHangs.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
            }
            else if (maCongViec == "PhieuDatCoc")
            {
                var phieu = await context.BhPhieuDatCocs.Where(d => d.MaPhieuDc == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
            }
            else if (maCongViec == "CSBH")
            {
                var phieu = await context.DaChinhSachBanHangs.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
            }
            else if (maCongViec == "ThanhLyDatCoc")
            {
                var phieu = await context.BhPhieuThanhLyDatCocs.Where(d => d.MaPhieuThanhLy == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
                //Kiểm tra nếu bước duyệt cuối thì cập nhật lại hiện trạng kinh doanh sản phẩm là mở bán
                string maCongTrinh = string.Empty;
                int buocDuyetCuoi = await BuocDuyetCuoi(phieu.MaQuiTrinhDuyet ?? 0);
                if (buocDuyetCuoi == trangThai)
                {
                    var hdmb = await context.BhPhieuDatCocs.Where(d => d.MaPhieuDc == phieu.MaPhieuDatCoc).FirstOrDefaultAsync();
                    if (hdmb != null)
                    {
                        maCongTrinh = hdmb.MaDuAn;
                        var sp = await context.DaDanhMucSanPhams.Where(d => d.MaSanPham == hdmb.MaCanHo && d.MaDuAn == hdmb.MaDuAn).FirstOrDefaultAsync();
                        if (sp != null)
                        {
                            sp.HienTrangKd = "MB";
                            context.SaveChanges();
                        }
                    }
                    if (phieu.PhiPhat > 0)//Tạo phiếu công nợ phải thu bên BDS
                    {
                        await TaoPhieuCongNoPhaiThuBDSAsync(phieu.MaPhieuThanhLy, null, maCongViec, string.Empty);
                        await TaoCongNoPTERP(phieu.MaPhieuThanhLy, null, maCongViec, maCongTrinh);
                    }
                    if (phieu.TienHoanLai > 0)//Tạo phiếu công nợp phải trả bên BDS
                    {
                        await TaoPhieuCongNoPhaiTraBDSAsync(phieu.MaPhieuThanhLy, null, maCongViec, string.Empty);
                        await TaoCongNoPTraERP(phieu.MaPhieuThanhLy, null, maCongViec, maCongTrinh);
                    }
                }

            }
            else if (maCongViec == "HopDongMuaBan")
            {
                var phieu = await context.KdHopDongs.Where(d => d.MaHopDong == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
            }
            else if (maCongViec == "ThanhLyHopDong")
            {
                var phieu = await context.KdThanhLyHopDongs.Where(d => d.MaPhieuTl == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
                //Kiểm tra nếu bước duyệt cuối thì cập nhật lại hiện trạng kinh doanh sản phẩm là còn trống
                int buocDuyetCuoi = await BuocDuyetCuoi(phieu.MaQuiTrinhDuyet ?? 0);
                if (buocDuyetCuoi == trangThai)
                {
                    var hdmb = await context.KdHopDongs.Where(d => d.MaHopDong == phieu.MaHopDong).FirstOrDefaultAsync();
                    if (hdmb != null)
                    {
                        var sp = await context.DaDanhMucSanPhams.Where(d => d.MaSanPham == hdmb.MaCanHo && d.MaDuAn == hdmb.MaDuAn).FirstOrDefaultAsync();
                        if (sp != null)
                        {
                            sp.HienTrangKd = "MB";
                            context.SaveChanges();
                        }
                    }
                }
            }
            else if (maCongViec == "HopDongChuyenNhuong")
            {
                var phieu = await context.KdChuyenNhuongs.Where(d => d.MaChuyenNhuong == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }
            }
            else if (maCongViec == "PhieuGiuCho" || maCongViec == "PhieuGiuChoSanGD")
            {
                var phieu = await context.BhPhieuGiuChos.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    // 3. Xác định có phải bước duyệt cuối không
                    var buocDuyetCuoi = await BuocDuyetCuoi(phieu.MaQuiTrinhDuyet ?? 0);
                    var isBuocDuyetCuoi = (buocDuyetCuoi == trangThai);

                    // 4. Nếu là bước cuối -> cập nhật IsXacNhan cho phiếu giữ chỗ
                    if (isBuocDuyetCuoi)
                    {
                        phieu.IsxacNhan = true;

                        //Insert vô bảng lịch sử khi phiếu giữ chỗ đã được lưu phiếu
                        var sttPGC = new BhPhieuGiuChoStt
                        {
                            MaPhieuGiuCho = maPhieu,
                            MaDotBanHang = phieu.DotMoBan ?? string.Empty,
                            SoTtbooking = phieu.SoTtbooking ?? 0,
                        };
                        await context.AddAsync(sttPGC);
                    }
                    await context.SaveChangesAsync();
                }

            }
            else if (maCongViec == "TongHopBooking")
            {
                // 1. Lấy phiếu tổng hợp
                var phieu = await context.KdPhieuTongHopBookings
                    .FirstOrDefaultAsync(d => d.MaPhieu == maPhieu);

                if (phieu == null)
                {
                    // tuỳ cách bạn xử lý, có thể throw hoặc return result
                    // ở đây mình tạm throw để không xử lý tiếp logic duyệt
                    throw new InvalidOperationException($"Không tìm thấy phiếu tổng hợp booking: {maPhieu}");
                }

                // 2. Cập nhật trạng thái duyệt + quy trình duyệt (nếu cần)
                phieu.TrangThaiDuyet = trangThai;

                if (flag == 0)
                {
                    phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                }

                // 3. Xác định có phải bước duyệt cuối không
                var buocDuyetCuoi = await BuocDuyetCuoi(phieu.MaQuiTrinhDuyet ?? 0);
                var isBuocDuyetCuoi = (buocDuyetCuoi == trangThai);

                // 4. Nếu là bước cuối -> cập nhật IsXacNhan cho tất cả phiếu giữ chỗ thuộc THBK này
                if (isBuocDuyetCuoi)
                {
                    // Lấy toàn bộ PGC thuộc phiếu tổng hợp này mà chưa xác nhận
                    var phieuGiuChoList = await context.BhPhieuGiuChos
                        .Where(x => x.MaPhieuTh == phieu.MaPhieu && x.IsxacNhan != true)
                        .ToListAsync();
                    //Lưu tất cả các phiếu giữ chỗ vô bảng BH_PhieuGiuCho_STT
                    List<BhPhieuGiuChoStt> listPGCSTT = new List<BhPhieuGiuChoStt>();
                    foreach (var pgc in phieuGiuChoList)
                    {
                        pgc.IsxacNhan = true;
                        pgc.MaQuiTrinhDuyet = phieu.MaQuiTrinhDuyet;
                        pgc.TrangThaiDuyet = trangThai;

                        //Insert vô bảng phiếu giữ chỗ stt
                        var sttPGC = new BhPhieuGiuChoStt
                        {
                            MaPhieuGiuCho = pgc.MaPhieu,
                            MaDotBanHang = pgc.DotMoBan ?? string.Empty,
                            SoTtbooking = pgc.SoTtbooking ?? 0,
                        };
                        listPGCSTT.Add(sttPGC);
                    }
                    await context.BhPhieuGiuChoStts.AddRangeAsync(listPGCSTT);
                }

                // 5. Lưu toàn bộ thay đổi trước khi sinh công nợ
                await context.SaveChangesAsync();

                // 6. Nếu là bước cuối -> sinh công nợ cho ERP
                if (isBuocDuyetCuoi)
                {
                    await TaoCongNoPTERP(phieu.MaPhieu, phieu.NgayThu, maCongViec, phieu.MaDuAn);
                }
            }
            else if (maCongViec == "PDNHT")
            {
                var phieu = await context.KdPhieuDeNghiHoanTienBookings.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                if (phieu != null)
                {
                    phieu.TrangThaiDuyet = trangThai;
                    if (flag == 0)
                    {
                        phieu.MaQuiTrinhDuyet = maQuTrinhDuyet;
                    }
                    context.SaveChanges();
                }

                //Kiểm tra nếu bước duyệt cuối thì sinh công nợ cho 2 hệ thống
                int buocDuyetCuoi = await BuocDuyetCuoi(phieu.MaQuiTrinhDuyet ?? 0);
                if (buocDuyetCuoi == trangThai)
                {
                    string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                    string maxPhieuCNPT = await context.KtPhieuCongNoPhaiTras.OrderByDescending(d => d.NgayLap).Select(d => d.MaPhieu).FirstOrDefaultAsync() ?? string.Empty;
                    //Sinh công nợ bên BDS
                    KtPhieuCongNoPhaiTra pt = new KtPhieuCongNoPhaiTra();
                    pt.MaPhieu = GenerateNextCode("CNPT", maxPhieuCNPT);
                    pt.NgayLap = DateTime.Now;
                    pt.NguoiLap = maNhanVien;
                    pt.HanThanhToan = pt.NgayLap;
                    pt.DuAn = phieu.MaDuAn;
                    pt.MaChungTu = phieu.MaPhieu;
                    pt.IdChungTu = phieu.MaPhieu;
                    pt.NoiDung = phieu.NoiDung;
                    pt.MaKhachHang = phieu.MaSanGiaoDich;
                    pt.TenKhachHang = await context.DmSanGiaoDiches.Where(d => d.MaSanGiaoDich == phieu.MaSanGiaoDich).Select(d => d.TenSanGiaoDich).FirstOrDefaultAsync() ?? string.Empty;
                    pt.SoTien = await context.KdPhieuDeNghiHoanTienBookingSoPhieuBookings.Where(d => d.MaPhieuHoanTien == phieu.MaPhieu).SumAsync(d => d.SoTien) ?? 0;
                    pt.MaCongViec = "PDNHT";
                    pt.MaDoiTuong = pt.MaKhachHang;
                    await context.KtPhieuCongNoPhaiTras.AddAsync(pt);
                    context.SaveChanges();
                    //Sinh công nợ bên ERP
                    await TaoCongNoPTraERP(phieu.MaPhieu, null, "PDNHT", phieu.MaDuAn ?? string.Empty);
                    //KT_PhieuDuTruChi dtc = new KT_PhieuDuTruChi();
                    //string maxPhieuDTC = await contextERP.KT_PhieuDuTruChi.OrderByDescending(d => d.ngayLap).Select(d => d.maPhieuDuTruChi).FirstOrDefaultAsync() ?? string.Empty;
                    //dtc.maPhieuDuTruChi = GenerateNextCodeERP("DTC", maxPhieuDTC, 3);
                    //dtc.soPhieuDuTruChi = pt.MaChungTu;
                    //dtc.ngayLap = pt.NgayLap;
                    //dtc.ngayHachToan = pt.NgayLap;
                    //dtc.nguoiLap = maNhanVien;
                    //dtc.maDoiTuong = pt.MaKhachHang;
                    //dtc.tenDoiTuong = pt.TenKhachHang;
                    //dtc.noiDung = pt.NoiDung;
                    //dtc.soPhieuThanhToan = pt.MaChungTu;
                    //dtc.soTien = pt.SoTien;
                    //dtc.maCongTrinh = pt.DuAn;
                    //dtc.maCongViecChungTu = pt.MaCongViec;
                    //await contextERP.KT_PhieuDuTruChi.AddAsync(dtc);
                    //await context.SaveChangesAsync();
                    //await contextERP.SaveChangesAsync();
                }
            }
        }
        //Lấy thông tin người duyệt hiện tại
        public async Task<HtDmnguoiDuyet> ThongTinNguoiDuyet(string maCongViec, string maPhieu)
        {
            using var context = _factory.CreateDbContext();
            HtDmnguoiDuyet nd = new HtDmnguoiDuyet();
            nd = await context.HtDmnguoiDuyets.Where(d => d.MaCongViec == maCongViec && d.MaPhieu == maPhieu).OrderByDescending(d => d.Id).FirstOrDefaultAsync();
            if (nd == null)
            {
                nd = new HtDmnguoiDuyet();
            }
            return nd;
        }
        public async Task<int> BuocDuyetCuoi(int idQuiTrinh)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                string maBDC = await context.HtQuyTrinhDuyetBuocDuyets.Where(d => d.IdQuyTrinh == idQuiTrinh).OrderByDescending(d => d.ThuTu).Select(d => d.MaBuocDuyet).FirstOrDefaultAsync();
                int bdc = await context.HtBuocDuyets.Where(d => d.MaBuocDuyet == maBDC).Select(d => d.IdbuocDuyet).FirstOrDefaultAsync();
                return bdc;
            }
            catch
            {
                return 0;
            }

        }
        public async Task<List<LichSuDuyetPhieuDto>> GetLichSuDuyetPhieuAsyn(string maCongViec, string maPhieu)
        {
            var result = new List<LichSuDuyetPhieuDto>();
            try
            {
                using var context = _factory.CreateDbContext();
                using var connection = new SqlConnection(_connectionString);
                var param = new DynamicParameters();
                param.Add("@MaPhieu", maPhieu);
                param.Add("@MaCongViec", maCongViec);

                result = (await connection.QueryAsync<LichSuDuyetPhieuDto>(
                   "Proc_GetLichSuDuyetPhieu",
                   param,
                   commandType: CommandType.StoredProcedure
               )).ToList();
                return result;
            }
            catch
            {
                result = new List<LichSuDuyetPhieuDto>();
            }
            return result;
        }
        #endregion

        #region Sàn giao dịch
        public async Task<List<DmSanGiaoDich>> GetListSGGAsyn(string maDuAn)
        {
            var result = new List<DmSanGiaoDich>();
            try
            {
                using var context = _factory.CreateDbContext();
                result = await context.DmSanGiaoDiches.ToListAsync();
                return result;
            }
            catch
            {
                result = new List<DmSanGiaoDich>();
            }
            return result;
        }
        #endregion

        #region Get danh sách phiếu duyệt
        public async Task<List<CongViecDuyetModel>> GetDanhSachPhieuDuyet()
        {
            string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();
            param.Add("@maNV", maNhanVien);

            var result = await connection.QueryAsync<CongViecDuyetModel>(
                "Proc_NhacViec",
                param,
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }
        #endregion

        #region Hàm lấy thông tin chi tiết sản phẩm
        public async Task<SoDoCanHoModel> ThongTinChiTietSanPham(string maDuAn, string maSanPham)
        {
            SoDoCanHoModel soDoCH = new SoDoCanHoModel();
            try
            {
                string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                using var connection = new SqlConnection(_connectionString);
                var parameters = new DynamicParameters();
                parameters.Add("@MaDuAn", maDuAn);
                parameters.Add("@MaSanPham", maSanPham);

                var result = (await connection.QueryAsync<SoDoCanHoModel>(
                    "Proc_ViewChiTietSanPham",
                    parameters,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                if (result != null && result.Count > 0)
                {
                    soDoCH = result.Where(d => d.MaSanPham == maSanPham).FirstOrDefault();
                    soDoCH.FlagPDK = false;
                }
                return soDoCH;
            }
            catch
            {
                soDoCH = new SoDoCanHoModel();
                return soDoCH;
            }
        }
        #endregion

        #region Lấy hiện trạng kinh doanh sản phẩm
        public async Task<string> HienTrangKinhDoanhSanPham(string maDuAn, string maSanPham)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var sp = await context.DaDanhMucSanPhams.Where(d => d.MaSanPham == maSanPham && d.MaDuAn == maDuAn).FirstOrDefaultAsync();
                if (sp != null)
                {
                    sp.HienTrangKd = string.IsNullOrEmpty(sp.HienTrangKd) ? "ConTrong" : sp.HienTrangKd;
                    return sp.HienTrangKd;
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region lấy thông tin công ty
        public async Task<ThongTinCongTyModel> ThongTinCongTyAsync()
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var record = new ThongTinCongTyModel();

                var entity = await context.HtThongTinCongTies.FirstOrDefaultAsync();
                if (entity == null)
                    return record;

                record.MaCongTy = entity.MaCongTy;
                record.TenCongTy = entity.TenCongTy;
                record.DiaChiCongTy = entity.DiaChiCongTy;
                record.Fax = entity.Fax;
                record.DienThoai = entity.DienThoai;
                record.MaSoThue = entity.MaSoThue;
                record.TaiKhoan = entity.TaiKhoan;
                record.ChiNhanhNganHang = entity.ChiNhanhNganHang;
                record.DaiDienCongTy = entity.DaiDienCongTy;
                record.ChucVuNguoiDaiDien = entity.ChucVuNguoiDaiDien;
                record.CmndSoNguoiDaiDien = entity.CmndSoNguoiDaiDien;
                record.CmndNgayCapNguoiDd = entity.CmndNgayCapNguoiDd;
                record.CmndNoiCapNguoiDd = entity.CmndNoiCapNguoiDd;
                record.Email = entity.Email;
                record.TenTaiKhoan = entity.TenTaiKhoan;
                record.TenNganHang = entity.TenNganHang;
                record.TenChiNhanh = entity.TenChiNhanh;

                var files = await context.HtFileDinhKems.Where(d => d.Controller == "ThongTinCongTy" && d.MaPhieu == entity.MaCongTy
                    ).Select(d => new UploadedFileModel
                    {
                        Id = d.Id,
                        FileName = d.TenFileDinhKem,
                        FileNameSave = d.TenFileDinhKemLuu,
                        FileSize = d.FileSize,
                        ContentType = d.FileType,
                        FullDomain = d.FullDomain,
                    }).ToListAsync();

                record.Logo = files;

                return record;
            }
            catch
            {
                return new ThongTinCongTyModel();
            }
        }

        #endregion

        #region Hàm lấy thông tin sàn giao dịch theo công trình
        public async Task<CongTrinhSettingsModal> ThongTinCongTrinhSan()
        {
            CongTrinhSettingsModal ctSan = new CongTrinhSettingsModal();
            try
            {
                ctSan.MaCongTrinh = _settings.MaCongTrinh;
                ctSan.TenCongTrinh = _settings.TenCongTrinh;
            }
            catch
            {
                ctSan = new CongTrinhSettingsModal();
            }
            return ctSan;
        }
        #endregion

        #region Tạo công nợ phải thu qua bên ERP
        public async Task<string> TaoCongNoPTERP(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var contextERP = _factoryERP.CreateDbContext();
                var param = new DynamicParameters();

                param.Add("@MaPhieu", maPhieu);
                param.Add("@NgayKy", ngayKy);
                param.Add("@MaCongViec", maCongViec);

                var result = (await connection.QueryAsync<CongNoPhaiThuModel>(
                    "Proc_TaoPhieuCongNoPhaiThu",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                //string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                //foreach (var item in result)
                //{
                //    TC_PhieuPhaiThu phieuPT = new TC_PhieuPhaiThu();
                //    string maxPhieu = await contextERP.TC_PhieuPhaiThu.OrderByDescending(d => d.ngayLap).Select(d => d.maPhieu).FirstOrDefaultAsync() ?? string.Empty;
                //    phieuPT.maPhieu = GenerateNextCodeERP("CNPT", maxPhieu, 3);
                //    phieuPT.ngayLap = DateTime.Now;
                //    phieuPT.nguoiLap = maNhanVien;
                //    phieuPT.ngayHachToan = phieuPT.ngayLap;
                //    phieuPT.maDoiTuong = item.MaKhachHang;
                //    phieuPT.tenDoiTuong = item.TenKhachHang;
                //    phieuPT.noiDung = item.NoiDung;
                //    phieuPT.soPhieuThanhToan = item.MaPhieu;
                //    phieuPT.soTien = item.SoTien;
                //    phieuPT.maCongTrinh = maCongTrinh;
                //    phieuPT.maCongViecChungTu = maCongViec;
                //    await contextERP.TC_PhieuPhaiThu.AddAsync(phieuPT);
                //    await contextERP.SaveChangesAsync();
                //}
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region Tạo công nợ phải trả qua bên ERP
        public async Task<string> TaoCongNoPTraERP(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var contextERP = _factoryERP.CreateDbContext();
                var param = new DynamicParameters();

                param.Add("@MaPhieu", maPhieu);
                param.Add("@NgayKy", ngayKy);
                param.Add("@MaCongViec", maCongViec);

                var result = (await connection.QueryAsync<CongNoPhaiThuModel>(
                    "Proc_TaoPhieuCongNoPhaiThu",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                //string maNhanVien = _currentUser.MaNhanVien ?? string.Empty;
                //foreach (var item in result)
                //{

                //    KT_PhieuDuTruChi dtc = new KT_PhieuDuTruChi();
                //    string maxPhieuDTC = await contextERP.KT_PhieuDuTruChi.OrderByDescending(d => d.ngayLap).Select(d => d.maPhieuDuTruChi).FirstOrDefaultAsync() ?? string.Empty;
                //    dtc.maPhieuDuTruChi = GenerateNextCodeERP("DTC", maxPhieuDTC, 3);
                //    dtc.ngayLap = DateTime.Now;
                //    dtc.nguoiLap = maNhanVien;
                //    dtc.ngayHachToan = dtc.ngayLap;
                //    dtc.maDoiTuong = item.MaKhachHang;
                //    dtc.tenDoiTuong = item.TenKhachHang;
                //    dtc.noiDung = item.NoiDung;
                //    dtc.soPhieuThanhToan = item.MaPhieu;
                //    dtc.soTien = item.SoTien;
                //    dtc.maCongTrinh = maCongTrinh;
                //    dtc.maCongViecChungTu = maCongViec;
                //    await contextERP.KT_PhieuDuTruChi.AddAsync(dtc);
                //    await contextERP.SaveChangesAsync();
                //}
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        #endregion

        #region Tạo công nợ phải thu bên BDS
        public async Task<string> TaoPhieuCongNoPhaiThuBDSAsync(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var context = _factory.CreateDbContext();
                var param = new DynamicParameters();

                param.Add("@MaPhieu", maPhieu);
                param.Add("@NgayKy", ngayKy);
                param.Add("@MaCongViec", maCongViec);

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

        #region Tạo phiếu công nợ phải trả bên BDS
        public async Task<string> TaoPhieuCongNoPhaiTraBDSAsync(string maPhieu, DateTime? ngayKy, string maCongViec, string maCongTrinh)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                using var context = _factory.CreateDbContext();
                var param = new DynamicParameters();

                param.Add("@MaPhieu", maPhieu);
                param.Add("@NgayKy", ngayKy);
                param.Add("@MaCongViec", maCongViec);

                var result = (await connection.QueryAsync<CongNoPhaiThuModel>(
                    "Proc_TaoPhieuCongNoBDS",
                    param,
                    commandType: CommandType.StoredProcedure
                )).ToList();
                //KtPhieuCongNoPhaiTra r;
                //foreach (var item in result)
                //{
                //    string maxPhieuCNPT = await context.KtPhieuCongNoPhaiTras.OrderByDescending(d => d.NgayLap).Select(d => d.MaPhieu).FirstOrDefaultAsync() ?? string.Empty;
                //    r = new KtPhieuCongNoPhaiTra();//Trường hợp ở đây MaPhieu đã tự tăng trong Seq_PCNPT rùi nha
                //    r.MaPhieu = GenerateNextCode("CNPT", maxPhieuCNPT);
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
                //    await context.KtPhieuCongNoPhaiTras.AddAsync(r);
                //    context.SaveChanges();
                //}
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
        }
        #endregion

        #region Duyệt theo qui trình duyệt bất kỳ theo sàn giao dịch
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
                        if (maCongViec == "TongHopBooking")
                        {
                            var capNhatPhieu = await _context.KdPhieuTongHopBookings.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                            if (capNhatPhieu == null)
                            {
                                return "ChuaTonTai";
                            }
                            capNhatPhieu.TrangThaiDuyet = result.TrangThai;
                            capNhatPhieu.MaQuiTrinhDuyet = idQuiTrinh;
                        }
                        else if (maCongViec == "PhieuGiuChoSanGD" || maCongViec == "PhieuGiuCho")
                        {
                            var capNhatPhieu = await _context.BhPhieuGiuChos.Where(d => d.MaPhieu == maPhieu).FirstOrDefaultAsync();
                            if (capNhatPhieu == null)
                            {
                                return "ChuaTonTai";
                            }
                            capNhatPhieu.TrangThaiDuyet = result.TrangThai;
                            capNhatPhieu.MaQuiTrinhDuyet = idQuiTrinh;
                        }
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

        #region Kiểm tra tình trạng căn hộ có được đăng ký hay không
        public async Task<ResultModel> CanShowDangKyAsync(
string maDuAn,
string maCanHo,
string? maGioHang,
CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(maDuAn) || string.IsNullOrWhiteSpace(maCanHo))
                return ResultModel.Fail("Thiếu thông tin dự án hoặc căn hộ.");

            var duAnKey = maDuAn.Trim();
            var canHoKey = maCanHo.Trim();
            var gioHangKey = (maGioHang ?? "").Trim();

            try
            {
                await using var db = await _factory.CreateDbContextAsync(ct);

                // 0) Rule: sản phẩm phải tồn tại + chưa bán
                var hienTrang = await db.DaDanhMucSanPhams.AsNoTracking()
                    .Where(x => x.MaDuAn == duAnKey && x.MaSanPham == canHoKey)
                    .Select(x => x.HienTrangKd)
                    .FirstOrDefaultAsync(ct);

                if (hienTrang is null)
                    return ResultModel.Fail("Không tìm thấy thông tin căn hộ.");

                if (string.Equals((hienTrang ?? "ConTrong").Trim(), "DaBan", StringComparison.OrdinalIgnoreCase))
                    return ResultModel.Fail("Căn hộ đã bán. Không thể đăng ký.");

                // 1) Rule: căn hộ không được có PDK còn hiệu lực + chưa xác nhận
                var hasActivePdk = await db.BhPhieuDangKiChonCans.AsNoTracking()
                    .AnyAsync(x =>
                        x.MaDuAn == duAnKey &&
                        x.MaCanHo == canHoKey &&
                        (string.IsNullOrEmpty(gioHangKey) || x.MaGioHang == gioHangKey) &&
                        (x.IsHetHieuLuc != true) &&
                        (x.IsXacNhan == null || x.IsXacNhan == false),
                        ct);

                if (hasActivePdk)
                    return ResultModel.Fail("Căn hộ đã có phiếu đăng ký còn hiệu lực và chưa xác nhận.");

                // 2) Rule: nếu user là SGG thì check giới hạn số căn đang booking theo dự án
                var nguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                if (string.Equals(nguoiLap?.LoaiUser, "SGG", StringComparison.OrdinalIgnoreCase))
                {
                    var sanKey = (nguoiLap?.MaSanGiaoDich ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(sanKey))
                        return ResultModel.Fail("Không xác định được sàn giao dịch của tài khoản.");

                    // Limit lấy từ DM_SanGiaoDich_DuAn (theo MaSan + MaDuAn)
                    var limit = await db.DmSanGiaoDichDuAns.AsNoTracking()
                        .Where(x => (x.MaDuAn ?? "").Trim() == duAnKey
                                 && (x.MaSan ?? "").Trim() == sanKey)
                        .Select(x => (int?)x.SoLuongBookCanHo)
                        .FirstOrDefaultAsync(ct) ?? 0;

                    if (limit <= 0)
                        return ResultModel.Fail("Sàn giao dịch chưa được cấu hình giới hạn booking cho dự án này.");

                    // Đếm số booking còn hiệu lực + chưa xác nhận của sàn này trong dự án
                    var currentBookingCount = await db.BhPhieuDangKiChonCans.AsNoTracking()
                        .Where(x =>
                            x.MaDuAn == duAnKey &&
                            (x.IsHetHieuLuc != true) &&
                            (x.IsXacNhan == null || x.IsXacNhan == false) &&
                            x.NguoiLap == sanKey) // mapping: NguoiLap lưu MaSan
                        .CountAsync(ct);

                    // đạt trần thì chặn (vì bấm đăng ký sẽ +1 vượt)
                    if (currentBookingCount >= limit)
                        return ResultModel.Fail($"Sàn đã đạt giới hạn booking ({currentBookingCount}/{limit}).");
                }

                return ResultModel.Success("Được phép đăng ký.");
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                return ResultModel.Fail("Lỗi hệ thống. Không thể kiểm tra điều kiện đăng ký.");
            }
        }
        #endregion
    }

}

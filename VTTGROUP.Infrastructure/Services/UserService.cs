using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Linq;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.PhanQuyen;
using VTTGROUP.Domain.Model.User;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class UserService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly string _connectionString;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;
        private readonly IBaseService _baseService;
        public UserService(IDbContextFactory<AppDbContext> factory, ILogger<UserService> logger, IConfiguration config, ICurrentUserService currentUser, IBaseService baseService)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
            _baseService = baseService;
        }

        #region Index
        public async Task<(List<UserPagingDto> Data, int TotalCount)> GetPagingAsync(
         string? maPhongBan, string? trangThai, string? loaiUser, int page, int pageSize, string? qSearch)
        {
            using var context = _factory.CreateDbContext();
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            param.Add("@MaPhongBan", !string.IsNullOrEmpty(maPhongBan) ? maPhongBan : null);
            param.Add("@TrangThai", !string.IsNullOrEmpty(trangThai) ? trangThai : null);
            param.Add("@LoaiUser", loaiUser);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<UserPagingDto>(
                "Proc_User_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            var nhomUsers = await context.TblUserthuocnhoms
                .Select(d => new NhomUserOfUserModel
                {
                    MaNhanVien = d.MaNv,
                    MaNhom = d.MaNhomUser,
                    UserId = d.UserId,
                    IsChecked = true
                })
                .ToListAsync();

            foreach (var cv in result)
            {
                cv.nhomUsers = nhomUsers
                    .Where(x => x.UserId == cv.ID)
                    .ToList();
            }

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        public async Task<(List<UserPagingDto> Data, int TotalCount)> GetUserSanGDPagingAsync(
            string? trangThai, int page, int pageSize, string? qSearch)
        {
            using var context = _factory.CreateDbContext();
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            using var connection = new SqlConnection(_connectionString);
            var param = new DynamicParameters();

            var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
            if (NguoiLap == null)
            {
                return (new List<UserPagingDto>(), 0);
            }
            var maSanGD = NguoiLap.MaNhanVien;

            param.Add("@MaSanGD", !string.IsNullOrEmpty(maSanGD) ? maSanGD : null);
            param.Add("@TrangThai", !string.IsNullOrEmpty(trangThai) ? trangThai : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<UserPagingDto>(
                "Proc_UserSanGD_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            var nhomUsers = await context.TblUserthuocnhoms
                .Select(d => new NhomUserOfUserModel
                {
                    MaNhanVien = d.MaNv,
                    MaNhom = d.MaNhomUser,
                    UserId = d.UserId,
                    IsChecked = true
                })
                .ToListAsync();

            foreach (var cv in result)
            {
                cv.nhomUsers = nhomUsers
                    .Where(x => x.UserId == cv.ID)
                    .ToList();
            }

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Thêm, xóa, sửa nhân viên
        public async Task<ResultModel> SaveUserAsync(UserModel model, string domain)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();

                var entity = await _context.TblUsers.FirstOrDefaultAsync(d => d.TenDangNhap == model.TenDangNhap);
                if (entity != null)
                    return ResultModel.Fail($"Tên đăng nhập: {model.TenDangNhap} đã tồn tại trong hệ thống!");

                var nhanVien = await _context.TblUsers.FirstOrDefaultAsync(d => d.MaNhanVien == model.MaNhanVien);
                if (nhanVien != null)
                    return ResultModel.Fail($"Nhân viên: {model.TenNhanVien} đã tạo tài khoản sử dụng trước đó!");

                //if (model.MatKhau != model.XacNhanMatKhau)
                //	return ResultModel.Fail($"Xác nhận mật khẩu không khớp với mật khẩu");

                model.MatKhau = GenerateRandomPassword(10);

                var record = new TblUser
                {
                    TenDangNhap = model.TenDangNhap,
                    MatKhau = BCrypt.Net.BCrypt.HashPassword(model.MatKhau),
                    MaNhanVien = model.MaNhanVien,
                    NguoiLap = NguoiLap.MaNhanVien,
                    NgayLap = DateTime.Now,
                    TrangThai = true,
                    LoaiUser = model.LoaiUser
                };

                await _context.TblUsers.AddAsync(record);
                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(model.NhomUsers))
                {
                    var groups = model.NhomUsers.Split(',').ToArray();
                    var entities = groups
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(g => new TblUserthuocnhom
                    {
                        UserId = record.Id,
                        MaNhomUser = g,
                        MaNv = model.MaNhanVien
                    })
                    .ToList();

                    await _context.TblUserthuocnhoms.AddRangeAsync(entities);
                    await _context.SaveChangesAsync();
                }

                try
                {
                    var tieuDe = "Thông tin đăng nhập hệ thống";
                    var noiDung = GenerateCreateUserEmailHtml(domain, model.TenNhanVien, model.TenDangNhap, model.MatKhau);

                    await _baseService.AddEmailToSend(model.Email, tieuDe, noiDung, NguoiLap.MaNhanVien);
                }
                catch { }

                return ResultModel.SuccessWithId(record.Id.ToString(), "Thêm người sử dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SaveUserAsync] Lỗi khi Thêm người sử dụng");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể thêm người sử dụng: {ex.Message}");
            }
        }

        public async Task<ResultModel> FindUserAsync(string? id = null)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    id = _currentUser.UserID?.ToString();

                using var _context = _factory.CreateDbContext();
                var entity = await (
                      from u in _context.TblUsers
                      join nv in _context.TblNhanviens on u.MaNhanVien equals nv.MaNhanVien into nvGroup
                      from nv in nvGroup.DefaultIfEmpty()
                      join s in _context.DmSanGiaoDiches on u.MaNhanVien equals s.MaSanGiaoDich into sanGroup
                      from s in sanGroup.DefaultIfEmpty()
                      where u.Id == Convert.ToInt32(id)
                      select new UserModel
                      {
                          Id = u.Id,
                          IdNhanVien = nv.Id,
                          MaNhanVien = u.MaNhanVien ?? s.MaSanGiaoDich,
                          TenNhanVien = nv.HoVaTen ?? s.TenSanGiaoDich,
                          TenDangNhap = u.TenDangNhap,
                          Email = nv.Email ?? s.Email,
                          SoDienThoai = nv.SoDienThoai ?? s.DienThoai,
                          TrangThai = u.TrangThai,
                          TenLoaiUser = u.LoaiUser == "SGG" ? "Sàn giao dịch" : "Nhân viên",
                          LoaiUser = u.LoaiUser ?? "NV"
                      }).FirstOrDefaultAsync();

                if (entity == null)
                    return ResultModel.Fail($"Không tìm thấy thông tin người sử dụng");

                if (entity.LoaiUser == "SGG")
                {
                    entity.NhomUsers = string.Join(',', _context.TblUserthuocnhoms.Where(d => d.MaNv == entity.MaNhanVien && d.UserId == entity.Id).Select(d => d.MaNhomUser).ToList());
                }
                return ResultModel.SuccessWithData(entity, string.Empty);
            }
            catch (Exception ex)
            {
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin người sử dụng: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateUserAsync(UserModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();

                var user = await _context.TblUsers.FirstOrDefaultAsync(d => d.Id == model.Id);
                if (user == null)
                    return ResultModel.Fail("Không tìm thấy người sử dụng nào");

                user.TrangThai = model.TrangThai;
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Cập nhật người sử dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateUserAsync] Lỗi khi cập nhật người sử dụng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateUserSanGDAsync(UserModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();

                var user = await _context.TblUsers.FirstOrDefaultAsync(d => d.Id == model.Id);
                if (user == null)
                    return ResultModel.Fail("Không tìm thấy người sử dụng nào");

                user.TrangThai = model.TrangThai;
                var delNhom = await _context.TblUserthuocnhoms.Where(d => d.MaNv == user.MaNhanVien && d.UserId == user.Id).ToListAsync();
                if (delNhom.Any())
                    _context.TblUserthuocnhoms.RemoveRange(delNhom);

                if (!string.IsNullOrEmpty(model.NhomUsers))
                {
                    var groups = model.NhomUsers.Split(',').ToArray();
                    var entities = groups
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(g => new TblUserthuocnhom
                    {
                        UserId = user.Id,
                        MaNhomUser = g,
                        MaNv = user.MaNhanVien
                    })
                    .ToList();

                    await _context.TblUserthuocnhoms.AddRangeAsync(entities);
                }

                await _context.SaveChangesAsync();

                return ResultModel.Success($"Cập nhật người sử dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateUserAsync] Lỗi khi cập nhật người sử dụng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteUserAsync(string id)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();
                var user = await _context.TblUsers.FirstOrDefaultAsync(d => d.Id == Convert.ToInt32(id));
                if (user == null)
                    return ResultModel.Fail("Không tìm thấy người sử dụng nào");

                var userNhoms = await _context.TblUserthuocnhoms.Where(d => d.UserId == user.Id).ToListAsync();
                _context.TblUserthuocnhoms.RemoveRange(userNhoms);
                _context.TblUsers.Remove(user);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Xóa người sử dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteUserAsync] Lỗi khi xóa người sử dụng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> DeleteListAsync(List<UserPagingDto> lists)
        {
            try
            {
                var ids = lists?
                .Where(x => x?.IsSelected == true && x.ID != null)
                .Select(x => x.ID)
                .ToList() ?? new List<int?>();

                if (ids.Count == 0)
                    return ResultModel.Success("Không có dòng nào được chọn để xoá.");

                using var _context = _factory.CreateDbContext();
                var targetIds = ids;
                await using var tx = await _context.Database.BeginTransactionAsync();

                var c1 = await _context.TblUsers
                   .Where(d => targetIds.Contains(d.Id))
                   .ExecuteDeleteAsync();

                var c2 = await _context.TblUserthuocnhoms
                     .Where(d => targetIds.Contains(d.UserId))
                     .ExecuteDeleteAsync();

                await tx.CommitAsync();

                return ResultModel.Success($"Xóa danh sách người sử dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteListAsync] Lỗi khi xóa danh sách người sử dụng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region Reset mật khẩu
        public async Task<ResultModel> ResetPasswordUserAsync(UserModel model, string domain)
        {
            try
            {
                using var _context = _factory.CreateDbContext();

                var user = await _context.TblUsers.FirstOrDefaultAsync(d => d.Id == model.Id);
                if (user == null)
                    return ResultModel.Fail("Không tìm thấy người sử dụng nào");

                var passDefault = GenerateRandomPassword(10);
                var MatKhau = BCrypt.Net.BCrypt.HashPassword(passDefault);
                user.MatKhau = MatKhau;
                await _context.SaveChangesAsync();

                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();

                var tieuDe = "Thay đổi mật khẩu đăng nhập hệ thống";
                var noiDung = GenerateResetPasswrodUserEmailHtml(domain, model.TenNhanVien, model.TenDangNhap, passDefault);

                await _baseService.AddEmailToSend(model.Email, tieuDe, noiDung, NguoiLap.MaNhanVien);

                return ResultModel.Success($"Thay đổi mật khẩu người sử dụng thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ResetPasswordUserAsync] Lỗi khi thay đổi mật khẩu người sử dụng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> ChangePasswordUserAsync(UserModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var userId = _currentUser.UserID;
                var user = await _context.TblUsers.FirstOrDefaultAsync(d => d.Id == userId);
                if (user == null || !BCrypt.Net.BCrypt.Verify(model.MatKhau, user.MatKhau))
                    return ResultModel.Fail("Mật khẩu hiện tại không đúng");

                var matKhauMoi = BCrypt.Net.BCrypt.HashPassword(model.MatKhauMoi);
                user.MatKhau = matKhauMoi;
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Thay đổi mật khẩu thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChangePasswordUserAsync] Lỗi khi thay đổi mật khẩu người sử dụng");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        #endregion

        #region update status chọn nhóm user
        public async Task<ResultModel> UpdateCheckboxUserAsync(string maNhanVien, string maNhomUser, int? userId, bool isChecked)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                if (isChecked)
                {
                    var nhomUserOfUser = new TblUserthuocnhom
                    {
                        MaNv = maNhanVien,
                        UserId = Convert.ToInt32(userId),
                        MaNhomUser = maNhomUser,
                    };
                    await _context.TblUserthuocnhoms.AddAsync(nhomUserOfUser);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var nhomUserOfUser = await _context.TblUserthuocnhoms.FirstOrDefaultAsync(d => d.MaNv == maNhanVien && d.MaNhomUser == maNhomUser && d.UserId == userId);
                    if (nhomUserOfUser == null)
                        return ResultModel.Fail("Không tìm thấy nhóm của người dùng");

                    _context.TblUserthuocnhoms.Remove(nhomUserOfUser);
                    await _context.SaveChangesAsync();
                }
                return ResultModel.Success("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateCheckboxUserAsync] Lỗi khi cập nhật hiển thị phân quyền user");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }

        }
        #endregion

        #region Random mật khẩu mặt định
        public static string GenerateRandomPassword(int length = 10)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string specials = "!@#$%^&*?";

            string allChars = upper + lower + digits + specials;
            var random = new Random();

            // Đảm bảo mỗi loại ký tự xuất hiện ít nhất 1 lần
            var password = new List<char>
    {
        upper[random.Next(upper.Length)],
        lower[random.Next(lower.Length)],
        digits[random.Next(digits.Length)],
        specials[random.Next(specials.Length)]
    };

            // Bổ sung các ký tự còn lại
            for (int i = password.Count; i < length; i++)
            {
                password.Add(allChars[random.Next(allChars.Length)]);
            }

            // Trộn ngẫu nhiên lại
            return new string(password.OrderBy(_ => random.Next()).ToArray());
        }
        #endregion

        #region Nội dung email
        public string GenerateCreateUserEmailHtml(string domain, string fullName, string username, string password, string? note = null)
        {
            var html = $@"
        <div style='font-family:Segoe UI, sans-serif; font-size:14px; color:#333'>
            <h2 style='color:#2d8cf0;'>Chào {fullName},</h2>
            <p>Bạn đã được tạo tài khoản để đăng nhập hệ thống.</p>

            <table style='border-collapse:collapse; margin-top:10px;'>
                <tr>
                    <td style='padding:6px 12px; font-weight:bold;'>Tên đăng nhập:</td>
                    <td style='padding:6px 12px; background:#f0f0f0;'>{username}</td>
                </tr>
                <tr>
                    <td style='padding:6px 12px; font-weight:bold;'>Mật khẩu:</td>
                    <td style='padding:6px 12px; background:#f0f0f0;'>{password}</td>
                </tr>
            </table>

            <p style='margin-top:15px;'>Bạn vui lòng đăng nhập tại địa chỉ:</p>
            <p><a href='{domain}' style='color:#2d8cf0;'>{domain}</a></p>

            {(string.IsNullOrEmpty(note) ? "" : $"<p style='margin-top:10px; color:#555'><i>{note}</i></p>")}

            <hr style='margin:20px 0;' />
            <p style='font-size:12px; color:#999'>Email này được gửi tự động từ hệ thống. Vui lòng không phản hồi lại email này.</p>
        </div>";
            return html;
        }
        public string GenerateResetPasswrodUserEmailHtml(string domain, string fullName, string username, string password, string? note = null)
        {
            var html = $@"
        <div style='font-family:Segoe UI, sans-serif; font-size:14px; color:#333'>
            <h2 style='color:#2d8cf0;'>Chào {fullName},</h2>
            <p>Bạn đã được thay đổi mật khẩu để đăng nhập hệ thống.</p>

            <table style='border-collapse:collapse; margin-top:10px;'>
                <tr>
                    <td style='padding:6px 12px; font-weight:bold;'>Tên đăng nhập:</td>
                    <td style='padding:6px 12px; background:#f0f0f0;'>{username}</td>
                </tr>
                <tr>
                    <td style='padding:6px 12px; font-weight:bold;'>Mật khẩu:</td>
                    <td style='padding:6px 12px; background:#f0f0f0;'>{password}</td>
                </tr>
            </table>

            <p style='margin-top:15px;'>Bạn vui lòng đăng nhập tại địa chỉ:</p>
            <p><a href='{domain}' style='color:#2d8cf0;'>{domain}</a></p>

            {(string.IsNullOrEmpty(note) ? "" : $"<p style='margin-top:10px; color:#555'><i>{note}</i></p>")}

            <hr style='margin:20px 0;' />
            <p style='font-size:12px; color:#999'>Email này được gửi tự động từ hệ thống. Vui lòng không phản hồi lại email này.</p>
        </div>";
            return html;
        }
        #endregion

        #region get công việc theo user
        public async Task<List<CongViecModel>> GetTreeVuViecByCongViecUserAsync(string userId)
        {
            using var context = _factory.CreateDbContext();
            var congViecs = new List<CongViecModel>();
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("Proc_CayCongViec_ToanBoMenu_ByUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserId", !string.IsNullOrEmpty(userId) ? userId : DBNull.Value);

                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            congViecs.Add(new CongViecModel
                            {
                                MaCongViec = reader["MaCongViec"]?.ToString(),
                                TenCongViec = reader["TenCongViec"]?.ToString(),
                                MaCha = string.IsNullOrEmpty(reader["MaCha"].ToString()) ? string.Empty : reader["MaCha"].ToString(),
                                LevelCay = string.IsNullOrEmpty(reader["LevelCay"]?.ToString()) ? 0 : Convert.ToInt32(reader["LevelCay"]),
                                OrderPath = reader["OrderPath"]?.ToString(),
                                TreeRoot = reader["TreeRoot"]?.ToString(),
                                DoUuTien = string.IsNullOrEmpty(reader["DoUuTien"]?.ToString()) ? 0 : Convert.ToInt32(reader["DoUuTien"]),
                                DoUuTienRoot = string.IsNullOrEmpty(reader["DoUuTienRoot"]?.ToString()) ? 0 : Convert.ToInt32(reader["DoUuTienRoot"]),
                                IsCapCon = string.IsNullOrEmpty(reader["IsCapCon"]?.ToString()) ? 0 : Convert.ToInt32(reader["IsCapCon"]),
                                HienThiTrenMenu = string.IsNullOrEmpty(reader["HienThiTrenMenu"]?.ToString()) ? false : Convert.ToBoolean(reader["HienThiTrenMenu"]),
                                vuViecs = new List<VuViecOfCongViecModel>()
                            });
                        }
                    }
                }
                if (!string.IsNullOrEmpty(userId))
                {
                    var nhomUser = context.TblUserthuocnhoms.Where(d => d.UserId == Convert.ToInt32(userId)).Select(d => d.MaNhomUser).ToArray();
                    if (nhomUser != null)
                    {
                        var vuViecs = await context.TblCongviecvavuviecs.Where(d => nhomUser.Contains(d.MaNhomUser))
                        .Select(d => new VuViecOfCongViecModel
                        {
                            MaCongViec = d.MaCongViec,
                            MaVuViec = d.MaVuViec,
                            IsChecked = true
                        })
                        .ToListAsync();

                        foreach (var cv in congViecs)
                        {
                            cv.vuViecs = vuViecs
                                .Where(x => x.MaCongViec == cv.MaCongViec)
                                .ToList();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetTreeVuViecByCongViecAsync] Lỗi khi lấy toàn bộ danh sách công việc theo mã công việc");
            }
            return congViecs;
        }
        #endregion

        #region List user
        public async Task<List<TblUser>> GetListUserAsync()
        {
            var result = new List<TblUser>();
            try
            {
                using var context = _factory.CreateDbContext();
                result = await context.TblUsers.Where(d => d.TrangThai == true).ToListAsync();
                return result;
            }
            catch
            {
                result = new List<TblUser>();
            }
            return result;
        }
        #endregion

        #region Get nhóm theo user sàn
        public async Task<List<NhomUserModel>> GetNhomUserBySanGDAsync()
        {
            try
            {
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                if (NguoiLap == null)
                    return new List<NhomUserModel>();

                var maSanGD = NguoiLap.MaNhanVien;
                using var context = _factory.CreateDbContext();
                var blocks = await (
                    from us in context.TblUserthuocnhoms
                    join n in context.TblNhomusers on us.MaNhomUser equals n.MaNhomUser into nhomGroup
                    from n in nhomGroup.DefaultIfEmpty()
                    where us.MaNv == maSanGD
                    select new NhomUserModel
                    {
                        MaNhomUser = n.MaNhomUser,
                        TenNhomUser = n.TenNhomUser

                    }).Distinct().ToListAsync();
                return blocks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách nhóm nhóm người dùng");
            }
            return new List<NhomUserModel>();
        }
        #endregion
    }
}

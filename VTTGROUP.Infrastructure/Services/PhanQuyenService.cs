using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.NhanVien;
using VTTGROUP.Domain.Model.PhanQuyen;
using VTTGROUP.Infrastructure.Database;
using static Dapper.SqlMapper;

namespace VTTGROUP.Infrastructure.Services
{
    public class PhanQuyenService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<BlockService> _logger;
        private readonly IConfiguration _config;
        public PhanQuyenService(IDbContextFactory<AppDbContext> factory, ILogger<BlockService> logger, IConfiguration config)
        {
            _factory = factory;
            _logger = logger;
            _config = config;
        }

        #region Get nhóm user paging
        public async Task<(List<NhomUserPagingDto> Data, int TotalCount)> GetNhomUserPagingAsync(int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();

            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhomUserPagingDto>(
                "Proc_NhomUser_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Get nhân viên theo nhóm user paging
        public async Task<(List<NhanVienNhomUserPagingDto> Data, int TotalCount)> GetNhanVienByNhomPopupAsync(string? maNhom, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();
            param.Add("@MaNhomUser", !string.IsNullOrEmpty(maNhom) ? maNhom : null);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhanVienNhomUserPagingDto>(
                "Proc_NhanVienNhomUser_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Get nhóm user
        public async Task<List<NhomUserModel>> GetNhomUserAsync()
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var blocks = await context.TblNhomusers.Select(d => new NhomUserModel
                {
                    MaNhomUser = d.MaNhomUser,
                    TenNhomUser = d.TenNhomUser

                }).ToListAsync();
                return blocks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy toàn bộ danh sách nhóm nhóm người dùng");
            }
            return new List<NhomUserModel>();
        }
        public async Task<ResultModel> GetNhomUserByMaNhomAsync(string? id)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var nhom = await context.TblNhomusers.FirstOrDefaultAsync(d => d.MaNhomUser == id);
                if (nhom == null)
                    return ResultModel.Fail("Không tìm thấy thông tin của nhóm người dùng");

                var model = new NhomUserModel
                {
                    MaNhomUser = nhom.MaNhomUser,
                    TenNhomUser = nhom.TenNhomUser,
                    GhiChu = nhom.GhiChu
                };

                var listNhanVien = await (
                      from us in context.TblUserthuocnhoms
                      join u in context.TblUsers on us.UserId equals u.Id
                      join nv in context.TblNhanviens on u.MaNhanVien equals nv.MaNhanVien into nvGroup
                      from nv in nvGroup.DefaultIfEmpty()
                      join s in context.DmSanGiaoDiches on u.MaNhanVien equals s.MaSanGiaoDich into sanGroup
                      from s in sanGroup.DefaultIfEmpty()
                      where us.MaNhomUser == id
                      select new NhanVienModel
                      {
                          UserId = us.UserId,
                          MaNhanVien = us.MaNv,
                          HoVaTen = nv.HoVaTen ?? s.TenSanGiaoDich,
                          TenDangNhap = u.TenDangNhap,
                          Email = nv.Email ?? s.Email,
                          SoDienThoai = nv.SoDienThoai ?? s.DienThoai

                      }).ToListAsync();
                model.ListNhanVien = listNhanVien;
                return ResultModel.SuccessWithData(model, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetNhomUserByMaNhomAsync] Lỗi khi lấy toàn bộ danh sách nhóm user");
                return ResultModel.Fail(ex.Message);

            }
        }
        #endregion

        #region Get tree công việc nhóm user
        public async Task<List<CongViecModel>> GetTreePhanQuyenByCongViecAsync(string maCongViec)
        {
            var congViecs = new List<CongViecModel>();
            try
            {
                using var context = _factory.CreateDbContext();
                var connStr = _config.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("Proc_CayCongViec_PhanQuyen_Huy", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@maCongViec", !string.IsNullOrEmpty(maCongViec) ? maCongViec : DBNull.Value);

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
                                MaVuViec = reader["MaVuViec"]?.ToString(),
                                TenQuyen = reader["TenQuyen"]?.ToString(),
                                nhomUsers = new List<NhomUserOfCongViecModel>()
                            });
                        }
                    }
                }

                var nhomUsers = await context.TblCongviecvavuviecs
                .Select(d => new NhomUserOfCongViecModel
                {
                    MaCongViec = d.MaCongViec,
                    MaNhom = d.MaNhomUser,
                    MaVuViec = d.MaVuViec,
                    IsChecked = true
                })
                .ToListAsync();

                foreach (var cv in congViecs)
                {
                    cv.nhomUsers = nhomUsers
                        .Where(x => x.MaCongViec == cv.MaCongViec)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetTreePhanQuyenByCongViecAsync] Lỗi khi lấy toàn bộ danh sách công việc theo mã công việc");
            }
            return congViecs;
        }
        #endregion

        #region update status chọn nhóm user
        public async Task<ResultModel> UpdateCheckboxNhomUserAsync(string maCongViec, string maNhomUser, string maVuViec, bool isChecked)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();

                // 1) Lấy toàn bộ cha (bao gồm ông/bà...) nhưng KHÔNG gồm chính nó
                async Task<List<string>> GetAllAncestorsAsync(string maCv)
                {
                    var pairs = await _context.TblCongviecs
                    .Select(x => new { x.MaCongViec, x.MaCha })
                    .ToListAsync();

                    var map = pairs.ToDictionary(x => x.MaCongViec, x => x.MaCha);
                    var list = new List<string>();
                    var cur = maCv;
                    while (map.TryGetValue(cur, out var parent) && !string.IsNullOrEmpty(parent))
                    {
                        list.Add(parent);
                        cur = parent;
                    }
                    return list;
                }

                // 2) Lấy toàn bộ descendant (cây con) của 1 mã công việc (KHÔNG gồm chính nó)
                async Task<HashSet<string>> GetAllDescendantsAsync(string root)
                {
                    var pairs = await _context.TblCongviecs
                    .Select(x => new { x.MaCongViec, x.MaCha })
                    .ToListAsync();

                    var childMap = pairs
                        .GroupBy(x => x.MaCha ?? string.Empty)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.MaCongViec).ToList());

                    var result = new HashSet<string>();
                    var st = new Stack<string>();
                    st.Push(root);
                    while (st.Count > 0)
                    {
                        var cur = st.Pop();
                        if (!childMap.TryGetValue(cur, out var chs)) continue;
                        foreach (var c in chs)
                        {
                            if (result.Add(c)) st.Push(c);
                        }
                    }
                    result.Remove(root);
                    return result;
                }

                using var tx = await _context.Database.BeginTransactionAsync();

                if (isChecked)
                {
                    if (!string.IsNullOrEmpty(maVuViec))
                    {
                        var nhomUserOfCongViec = new TblCongviecvavuviec
                        {
                            MaCongViec = maCongViec,
                            MaVuViec = maVuViec,
                            MaNhomUser = maNhomUser,
                        };
                        await _context.TblCongviecvavuviecs.AddAsync(nhomUserOfCongViec);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {

                        var nhomUserOfCongViec = await _context.TblCongviecvavuviecs.Where(d => d.MaCongViec == maCongViec && d.MaNhomUser == maNhomUser).ToListAsync();
                        _context.TblCongviecvavuviecs.RemoveRange(nhomUserOfCongViec);

                        var vuViecOfCongViec = await _context.TblVuvieccuacongviecs.Where(d => d.MaCongViec == maCongViec).ToListAsync();
                        List<TblCongviecvavuviec> listVuViecOfNhoms = new List<TblCongviecvavuviec>();
                        foreach (var item in vuViecOfCongViec)
                        {
                            listVuViecOfNhoms.Add(new TblCongviecvavuviec
                            {
                                MaCongViec = maCongViec,
                                MaNhomUser = maNhomUser,
                                MaVuViec = item.MaVuViec
                            });
                        }
                        await _context.TblCongviecvavuviecs.AddRangeAsync(listVuViecOfNhoms);
                        await _context.SaveChangesAsync();

                    }

                    var ancestors = await GetAllAncestorsAsync(maCongViec);
                    if (ancestors.Count > 0)
                    {
                        var existedParents = await _context.TblCongviecvavuviecs
                            .Where(t => t.MaNhomUser == maNhomUser
                                     && t.MaVuViec == "001"
                                     && ancestors.Contains(t.MaCongViec))
                            .Select(t => t.MaCongViec)
                            .ToListAsync();

                        var toInsert = ancestors
                            .Except(existedParents)
                            .Select(cv => new TblCongviecvavuviec
                            {
                                MaCongViec = cv,
                                MaNhomUser = maNhomUser,
                                MaVuViec = "001"
                            })
                            .ToList();

                        if (toInsert.Count > 0)
                        {
                            await _context.TblCongviecvavuviecs.AddRangeAsync(toInsert);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(maVuViec))
                    {
                        var nhomUserOfCongViec = await _context.TblCongviecvavuviecs.FirstOrDefaultAsync(d => d.MaCongViec == maCongViec && d.MaNhomUser == maNhomUser && d.MaVuViec == maVuViec);
                        if (nhomUserOfCongViec == null)
                            return ResultModel.Fail("Không tìm thấy vụ việc của công việc");

                        _context.TblCongviecvavuviecs.Remove(nhomUserOfCongViec);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        var nhomUserOfCongViec = await _context.TblCongviecvavuviecs.Where(d => d.MaCongViec == maCongViec && d.MaNhomUser == maNhomUser).ToListAsync();
                        _context.TblCongviecvavuviecs.RemoveRange(nhomUserOfCongViec);
                        await _context.SaveChangesAsync();

                    }

                    // 1) Load cấu trúc cây 1 lần để dùng chung
                    var pairs = await _context.TblCongviecs
                        .Select(x => new { x.MaCongViec, x.MaCha })
                        .ToListAsync();

                    var parentMap = pairs.ToDictionary(x => x.MaCongViec, x => x.MaCha);
                    var childMap = pairs
                        .GroupBy(x => x.MaCha ?? string.Empty)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.MaCongViec).ToList());

                    // helper: ancestors (gần -> xa)
                    List<string> GetAllAncestors(string cv)
                    {
                        var list = new List<string>();
                        var cur = cv;
                        while (parentMap.TryGetValue(cur, out var p) && !string.IsNullOrEmpty(p))
                        {
                            list.Add(p!);
                            cur = p!;
                        }
                        return list;
                    }

                    // helper: descendants (KHÔNG gồm chính nó)
                    HashSet<string> GetAllDescendants(string root)
                    {
                        var result = new HashSet<string>();
                        var st = new Stack<string>();
                        st.Push(root);
                        while (st.Count > 0)
                        {
                            var cur = st.Pop();
                            if (!childMap.TryGetValue(cur, out var chs)) continue;
                            foreach (var c in chs)
                            {
                                if (result.Add(c)) st.Push(c);
                            }
                        }
                        result.Remove(root);
                        return result;
                    }

                    // 2) Lấy toàn bộ CV đang được gán của nhóm (mọi MaVuViec) để kiểm nhanh trong bộ nhớ
                    var assignedCv = await _context.TblCongviecvavuviecs
                        .Where(t => t.MaNhomUser == maNhomUser)
                        .Select(t => t.MaCongViec)
                        .ToListAsync();
                    var assignedSet = new HashSet<string>(assignedCv);

                    // 3) Duyệt ancestor từ gần -> xa, chỉ xoá khi cây con KHÔNG còn assignment nào
                    var ancestors = GetAllAncestors(maCongViec);

                    foreach (var parent in ancestors)
                    {
                        var descendants = GetAllDescendants(parent);

                        // Có con cháu nào còn được gán cho nhóm không? (bất kỳ MaVuViec nào)
                        bool hasAnyChildAssigned = descendants.Any(assignedSet.Contains);

                        if (!hasAnyChildAssigned)
                        {
                            // Không còn gì ở cây con => gỡ '001' của parent (nếu có)
                            var parentRow = await _context.TblCongviecvavuviecs.FirstOrDefaultAsync(t =>
                                t.MaCongViec == parent &&
                                t.MaNhomUser == maNhomUser &&
                                t.MaVuViec == "001");

                            if (parentRow != null)
                            {
                                _context.TblCongviecvavuviecs.Remove(parentRow);

                                // Cập nhật "trạng thái" trong bộ nhớ để cấp cao hơn thấy parent này cũng đã gỡ
                                assignedSet.Remove(parent);
                            }
                        }
                        // Nếu vẫn còn con cháu được gán => giữ nguyên parent
                    }

                    // 4) Lưu tất cả removals cho ancestor 1 lần
                    await _context.SaveChangesAsync();

                }
                await tx.CommitAsync();
                return ResultModel.Success("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateCheckboxNhomUserAsync] Lỗi khi cập nhật hiển thị phân quyền công việc");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }

        }

        public async Task<ResultModel> UpdateTrangThaiNhomUserAsync(string maNhomUser, bool isChecked)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                _context.ChangeTracker.Clear();
                var nhomUser = await _context.TblNhomusers.FirstOrDefaultAsync(d => d.MaNhomUser == maNhomUser);
                if (nhomUser == null)
                    return ResultModel.Fail("Không tìm thấy thông tin nhóm quyền sử dụng");
                nhomUser.TrangThai = isChecked;
                await _context.SaveChangesAsync();

                return ResultModel.Success("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateCheckboxNhomUserAsync] Lỗi khi cập nhật hiển thị phân quyền công việc");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }

        }
        #endregion

        #region Delete Nhóm User
        public async Task<ResultModel> DeleteNhomUserAsync(NhomUserModel nhom)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var congViecOfNhomUser = await _context.TblCongviecvavuviecs.Where(d => d.MaNhomUser == nhom.MaNhomUser).ToListAsync();
                _context.TblCongviecvavuviecs.RemoveRange(congViecOfNhomUser);

                var userNhomOfUsers = await _context.TblUserthuocnhoms.Where(d => d.MaNhomUser == nhom.MaNhomUser).ToListAsync();
                _context.TblUserthuocnhoms.RemoveRange(userNhomOfUsers);

                var nhomUser = await _context.TblNhomusers.FirstOrDefaultAsync(d => d.MaNhomUser == nhom.MaNhomUser);
                _context.TblNhomusers.Remove(nhomUser);

                await _context.SaveChangesAsync();

                return ResultModel.Success($"Xóa nhóm {nhom.TenNhomUser} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteNhomUserAsync] Lỗi khi xóa nhóm user");
                return ResultModel.Fail($"Lỗi hệ thống: không thể xóa: {ex.Message}");
            }
        }
        #endregion

        #region Thêm nhóm user
        public async Task<ResultModel> SaveNhomUserAsync(NhomUserModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.TblNhomusers.FirstOrDefaultAsync(d => d.MaNhomUser.ToLower() == model.MaNhomUser.ToLower());
                if (entity != null)
                {
                    return ResultModel.Fail($"Nhóm {model.TenNhomUser} đã tồn tại");
                }
                TblNhomuser record = new TblNhomuser
                {
                    MaNhomUser = model.MaNhomUser,
                    TenNhomUser = model.TenNhomUser,
                    GhiChu = model.GhiChu
                };

                await _context.TblNhomusers.AddAsync(record);
                await _context.SaveChangesAsync();

                return ResultModel.Success($"Thêm nhóm {model.TenNhomUser} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SaveNhomUserAsync] Lỗi khi thêm nhóm người dùng");
                return ResultModel.Fail($"Lỗi hệ thống: không thể thêm: {ex.Message}");
            }
        }
        #endregion

        #region Get nhân viên theo user paging
        public async Task<(List<NhanVienUserPagingDto> Data, int TotalCount)> GetNhanVienByUserPopupAsync(int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<NhanVienUserPagingDto>(
                "Proc_NhanVienUser_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Cập nhật nhóm
        public async Task<ResultModel> UpdateNhomUserAsync(NhomUserModel model)
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var entity = await _context.TblNhomusers.FirstOrDefaultAsync(d => d.MaNhomUser.ToLower() == model.MaNhomUser.ToLower());
                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy thông tin nhóm người dùng");

                entity.TenNhomUser = model.TenNhomUser;
                entity.GhiChu = model.GhiChu;

                var listOld = await _context.TblUserthuocnhoms.Where(d => d.MaNhomUser.ToLower() == model.MaNhomUser.ToLower()).ToListAsync();
                _context.TblUserthuocnhoms.RemoveRange(listOld);

                if (model.ListNhanVien != null && model.ListNhanVien.Any())
                {
                    List<TblUserthuocnhom> listNhom = new List<TblUserthuocnhom>();
                    foreach (var item in model.ListNhanVien)
                    {
                        var record = new TblUserthuocnhom
                        {
                            MaNhomUser = model.MaNhomUser,
                            MaNv = item.MaNhanVien,
                            UserId = Convert.ToInt32(item.UserId),
                        };
                        listNhom.Add(record);
                    }
                    await _context.TblUserthuocnhoms.AddRangeAsync(listNhom);
                }

                await _context.SaveChangesAsync();

                return ResultModel.Success($"Cập nhậtt nhóm {model.MaNhomUser} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SaveNhomUserAsync] Lỗi khi thêm nhóm user");
                return ResultModel.Fail($"Lỗi hệ thống: không thể thêm: {ex.Message}");
            }
        }
        #endregion
    }
}

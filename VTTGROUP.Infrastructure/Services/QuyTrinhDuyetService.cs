using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.DuAn;
using VTTGROUP.Domain.Model.QuyTrinhDuyet;
using VTTGROUP.Infrastructure.Database;
using static Dapper.SqlMapper;

namespace VTTGROUP.Infrastructure.Services
{
    public class QuyTrinhDuyetService
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString;
        private readonly ILogger<QuyTrinhDuyetService> _logger;
        private readonly IConfiguration _config;
        private readonly ICurrentUserService _currentUser;

        public QuyTrinhDuyetService(AppDbContext context, ILogger<QuyTrinhDuyetService> logger, IConfiguration config, ICurrentUserService currentUser)
        {
            _context = context;
            _logger = logger;
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
            _currentUser = currentUser;
        }

        #region Paging index
        public async Task<(List<QuyTrinhDuyetPagingDto> Data, int TotalCount)> GetPagingAsync(string maCongViec, string maDuAn, string? trangThai, int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();

            param.Add("@MaCongViec", !string.IsNullOrEmpty(maCongViec) ? maCongViec : null);
            param.Add("@MaDuAn", !string.IsNullOrEmpty(maDuAn) ? maDuAn : null);
            param.Add("@TrangThai", trangThai);
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<QuyTrinhDuyetPagingDto>(
                "Proc_QuyTrinhDuyet_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }
        #endregion

        #region Get thông tin phiếu
        public async Task<ResultModel> GetByIdAsync(string? id)
        {
            try
            {
                var entity = await (from t in _context.HtQuyTrinhDuyets
                                    join cv in _context.TblCongviecs on t.MaCongViec equals cv.MaCongViec into cvGroup
                                    from cv in cvGroup.DefaultIfEmpty()
                                    where t.Id == Convert.ToInt32(id)
                                    select new QuyTrinhDuyetModel
                                    {
                                        Id = t.Id,
                                        TenQuyTrinh = t.TenQuyTrinh,
                                        TrangThai = t.TrangThai,
                                        NgayLap = t.NgayLap ?? DateTime.Now,
                                        MaCongViec = t.MaCongViec,
                                        TenCongViec = cv.TenCongViec,
                                        MaNhanVien = t.NguoiLap,
                                        GhiChu = t.GhiChu
                                    }).FirstOrDefaultAsync();

                if (entity == null)
                {
                    entity = new QuyTrinhDuyetModel();
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                    entity.NgayLap = DateTime.Now;
                    entity.TrangThai = 1;
                }
                else
                {
                    entity.NguoiLap = await _currentUser.GetThongTinNguoiLapAsync(entity.MaNhanVien);
                    var buocDuyets = await (from t in _context.HtQuyTrinhDuyetBuocDuyets
                                            join bd in _context.HtBuocDuyets on t.MaBuocDuyet equals bd.MaBuocDuyet into bdGroup
                                            from bd in bdGroup.DefaultIfEmpty()

                                            join nv in _context.TblNhanviens on t.NguoiDuyet equals nv.MaNhanVien into nvGroup
                                            from nv in nvGroup.DefaultIfEmpty()
                                            where t.IdQuyTrinh == Convert.ToInt32(id)
                                            select new QuyTrinhDuyetBuocDuyetModel
                                            {
                                                BuocDuyet = new BuocDuyetModel
                                                {
                                                    MaBuocDuyet = t.MaBuocDuyet,
                                                    TenBuocDuyet = bd.TenBuocDuyet
                                                },
                                                NguoiDuyet = new Domain.Model.NhanVien.NguoiLapModel
                                                {
                                                    MaNhanVien = t.NguoiDuyet,
                                                    HoVaTen = nv.HoVaTen
                                                },
                                                ThuTu = t.ThuTu
                                            }).ToListAsync();
                    entity.ListBuocDuyet = buocDuyets;

                    var duAns = await (from t in _context.HtQuyTrinhDuyetDuAns
                                       join da in _context.DaDanhMucDuAns on t.MaDuAn equals da.MaDuAn into daGroup
                                       from da in daGroup.DefaultIfEmpty()
                                       where t.IdQuyTrinh == Convert.ToInt32(id)
                                       select new DuAnModel
                                       {
                                           MaDuAn = t.MaDuAn,
                                           TenDuAn = da.TenDuAn
                                       }).ToListAsync();

                    entity.ListDuAn = duAns;
                }
                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin quy trình duyệt");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        #endregion

        #region get danh sách bước duyệt paging
        public async Task<(List<BuocDuyetPagingDto> Data, int TotalCount)> GetPagingBuocDuyetAsync(int page, int pageSize, string? qSearch)
        {
            qSearch = string.IsNullOrEmpty(qSearch) ? null : qSearch;
            var connStr = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connStr);
            var param = new DynamicParameters();
            param.Add("@Page", page);
            param.Add("@PageSize", pageSize);
            param.Add("@QSearch", qSearch);

            var result = (await connection.QueryAsync<BuocDuyetPagingDto>(
                "Proc_BuocDuyet_GetPaging",
                param,
                commandType: CommandType.StoredProcedure
            )).ToList();

            int total = result.FirstOrDefault()?.TotalCount ?? 0;

            return (result, total);
        }

        #endregion

        #region Thêm, xóa, sửa 
        public async Task<ResultModel> SavePhieuAsync(QuyTrinhDuyetModel model)
        {
            try
            {
                HtQuyTrinhDuyet record = new HtQuyTrinhDuyet
                {
                    MaCongViec = model.MaCongViec,
                    TenQuyTrinh = model.TenQuyTrinh,
                    GhiChu = model.GhiChu,
                    NgayLap = DateTime.Now,
                    TrangThai = model.TrangThai,
                };
                var NguoiLap = await _currentUser.GetThongTinNguoiLapAsync();
                record.NguoiLap = NguoiLap.MaNhanVien;
                await _context.HtQuyTrinhDuyets.AddAsync(record);
                await _context.SaveChangesAsync();

                var listBuocDuyets = new List<HtQuyTrinhDuyetBuocDuyet>();
                if (model.ListBuocDuyet != null && model.ListBuocDuyet.Any())
                {
                    foreach (var item in model.ListBuocDuyet)
                    {
                        var r = new HtQuyTrinhDuyetBuocDuyet
                        {
                            MaBuocDuyet = item.BuocDuyet?.MaBuocDuyet,
                            NguoiDuyet = item.NguoiDuyet?.MaNhanVien,
                            ThuTu = item.ThuTu,
                            IdQuyTrinh = record.Id
                        };
                        listBuocDuyets.Add(r);
                    }

                    await _context.HtQuyTrinhDuyetBuocDuyets.AddRangeAsync(listBuocDuyets);
                }

                var listDuAns = new List<HtQuyTrinhDuyetDuAn>();
                if (model.ListDuAn != null && model.ListDuAn.Any())
                {
                    foreach (var item in model.ListDuAn)
                    {
                        var r = new HtQuyTrinhDuyetDuAn
                        {
                            MaDuAn = item.MaDuAn,
                            IdQuyTrinh = record.Id
                        };
                        listDuAns.Add(r);
                    }

                    await _context.HtQuyTrinhDuyetDuAns.AddRangeAsync(listDuAns);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.Id.ToString(), $"Thêm quy trình duyệt {model.TenQuyTrinh} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SavePhieuAsync] Lỗi khi thêm quy trình duyệt");
                return ResultModel.Fail($"Lỗi hệ thống: không thể thêm: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdatePhieuAsync(QuyTrinhDuyetModel model)
        {
            try
            {
                var entity = await _context.HtQuyTrinhDuyets.FirstOrDefaultAsync(d => d.Id == model.Id);
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy quy trình duyệt.");
                }

                entity.TenQuyTrinh = model.TenQuyTrinh;
                entity.GhiChu = model.GhiChu;
                entity.TrangThai = model.TrangThai;

                var delBuocDuyets = await _context.HtQuyTrinhDuyetBuocDuyets.Where(d => d.IdQuyTrinh == model.Id).ToListAsync();
                _context.HtQuyTrinhDuyetBuocDuyets.RemoveRange(delBuocDuyets);

                var listBuocDuyets = new List<HtQuyTrinhDuyetBuocDuyet>();
                if (model.ListBuocDuyet != null && model.ListBuocDuyet.Any())
                {
                    foreach (var item in model.ListBuocDuyet)
                    {
                        var r = new HtQuyTrinhDuyetBuocDuyet
                        {
                            MaBuocDuyet = item.BuocDuyet?.MaBuocDuyet,
                            NguoiDuyet = item.NguoiDuyet?.MaNhanVien,
                            ThuTu = item.ThuTu,
                            IdQuyTrinh = entity.Id
                        };
                        listBuocDuyets.Add(r);
                    }

                    await _context.HtQuyTrinhDuyetBuocDuyets.AddRangeAsync(listBuocDuyets);
                }

                var delDuAns = await _context.HtQuyTrinhDuyetDuAns.Where(d => d.IdQuyTrinh == model.Id).ToListAsync();
                _context.HtQuyTrinhDuyetDuAns.RemoveRange(delDuAns);

                var listDuAns = new List<HtQuyTrinhDuyetDuAn>();
                if (model.ListDuAn != null && model.ListDuAn.Any())
                {
                    foreach (var item in model.ListDuAn)
                    {
                        var r = new HtQuyTrinhDuyetDuAn
                        {
                            MaDuAn = item.MaDuAn,
                            IdQuyTrinh = entity.Id
                        };
                        listDuAns.Add(r);
                    }

                    await _context.HtQuyTrinhDuyetDuAns.AddRangeAsync(listDuAns);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(entity.Id.ToString(), $"Cập nhật quy trình duyệt {model.TenQuyTrinh} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdatePhieuAsync] Lỗi khi Cập nhật quy trình duyệt");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeletePhieuAsync(int? Id)
        {
            try
            {
                var entity = await _context.HtQuyTrinhDuyets.FirstOrDefaultAsync(d => d.Id == Id);
                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thấy quy trình duyệt.");
                }

                var delBuocDuyets = await _context.HtQuyTrinhDuyetBuocDuyets.Where(d => d.IdQuyTrinh == Id).ToListAsync();
                _context.HtQuyTrinhDuyetBuocDuyets.RemoveRange(delBuocDuyets);

                var delDuAns = await _context.HtQuyTrinhDuyetDuAns.Where(d => d.IdQuyTrinh == Id).ToListAsync();
                _context.HtQuyTrinhDuyetDuAns.RemoveRange(delDuAns);

                _context.HtQuyTrinhDuyets.Remove(entity);

                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xóa quy trình duyệt thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeletePhieuAsync] Lỗi khi xóa quy trình duyệt");
                return ResultModel.Fail($"Lỗi hệ thống: không thể xóa: {ex.Message}");
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
        #endregion

    }
}

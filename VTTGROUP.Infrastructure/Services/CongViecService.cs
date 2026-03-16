using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using VTTGROUP.Domain.Model;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class CongViecService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CongViecService> _logger;
        private readonly IConfiguration _config;
        public CongViecService(AppDbContext context, ILogger<CongViecService> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        public async Task<List<CongViecModel>> GetTreeAllAsync()
        {
            var congViecs = new List<CongViecModel>();
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("Proc_CayCongViec_ToanBoMenu", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@maCongViec", DBNull.Value);

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
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetTreeAllAsync] Lỗi khi lấy toàn bộ danh sách công việc");
            }
            return congViecs;
        }

        public async Task<List<CongViecModel>> GetListCongViecConAsync()
        {
            try
            {
                var vuViecs = await _context.TblCongviecs.Where(d => !string.IsNullOrEmpty(d.MaCha) && d.TrangThaiHienThi == 1).Select(d => new CongViecModel
                {
                    MaCongViec = d.MaCongViec,
                    TenCongViec = d.TenCongViec,
                }).ToListAsync();
                return vuViecs;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetListCongViecConAsync] Lỗi khi lấy toàn bộ danh sách công việc con");
            }
            return new List<CongViecModel>();
        }

        public async Task<List<VuViecModel>> GetVuViecAsync()
        {
            try
            {
                var vuViecs = await _context.TblVuviecs.Select(d => new VuViecModel
                {
                    MaVuViec = d.MaVuViec,
                    TenVuViec = d.TenVuViec,
                }).ToListAsync();
                return vuViecs;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetVuViecAsync] Lỗi khi lấy toàn bộ danh sách công việc");
            }
            return new List<VuViecModel>();
        }

        public async Task<List<CongViecModel>> GetTreeVuViecByCongViecAsync(string maCongViec)
        {
            var congViecs = new List<CongViecModel>();
            try
            {
                var connStr = _config.GetConnectionString("DefaultConnection");
                using (var conn = new SqlConnection(connStr))
                using (var cmd = new SqlCommand("Proc_CayCongViec_ToanBoMenu", conn))
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
                                vuViecs = new List<VuViecOfCongViecModel>()
                            });
                        }
                    }
                }
                var vuViecs = await _context.TblVuvieccuacongviecs
                .Select(d => new VuViecOfCongViecModel
                {
                    Id = d.VuViecCuaCongViecId,
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetTreeVuViecByCongViecAsync] Lỗi khi lấy toàn bộ danh sách công việc theo mã công việc");
            }
            return congViecs;
        }

        public async Task<ResultModel> ChangeHienThiCongViecAsync(string maCongViec)
        {
            try
            {
                var congViec = await _context.TblCongviecs.FirstOrDefaultAsync(d => d.MaCongViec == maCongViec);
                if (congViec == null)
                    return ResultModel.Fail("Không tìm thấy công việc");

                congViec.HienThiTrenMenu = !congViec.HienThiTrenMenu;

                await _context.SaveChangesAsync();
                return ResultModel.Success($"Cập nhật {congViec.TenCongViec} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChangeHienThiCongViecAsync] Lỗi khi cập nhật hiển thị công việc");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }
        }

        public async Task<ResultModel> ChangeDoUuTienAsync(CongViecModel item)
        {
            try
            {
                var congViec = await _context.TblCongviecs.FirstOrDefaultAsync(d => d.MaCongViec == item.MaCongViec);
                if (congViec == null)
                    return ResultModel.Fail("Không tìm thấy công việc");

                congViec.DoUuTien = item.DoUuTien;

                await _context.SaveChangesAsync();
                return ResultModel.Success($"Cập nhật {congViec.TenCongViec} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ChangeDoUuTienAsync] Lỗi khi cập nhật hiển thị công việc");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }
        }
        public async Task<ResultModel> UpdateCheckboxVuViecAsync(string maCongViec, string maVuViec, bool isChecked)
        {
            try
            {
                if (isChecked)
                {
                    var vuViecOfCongViec = new TblVuvieccuacongviec
                    {
                        MaCongViec = maCongViec,
                        MaVuViec = maVuViec
                    };
                    await _context.TblVuvieccuacongviecs.AddAsync(vuViecOfCongViec);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var vuViecOfCongViec = await _context.TblVuvieccuacongviecs.FirstOrDefaultAsync(d => d.MaCongViec == maCongViec && d.MaVuViec == maVuViec);
                    if (vuViecOfCongViec == null)
                        return ResultModel.Fail("Không tìm thấy vụ việc của công việc");

                    _context.TblVuvieccuacongviecs.Remove(vuViecOfCongViec);
                    await _context.SaveChangesAsync();
                }
                return ResultModel.Success("Cập nhật thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateCheckboxVuViecAsync] Lỗi khi cập nhật hiển thị công việc");
                return ResultModel.Fail($"Lỗi hệ thống: không thể cập nhật: {ex.Message}");
            }
        }

        public async Task<ResultModel> DeleteCongViecAsync(string maCongViec)
        {
            try
            {
                _context.ChangeTracker.Clear();
                string tenCongViec = await _context.TblCongviecs
                    .Where(x => x.MaCongViec == maCongViec)
                    .Select(x => x.TenCongViec)
                    .FirstOrDefaultAsync();

                if (tenCongViec == null)
                    return ResultModel.Fail("Không tìm thấy công việc");

                // Xóa quan hệ với vụ việc (nếu có)
                var vuViecs = await _context.TblVuvieccuacongviecs
                    .Where(x => x.MaCongViec == maCongViec)
                    .ToListAsync();

                var congViecsLienQuan = await _context.TblCongviecvavuviecs
                    .Where(x => x.MaCongViec == maCongViec)
                    .ToListAsync();

                // Remove theo batch
                _context.RemoveRange(vuViecs);
                _context.RemoveRange(congViecsLienQuan);

                // Attach rồi remove để không bị lỗi tracking
                var congViec = new TblCongviec { MaCongViec = maCongViec };
                _context.TblCongviecs.Attach(congViec);
                _context.TblCongviecs.Remove(congViec);

                await _context.SaveChangesAsync();
                return ResultModel.Success($"Xóa {tenCongViec} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteCongViecAsync] Lỗi khi xóa công việc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }
        public async Task<ResultModel> SaveCongViecAsync(CongViecModel model)
        {
            try
            {
                bool exists = await _context.TblCongviecs
                    .AnyAsync(d => d.MaCongViec == model.MaCongViec);

                if (exists)
                    return ResultModel.Fail("Công việc đã tồn tại.");

                var record = new TblCongviec
                {
                    MaCongViec = model.MaCongViec,
                    TenCongViec = model.TenCongViec,
                    TenController = model.TenController,
                    TenAction = model.TenAction,
                    TienTo = model.TienTo,
                    DoUuTien = model.DoUuTien,
                    TrangThaiHienThi = model.TrangThaiHienThi,
                    GhiChu = model.GhiChu,
                    MaCha = model.MaCha,
                    HienThiTrenMenu = model.HienThiTrenMenu ?? false
                };

                await _context.TblCongviecs.AddAsync(record);

                if (model.vuViecs?.Any() == true)
                {
                    var vuViecs = model.vuViecs.Select(x => new TblVuvieccuacongviec
                    {
                        MaCongViec = model.MaCongViec, // dùng từ model cho chắc
                        MaVuViec = x.MaVuViec
                    }).ToList();

                    _context.TblVuvieccuacongviecs.AddRange(vuViecs);
                }

                await _context.SaveChangesAsync();
                return ResultModel.SuccessWithId(record.MaCongViec, $"Thêm {record.TenCongViec} thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SaveCongViecAsync] Lỗi khi thêm công việc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> GetByIdAsync(string id)
        {
            try
            {
                var entity = await _context.TblCongviecs
                 .Where(d => d.MaCongViec == id)
                 .Select(d => new CongViecModel
                 {
                     MaCongViec = d.MaCongViec,
                     TenCongViec = d.TenCongViec,
                     TenController = d.TenController,
                     TenAction = d.TenAction,
                     HienThiTrenMenu = d.HienThiTrenMenu,
                     DoUuTien = d.DoUuTien,
                     GhiChu = d.GhiChu,
                     MaCha = d.MaCha,
                     TrangThaiHienThi = d.TrangThaiHienThi,
                     TienTo = d.TienTo

                 })
                 .FirstOrDefaultAsync();

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy công việc.");

                entity.vuViecs = await _context.TblVuvieccuacongviecs.Where(d => d.MaCongViec == entity.MaCongViec)
                    .Select(d => new VuViecOfCongViecModel
                    {
                        Id = d.VuViecCuaCongViecId,
                        MaCongViec = d.MaCongViec,
                        MaVuViec = d.MaVuViec,
                        IsChecked = true
                    })
                    .ToListAsync();

                return ResultModel.SuccessWithData(entity, "Lấy dữ liệu thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GetByIdAsync] Lỗi khi lấy thông tin một công việc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateByIdAsync(CongViecModel model)
        {
            try
            {
                var entity = await _context.TblCongviecs.FirstOrDefaultAsync(d => d.MaCongViec == model.MaCongViec);

                if (entity == null)
                    return ResultModel.Fail("Không tìm thấy công việc.");

                entity.TenCongViec = model.TenCongViec;
                entity.TenController = model.TenController;
                entity.TenAction = model.TenAction;
                entity.TienTo = model.TienTo;
                entity.DoUuTien = model.DoUuTien;
                entity.TrangThaiHienThi = model.TrangThaiHienThi;
                entity.GhiChu = model.GhiChu;
                entity.MaCha = model.MaCha;
                entity.HienThiTrenMenu = model.HienThiTrenMenu ?? false;

                var vuViecOlds = await _context.TblVuvieccuacongviecs
                                    .Where(x => x.MaCongViec == model.MaCongViec)
                                    .ToListAsync();

                _context.RemoveRange(vuViecOlds);

                if (model.vuViecs?.Any() == true)
                {
                    var vuViecs = model.vuViecs.Select(x => new TblVuvieccuacongviec
                    {
                        MaCongViec = model.MaCongViec,
                        MaVuViec = x.MaVuViec
                    }).ToList();

                    _context.TblVuvieccuacongviecs.AddRange(vuViecs);
                }

                await _context.SaveChangesAsync();


                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenCongViec} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật công việc");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }

        }


    }
}

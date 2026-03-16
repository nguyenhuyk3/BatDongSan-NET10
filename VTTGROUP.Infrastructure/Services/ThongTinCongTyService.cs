using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VTTGROUP.Domain.Model;
using VTTGROUP.Domain.Model.LoaiCanHo;
using VTTGROUP.Domain.Model.ThongTinCongTy;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class ThongTinCongTyService
    {
        private readonly IDbContextFactory<AppDbContext> _factory;
        private readonly ILogger<ThongTinCongTyService> _logger;

        public ThongTinCongTyService(IDbContextFactory<AppDbContext> factory, ILogger<ThongTinCongTyService> logger)
        {
            _factory = factory;
            _logger = logger;
        }
        public async Task<ResultModel> GetThongTinCongTy()
        {
            try
            {
                using var _context = _factory.CreateDbContext();
                var record = new ThongTinCongTyModel();

                var entity = await _context.HtThongTinCongTies.FirstOrDefaultAsync();
                if (entity == null)
                    return ResultModel.Fail($"Không tìm thấy thông tin công ty");

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

                var files = await _context.HtFileDinhKems.Where(d => d.Controller == "ThongTinCongTy" && d.MaPhieu == entity.MaCongTy
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

                return ResultModel.SuccessWithData(record, string.Empty);
            }
            catch (Exception ex)
            {
                return ResultModel.Fail($"Lỗi hệ thống: Không thể lấy thông tin công ty: {ex.Message}");
            }
        }

        public async Task<ResultModel> UpdateAsync(ThongTinCongTyModel model)
        {
            try
            {
                using var context = _factory.CreateDbContext();
                var entity = await context.HtThongTinCongTies.FirstOrDefaultAsync();

                if (entity == null)
                {
                    return ResultModel.Fail("Không tìm thông tin công ty.");
                }

                entity.TenCongTy = model.TenCongTy;
                entity.DiaChiCongTy = model.DiaChiCongTy;
                entity.Fax = model.Fax;
                entity.DienThoai = model.DienThoai;
                entity.MaSoThue = model.MaSoThue;
                entity.TaiKhoan = model.TaiKhoan;
                entity.ChiNhanhNganHang = model.ChiNhanhNganHang;
                entity.DaiDienCongTy = model.DaiDienCongTy;
                entity.ChucVuNguoiDaiDien = model.ChucVuNguoiDaiDien;
                entity.CmndSoNguoiDaiDien = model.CmndSoNguoiDaiDien;
                entity.CmndNgayCapNguoiDd = model.CmndNgayCapNguoiDd;
                entity.CmndNoiCapNguoiDd = model.CmndNoiCapNguoiDd;
                entity.Email = model.Email;
                entity.TenTaiKhoan = model.TenTaiKhoan;
                entity.TenNganHang = model.TenNganHang;
                entity.TenChiNhanh = model.TenChiNhanh;

                List<HtFileDinhKem> listFiles = new List<HtFileDinhKem>();
                var UploadedFiles = await context.HtFileDinhKems.Where(d => d.MaPhieu == model.MaCongTy && d.Controller == "ThongTinCongTy").ToListAsync();

                if (model.Logo != null && model.Logo.Any())
                {
                    foreach (var file in model.Logo)
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
                            MaPhieu = model.MaCongTy,
                            TenFileDinhKem = file.FileName,
                            TenFileDinhKemLuu = savedPath,
                            TaiLieuUrl = savedPath,
                            Controller = "ThongTinCongTy",
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

                await context.SaveChangesAsync();

                return ResultModel.SuccessWithData(entity, $"Cập nhật {entity.TenCongTy} thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[UpdateByIdAsync] Lỗi khi cập nhật dự án");
                return ResultModel.Fail($"Lỗi hệ thống: {ex.Message}");
            }
        }

        private async Task<string> SaveFileWithTickAsync(UploadedFileModel file)
        {
            if (string.IsNullOrEmpty(file.FileName)) return "";

            var absolutePath = Path.Combine(file.FolderUrl, file.FileNameSave);

            // 5. Trả về tên file lưu (để lưu DB)
            return absolutePath.Replace("\\", "/"); // ex: uploads/abc_637xxxx.pdf
        }
    }
}

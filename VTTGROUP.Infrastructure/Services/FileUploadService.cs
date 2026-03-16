using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VTTGROUP.Domain.Model;
using VTTGROUP.Infrastructure.Database;

namespace VTTGROUP.Infrastructure.Services
{
    public class FileUploadService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BlockService> _logger;
        private readonly IConfiguration _config;
        public FileUploadService(AppDbContext context, ILogger<BlockService> logger, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _config = config;
        }

        #region Thêm, xóa, sửa Block
        public async Task<ResultModel> DeleteFileDinhKem(int? id, string fileNameSave)
        {
            try
            {
                var file = await _context.HtFileDinhKems
                        .Where(d => d.Id == id || d.TenFileDinhKemLuu.EndsWith("/" + fileNameSave))
                        .FirstOrDefaultAsync();
                if (file == null)
                    return ResultModel.Fail($"Lỗi hệ thống: Không tìm thấy file: {fileNameSave}");

                _context.HtFileDinhKems.Remove(file);
                await _context.SaveChangesAsync();
                return ResultModel.Success("Xóa file thành công");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[DeleteFileDinhKem] Lỗi khi Xóa file");
                return ResultModel.Fail($"Lỗi hệ thống: Không thể xóa file: {ex.Message}");
            }
        }
        #endregion

    }
}

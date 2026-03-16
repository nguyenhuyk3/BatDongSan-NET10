using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.LaiPhatQuaHan
{
    public interface ILaiPhatQuaHanService
    {
        Task<List<SystemLaiPhatQuaHan>> GetLaiPhatQuaHanAsync(string maDuAn, string maKhachHang, string maCanHo, string maGiaiDoanTT);
    }
}

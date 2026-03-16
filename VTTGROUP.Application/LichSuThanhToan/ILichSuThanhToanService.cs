using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.LichSuThanhToan
{
    public interface ILichSuThanhToanService
    {
        Task<List<SystemLichSuThanhToan>> GetLichSuThanhToanAsync(string maDuAn, string maKhachHang, string maCanHo, string maHopDong, string maGiaiDoanTT);
    }
}

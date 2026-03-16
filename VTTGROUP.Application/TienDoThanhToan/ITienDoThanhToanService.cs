using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.TienDoThanhToan
{
    public interface ITienDoThanhToanService
    {
        Task<List<SystemTienDoThanhToan>> GetTienDoThanhToanAsync(string maDuAn,string maHopDong,string maKhachHang,string QSearch, int? trangThaiThanhToan);
    }
}

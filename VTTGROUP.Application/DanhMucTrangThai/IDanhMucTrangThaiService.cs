using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.DanhMucTrangThai
{
    public interface IDanhMucTrangThaiService
    {
        Task<List<SysDanhMucTrangThai>> GetDanhMucTrangThaiAsync(int? pageIndex, int? numOfPage);
    }
}

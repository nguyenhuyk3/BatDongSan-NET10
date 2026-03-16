using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.KyThanhToan
{
    public interface IKyThanhToanService
    {
        Task<List<SystemKyThanhToan>> GetKyThanhToanAsync(string maDuAn, string maKhachHang, string maHopDong);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.KhachHang
{
    public interface IKhachHangService
    {
        Task<List<SystemKhachHang>> GetKhachHangAsync(string QSearch, char? LoaiHinh,int? Page,int? PageSize);
    }
}

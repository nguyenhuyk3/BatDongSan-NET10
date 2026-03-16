
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTTGROUP.Domain.Entities;

namespace VTTGROUP.Application.FileDinhKem
{
    public interface IFileDinhKemService
    {
        Task<List<SystemFileDinhKem>> GetFileDinhKemAsync(string maDuAn, string maKhachHang, string maHopDong, string maKyTT);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Domain.Entities
{
    public class SystemKyThanhToan
    {
        public string? MaKyTT { get; set; }
        public string? TenKyTT { get; set; }
        public string? MaDuAn { get; set; }
        public int ThuTuHT { get; set; }
        public DateTime? NgayDuKien { get; set; }
        public DateTime? NgayThucHien { get; set; }

    }
}

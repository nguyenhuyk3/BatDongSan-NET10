using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Domain.Entities
{
    public class SystemMenu
    {
        public string MaCongViec { get; set; }
        public string TenCongViec { get; set; }
        public string? TenController { get; set; }
        public string? TenAction { get; set; }
        public string? GhiChu { get; set; }
        public int? DoUuTien { get; set; }
        public string? MaCha { get; set; }
        public string? MaVuViec { get; set; }
        public int SoLuongCongViecCon { get; set; }

        public List<SystemMenu> Children { get; set; } = new();
    }
}

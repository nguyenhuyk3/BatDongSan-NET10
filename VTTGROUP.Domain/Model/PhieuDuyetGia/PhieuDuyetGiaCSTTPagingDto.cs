using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Domain.Model.PhieuDuyetGia
{
    public class PhieuDuyetGiaCSTTPagingDto
    {
        public int STT { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public string MaCSTT  { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}

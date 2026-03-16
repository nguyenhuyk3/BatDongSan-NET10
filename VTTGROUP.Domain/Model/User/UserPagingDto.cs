using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Domain.Model.User
{
    public class UserPagingDto
    {
        public int STT { get; set; }
        public int? ID { get; set; } = null!;
        public string MaNhanVien { get; set; } = null!;
        public string HoVaTen { get; set; } = null!;
        public string TenDangNhap { get; set; } = null!;
        public string UrlDaiDien { get; set; } = null!;
        public List<NhomUserOfUserModel> nhomUsers { get; set; }
        public bool TrangThai { get; set; }
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
    }
}

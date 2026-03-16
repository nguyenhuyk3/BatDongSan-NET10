using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTTGROUP.Domain.Model.User
{
    public class UserModel
    {
        public int Id { get; set; }
        public string TenDangNhap { get; set; } = null!;
        public string MatKhauMoi { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string XacNhanMatKhau { get; set; } = null!;
        public string MaNhanVien { get; set; } = null!;
        public string TenNhanVien { get; set; } = null!;
        public int? IdNhanVien { get; set; }
        public string Email { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public bool TrangThai { get; set; }
        public DateTime NgayLap { get; set; }
        public string? NguoiLap { get; set; }
        public string? LoaiUser { get; set; }
        public string? TenLoaiUser { get; set; }
        public string? NhomUsers { get; set; }
    }
    public class NhomUserOfUserModel
    {
        public int? UserId { get; set; }
        public string? MaNhom { get; set; }
        public string? MaNhanVien { get; set; }
        public bool? IsChecked { get; set; }
        public bool IsCheckedBool
        {
            get => IsChecked == true;
            set => IsChecked = value;
        }
    }
}

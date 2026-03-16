using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.PhanQuyen
{
    public class PhanQuyenModel
    {
    }

    public class NhomUserPagingDto
    {
        public int STT { get; set; }
        public string MaNhomUser { get; set; } = null!;
        public string TenNhomUser { get; set; } = null!;
        public string? GhiChu { get; set; }
        public bool? TrangThai { get; set; }
        public int TotalCount { get; set; }
    }

    public class NhanVienNhomUserPagingDto
    {
        public int STT { get; set; }
        public string MaNhanVien { get; set; } = null!;
        public string TenNhanVien { get; set; } = null!;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public int TotalCount { get; set; }
    }
    public class NhanVienUserPagingDto
    {
        public int STT { get; set; }
        public int? UserId { get; set; }
        public string MaNhanVien { get; set; } = null!;
        public string TenNhanVien { get; set; } = null!;
        public string TenDangNhap { get; set; } = null!;
        public string? Email { get; set; }
        public string? SoDienThoai { get; set; }
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
    }
    public partial class NhomUserModel
    {
        public string MaNhomUser { get; set; } = null!;

        public string? TenNhomUser { get; set; }

        public string? GhiChu { get; set; }

        public bool? TrangThai { get; set; }
        public List<NhanVienModel>? ListNhanVien { get; set; }

    }

    public partial class CongViecNhomUserModel
    {
        public string MaNhomUser { get; set; } = null!;

        public string? TenNhomUser { get; set; }

        public string? GhiChu { get; set; }

        public bool? TrangThai { get; set; }

    }
    public partial class TrangThaiUserModel
    {
        public string MaTrangThai { get; set; } = null!;

        public string? TenTrangThai { get; set; }

    }

}

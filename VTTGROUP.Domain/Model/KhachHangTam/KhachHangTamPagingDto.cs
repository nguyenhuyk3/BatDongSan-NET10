namespace VTTGROUP.Domain.Model.KhachHang
{
    public class KhachHangTamPagingDto
    {
        public int STT { get; set; }
        public string MaKhachHangTam { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string MaDoiTuongKhachHang { get; set; } = string.Empty;
        public string TenDoiTuongKhachHang { get; set; } = string.Empty;
        public string MaLoaiIdCard { get; set; } = string.Empty;
        public string TenLoaiIdCard { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public DateTime NgayCapIdCard { get; set; }
        public string NoiCapIdCard { get; set; } = string.Empty;
        public string DiaChiThuongTru { get; set; } = string.Empty;
        public string DiaChiHienNay { get; set; } = string.Empty;
        public string TenNguonKhach { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaSanGD { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string NguoiDaiDien { get; set; } = string.Empty;
        public string SoDienThoaiNguoiDaiDien { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public string QuocTich { get; set; } = string.Empty;
        public string TenQuocGia { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public int FlagDelete { get; set; }
    }

    public class KhachHangPagingDto
    {
        public int STT { get; set; }
        public string MaKhachHangTam { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string TenDoiTuongKhachHang { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = string.Empty;
        public string TenNhanVien { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public string IDLanDieuChinh { get; set; } = string.Empty;
        public string QuocTich { get; set; } = string.Empty;
        public string TenQuocGia { get; set; } = string.Empty;
        public string DiaChiThuongTru { get; set; } = string.Empty;
        public string DiaChiLienLac { get; set; } = string.Empty;
        public string NguoiDaiDien { get; set; } = string.Empty;
        public string SoDienThoaiDaiDien { get; set; } = string.Empty;
        public DateTime NgayCapIdCard { get; set; }
        public string NoiCapIdCard { get; set; } = string.Empty;
        public string MaLoaiIdCard { get; set; } = string.Empty;
        public string TenLoaiIdCard { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public bool IsSelected { get; set; }
        public int FlagDelete { get; set; }

    }
}

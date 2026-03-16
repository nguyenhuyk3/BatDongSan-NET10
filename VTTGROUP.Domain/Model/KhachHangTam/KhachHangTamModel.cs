using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.KhachHangTam
{
    public class KhachHangTamModel
    {
        public string MaKhachHang { get; set; } = string.Empty;
        public string MaKhachHangTam { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaDoiTuongKhachHang { get; set; } = string.Empty;
        public string TenDoiTuongKhachHang { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? QuocTich { get; set; } = string.Empty;
        public string? TenQuocTich { get; set; } = string.Empty;
        public string? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string MaLoaiIdCard { get; set; } = string.Empty;
        public string TenLoaiIdCard { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public string? NgayCapIdCard { get; set; }
        public string NoiCapIdCard { get; set; } = string.Empty;
        public string DiaChiThuongTru { get; set; } = string.Empty;
        public string DiaChiHienNay { get; set; } = string.Empty;
        public string MaNguonKhach { get; set; } = string.Empty;
        public string TenNguonKhach { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaSanGD { get; set; } = string.Empty;
        public string TenSanGD { get; set; } = string.Empty;
        public string NguoiDaiDien { get; set; } = string.Empty;
        public string SoDienThoaiNguoiDaiDien { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public string IDLanDieuChinh { get; set; } = string.Empty;
        public List<UploadedFileModel> FileAnhs { get; set; } = new List<UploadedFileModel>();
        public List<UploadedFileModel> MatSauFile { get; set; } = new List<UploadedFileModel>();
        public bool IsCheckGioiTinh { get; set; } = false;
        public bool IsCheckNgayCapNoiCapIdCard { get; set; } = false;
        public bool IsHienThiNguoiDaiDien { get; set; } = false;
        public bool IsHienThiNgaySinh { get; set; } = false;
        public bool IsHienThiQuocTich { get; set; } = false;
        public bool IsHienThiDiaChiThuongTru { get; set; } = false;
        public string GhiChu { get; set; } = string.Empty;
        public string? NguoiLienHe { get; set; }
        public string? ChucVuNguoiDaiDien { get; set; }      
    }

    public class KhachHangPhieuDatCoc()
    {
        public string MaKhachHang { get; set; } = string.Empty;
        public string IDKhachHangCT { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? NgaySinh { get; set; }
        public string? NgayCapIdCard { get; set; }
        public string? IdCard { get; set; }
        public string SoDienThoai { get; set; } = string.Empty;
        public string NoiCapIdCard { get; set; } = string.Empty;
        public string DiaChiThuongTru { get; set; } = string.Empty;
        public string DiaChiHienNay { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenCanHo { get; set; } = string.Empty;
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaChinhSachTT { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal TyLeThueVAT { get; set; } = 0;
        public decimal GiaCanHoTruocThue { get; set; } = 0;
        public decimal GiaCanHoSauThue { get; set; } = 0;
        public decimal DonGiaDat { get; set; } = 0;
        public decimal GiaDat { get; set; } = 0;
        public decimal TyLeCK { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public decimal GiaBanTienThue { get; set; } = 0;
        public decimal TyLeQuyBaoTri { get; set; } = 0;
        public decimal TienQuyBaoTri { get; set; } = 0;
        public string NguoiDaiDien { get; set; } = string.Empty;
        public string ChucVuNguoiDaiDien { get; set; } = string.Empty;
        public string NguoiLienHe { get; set; } = string.Empty;
        public string SoDienThoaiNguoiLienHe { get; set; } = string.Empty;
        public string MaDoiTuongKH { get; set; } = string.Empty;
    }
}

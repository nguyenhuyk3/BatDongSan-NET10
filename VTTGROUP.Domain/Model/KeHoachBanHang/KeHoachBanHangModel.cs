using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.KeHoachBanHang
{
    public class KeHoachBanHangModel
    {
        public string MaPhieuKH { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = null!;
        public NguoiLapModel? NguoiLap { get; set; }
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public decimal DonGiaTB { get; set; } = 0;
        public decimal DonGiaTBGoc { get; set; } = 0;
        public decimal DoanhThuDuKien { get; set; } = 0;
        public int MaQuiTrinhDuyet { get; set; } = 0;       
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public decimal? SaiSoDoanhThuChoPhepKHBH { get; set; } = 0;
        public int TongSoLuongCanHo { get; set; } = 0;
    }

    public class DotMoBanCanHoTheoDuAn
    {
        public string MaDot { get; set; } = string.Empty;
        public string TenDot { get; set; } = string.Empty;
        public string MaMau { get; set; } = string.Empty;
        public string MaPhieuKeHoach { get; set; } = string.Empty;
        public int ThuTuHienThi { get; set; } = 1;
    }
    public class SanPhamPopupPaginModel
    {
        public int TotalCount { get; set; } = 0;
        public int STT { get; set; } = 0;
        public string MaSanPham { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string MaBlock { get; set; } = null!;
        public string TenBlock { get; set; } = null!;
        public string MaTang { get; set; } = null!;
        public string TenTang { get; set; } = null!;
        public string MaLoaiCan { get; set; } = null!;
        public string TenLoaiCanHo { get; set; } = null!;
        public string MaLoaiGoc { get; set; } = null!;
        public string TenLoaiGoc { get; set; } = null!;
        public string MaViTri { get; set; } = null!;
        public string TenViTri { get; set; } = null!;
        public string MaView { get; set; } = null!;
        public string TenView { get; set; } = null!;
        public string MaTruc { get; set; } = null!;
        public string TenTruc { get; set; } = null!;
        public string MaKhoi { get; set; } = null!;
        public string TenMatKhoi { get; set; } = null!;
        public float DienTichTimTuong { get; set; } = 0!;
        public float HeSoTuTinh { get; set; } = 0!;
        public bool IsSelected { get; set; }
    }

    public class KeHoachBanHangChiTietCanHo
    {
        public string MaPhieuKH { get; set; } = string.Empty;
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaSanPham { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public decimal HeSoCanHo { get; set; } = 0;
        public decimal DienTichCanHo { get; set; } = 0;
    }

    public class GiaBanTheoDotDto
    {
        public string MaPhieuKH { get; set; } = string.Empty;
        public string MaDot { get; set; } = string.Empty;
        public string MaMau { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public decimal TongDienTichCanHo { get; set; } = 0;
        public int SoCanMoBanTheoDot { get; set; } = 0;
        public decimal DonGia { get; set; } = 0;
        //public decimal DoanhThuDuKien => IsXacNhan ? DonGia * TongDienTichCanHo : 0;
        public decimal DoanhThuDuKien => DonGia * TongDienTichCanHo;
        public bool IsXacNhan { get; set; } = false;
        public bool IsDonGiaDaDuyet { get; set; } = false;
    }
}

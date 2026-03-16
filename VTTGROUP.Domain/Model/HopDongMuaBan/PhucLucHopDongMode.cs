using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.HopDongMuaBan
{
    public class PhucLucHopDongPaginDTO
    {
        public string MaPhuLuc { get; set; } = string.Empty;
        public string SoPhuLuc { get; set; } = string.Empty;
        public string MaHopDong { get; set; } = string.Empty;
        public string SoHopDong { get; set; } = string.Empty;
        public string MaDatCoc { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public DateTime NgayKyPL { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string DotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string MaCSTT { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public decimal GiaBan { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public decimal GiaBanSauCK { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public string NoiDung { get; set; } = string.Empty;
        public string MaQuiTrinhDuyet { get; set; } = string.Empty;
        public string TrangThaiDuyet { get; set; } = string.Empty;
    }
    public class PhucLucHopDongMode
    {
        public string MaPhuLuc { get; set; } = string.Empty;
        public string SoPhuLuc { get; set; } = string.Empty;
        public string MaHopDong { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string NgayKy { get; set; } = string.Empty;
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaChinhSachTT { get; set; } = string.Empty;
        public string TenChinhSachTT { get; set; } = string.Empty;
        public decimal GiaBan { get; set; } = 0;
        public decimal GiaTriCK { get; set; } = 0;
        public decimal GiaBanSauCK { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public string GhiChu { get; set; } = string.Empty;
        public string MaQuiTrinhDuyet { get; set; } = string.Empty;
        public string TrangThaiDuyet { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string MaMauIn { get; set; } = string.Empty;
        public string TenMauIn { get; set; } = string.Empty;

        public HopDongMuaBanModel? HopDong { get; set; }

    }
}

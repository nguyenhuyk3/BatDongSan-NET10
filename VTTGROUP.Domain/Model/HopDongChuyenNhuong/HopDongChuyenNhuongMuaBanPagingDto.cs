namespace VTTGROUP.Domain.Model.HopDongChuyenNhuong
{
    public class HopDongChuyenNhuongMuaBanPagingDto
    {
        public int STT { get; set; }
        public string MaHopDong { get; set; } = string.Empty;
        public string SoHopDong { get; set; } = string.Empty;
        public string MaDatCoc { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string MaChinhSachThanhToan { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public decimal DienTichTimTuong { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal GiaDat { get; set; } = 0;
        public decimal GiaBanTruocThue { get; set; } = 0;
        public decimal TyLeThueVAT { get; set; } = 0;
        public decimal GiaBanTienThue { get; set; } = 0;
        public decimal GiaBanSauThue { get; set; } = 0;
        public decimal GiaTriDaThu { get; set; } = 0;
        public decimal TyLeQuyBaoTri { get; set; } = 0;
        public decimal TienQuyBaoTri { get; set; } = 0;
        public decimal PhiBaoTriDaThanhToan { get; set; } = 0;
        public DateTime NgayKy { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string IDLanDieuChinhKH { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int SoPhuLuc { get; set; }
    }
}

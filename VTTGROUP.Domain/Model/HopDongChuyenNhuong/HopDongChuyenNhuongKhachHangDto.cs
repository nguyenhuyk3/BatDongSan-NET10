namespace VTTGROUP.Domain.Model.HopDongChuyenNhuong
{
    public class HopDongChuyenNhuongKhachHangDto
    {
        public int STT { get; set; }
        public string MaThamChieu { get; set; } = null!;
        public string MaKhachHang { get; set; } = null!;
        public string TenKhachHang { get; set; } = null!;
        public string IdCard { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public bool IsDaiDien { get; set; } = false;
        public string IDLanDieuChinhKH { get; set; } = string.Empty;
        public int VaiTro { get; set; }
        public int VaiTroHienTai { get; set; }
        public string Nguon { get; set; } = string.Empty;
    }
}

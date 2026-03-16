namespace VTTGROUP.Domain.Model.HopDongChuyenNhuong
{
    public class HopDongChuyenNhuongPagingDto
    {
        public int STT { get; set; }
        public string MaChuyenNhuong { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public DateTime NgayChuyenNhuong { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaHopDong { get; set; } = string.Empty;
        public string SoHopDong { get; set; } = string.Empty;
        public string MaSanPham { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string BenChuyen { get; set; } = string.Empty;
        public string BenNhan { get; set; } = string.Empty;
        public string DaiDienChuyen { get; set; } = string.Empty;
        public string DaiDienNhan { get; set; } = string.Empty;
        public decimal GiaTriCanHo { get; set; } = 0;
        public decimal GiaTriDaThanhToan { get; set; } = 0;
        public decimal PhiBaoTri { get; set; } = 0;
        public decimal PhiBaoTriDaTT { get; set; } = 0;
        public string NoiDung { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
    }
}

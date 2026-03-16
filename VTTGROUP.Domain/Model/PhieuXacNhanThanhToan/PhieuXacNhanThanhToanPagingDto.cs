namespace VTTGROUP.Domain.Model.PhieuXacNhanThanhToan
{
    public class PhieuXacNhanThanhToanPagingDto
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public DateTime NgayHachToan { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool IsXacNhan { get; set; } = false;
        public decimal TongTien { get; set; } = 0;
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
    }
}

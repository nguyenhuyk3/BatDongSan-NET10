namespace VTTGROUP.Domain.Model.TongHopBooking
{
    public class TongHopBookingPhieuGiuChoModel
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string SoPhieu { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaKhachHangTam { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string TenDoiTuongKhachHang { get; set; } = string.Empty;
        public string TenLoaiIdCard { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public decimal SoTienGiuCho { get; set; } = 0;
        public string MaSanMoiGioi { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string TenNhanVienMG { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool ISXacNhan { get; set; } = false;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public bool IsSelected { get; set; }
    }
}


namespace VTTGROUP.Domain.Model.ChinhSachBanHang
{ 
    public class ChinhSachBanHangPaginDto
    {
        public int STT { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string HoVaTen { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public decimal DonGiaTB { get; set; }
        public decimal DoanhThuDuKien { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}

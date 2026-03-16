using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.PhieuGiuCho
{
    public class PhieuGiuChoPagingDto
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string SoPhieu { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string DotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaKhachHangTam { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public decimal SoTienGiuCho { get; set; } = 0;
        public string MaSanMoiGioi { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string TenNhanVienMG { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public bool ISXacNhan { get; set; } = false;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public int SoTTBooking { get; set; } = 0;
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public bool IsSelected { get; set; }
        public string MaPhieuTH { get; set; } = string.Empty;
        public int STTMax { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public bool IsMainRow { get; set; } = true;
    }

    public class PhieuGiuChoModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string SoPhieu { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public string DotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaSanMoiGioi { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public decimal SoTienGiuCho { get; set; } = 0;
        public string TenNhanVienMoiGioi { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string MaLoaiThietKe { get; set; } = string.Empty;
        public string TenLoaiCanHo { get; set; } = string.Empty;
        public string MaMatKhoi { get; set; } = string.Empty;
        public string TenMatKhoi { get; set; } = string.Empty;
        public bool ISXacNhan { get; set; } = false;
        public string MaPhieuTH { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
    }
    public class DuAnTheoSanModel
    {
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
    }

    public class DotBanHangBookingDto
    {
        public string MaDotBanHang { get; set; } = string.Empty;
        public string TenDotBanHang { get; set; } = string.Empty;
        public int? SoTTBooking { get; set; }
    }

    public class DotMoBanOptionDto
    {
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
    }

    /// <summary>
    /// Dữ liệu dùng cho popup "Chuyển booking qua đợt bán hàng mới"
    /// </summary>
    public class ChuyenBookingDotBanHangDto
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string? MaDuAn { get; set; }

        /// <summary>Các đợt + STT booking hiện có của phiếu</summary>
        public List<DotBanHangBookingDto> ExistingDots { get; set; } = new();

        /// <summary>Các đợt có thể chọn (đã loại trừ đợt đã tồn tại)</summary>
        public List<DotMoBanOptionDto> AvailableDots { get; set; } = new();
    }
}

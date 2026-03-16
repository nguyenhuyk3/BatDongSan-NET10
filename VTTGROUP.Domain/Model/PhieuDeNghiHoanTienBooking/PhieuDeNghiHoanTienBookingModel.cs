using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.PhieuDeNghiHoanTienBooking
{
    public class PhieuDeNghiHoanTienBookingModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = null!;
        public NguoiLapModel? NguoiLap { get; set; }
        public DateTime? NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaSanGiaoDich { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string? NoiDung { get; set; }
        public List<PhieuDeNghiHoanTienBookingCTModel> ListCT { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
    }

    public class PhieuDeNghiHoanTienBookingCTModel
    {
        public int? Id { get; set; }
        public string? MaPhieu { get; set; }
        public string? MaBooking { get; set; }
        public string? MaPhieuTHThu { get; set; }
        public decimal? SoTien { get; set; }
        public string? GhiChu { get; set; }
        public string? MaKhachHang { get; set; }
        public string? TenKhachHang { get; set; }
        public string? TenDoiTuongKH { get; set; }
        public string? TenLoaiIDCard { get; set; }
        public string? IDCard { get; set; }
        public int STT { get; set; }
        public bool IsSelected { get; set; }
    }

    public class PhieuDeNghiHoanTienTongHopBookingModel
    {
        public int STT { get; set; }
        public string MaPhieuBooking { get; set; } = string.Empty;
        public string MaPhieuTH { get; set; } = string.Empty;
        public string SoPhieu { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaKhachHangTam { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string TenDoiTuongKhachHang { get; set; } = string.Empty;
        public string TenLoaiIdCard { get; set; } = string.Empty;
        public string IdCard { get; set; } = string.Empty;
        public decimal SoTien { get; set; } = 0;
        public string MaSanMoiGioi { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string TenNhanVienMG { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public bool IsSelected { get; set; }
    }
}

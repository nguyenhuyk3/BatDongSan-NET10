using VTTGROUP.Domain.Model.GioHang;
using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.TongHopBooking
{
    public class TongHopBookingModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = null!;
        public NguoiLapModel? NguoiLap { get; set; }
        public DateTime? NgayLap { get; set; }
        public string? NgayThu { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaSanGiaoDich { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string? NoiDung { get; set; }
        public List<TongHopBookingCTModel> ListCT { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
    }
    public class TongHopBookingCTModel
    {
        public int? Id { get; set; }
        public string? MaPhieuTH { get; set; }
        public string? MaBooking { get; set; }
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
}

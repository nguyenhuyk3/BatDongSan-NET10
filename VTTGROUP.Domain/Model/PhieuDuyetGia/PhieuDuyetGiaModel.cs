using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.PhieuDuyetGia
{
    public class PhieuDuyetGiaModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = null!;
        public NguoiLapModel? NguoiLap { get; set; }
        public DateTime? NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaKeHoach { get; set; } = string.Empty;
        public string TenKeHoach { get; set; } = string.Empty;
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public decimal? GiaBanKeHoach { get; set; } = 0;
        public decimal? GiaBanThucTe { get; set; } = 0;
        public decimal? TyLeChuyenDoi { get; set; } = 0;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string? NoiDung { get; set; }
        public List<PhieuDuyetGiaChinhSachThanhToanModel>? ListChinhSachThanhToan { get; set; }
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
    }

    public partial class PhieuDuyetGiaChinhSachThanhToanModel
    {
        public string MaPhieu { get; set; } = null!;
        public string MaCSTT { get; set; } = null!;
        public string TenCSTT { get; set; } = null!;
    }

    public partial class KHDotMoBanModel
    {
        public string MaDotMoBan { get; set; } = null!;
        public string TenDotMoBan { get; set; } = null!;
        public string MaKeHoach { get; set; } = null!;
		public decimal DonGiaTBDot { get; set; } = 0;
		public string MaPhieuDG { get; set; } = null!;
        public decimal GiaBanThucTe { get; set; } = 0;
        public decimal TyLeBanHang { get; set; } = 0;
    }
}

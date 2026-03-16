using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.ChinhSachThanhToan
{
    public class ChinhSachThanhToanModel
    {
        public string MaCSTT { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = null!;
        public NguoiLapModel? NguoiLap { get; set; }
        public DateTime? NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public decimal? TyLeChietKhau { get; set; } = 0;
        public bool? IsXacNhan { get; set; } = false;
        public string? NoiDung { get; set; }
        public List<ChinhSachThanhToanChiTietModel>? ListChinhSachThanhToan { get; set; }
        public List<ChinhSachThanhToanHopDongChiTietModel>? ListChinhSachThanhToanHD { get; set; }
        public List<ChinhSachThanhToanHopDongChiTietCanTruDatCoc>? ListChinhSachHopDongCanTruDatCoc { get; set; }
    }

    public class ChinhSachThanhToanChiTietModel
    {
        public string MaCSTT { get; set; } = string.Empty;
        public int DotTT { get; set; } = 0;
        public string NoiDungTT { get; set; } = string.Empty;
        public string MaKyTT { get; set; } = string.Empty;
        public string TenKyTT { get; set; } = string.Empty;
        public int SoKhoangCachNgay { get; set; } = 0;
        public int DotThamChieu { get; set; } = 0;
        public decimal? TyLeTTDatCoc { get; set; } = 0;
        public decimal? TyLeTTVAT { get; set; } = 0;
        public bool IsCongNoDC { get; set; } = false;
        public bool IsDotThamChieuInvalid { get; set; } = true;
        public bool IsSelected { get; set; }
    }

    public class ChinhSachThanhToanHopDongChiTietModel
    {
        public string MaCSTT { get; set; } = string.Empty;
        public int DotTT { get; set; } = 0;
        public string NoiDungTT { get; set; } = string.Empty;
        public string MaKyTT { get; set; } = string.Empty;
        public string TenKyTT { get; set; } = string.Empty;
        public int SoKhoangCachNgay { get; set; } = 0;
        public int DotThamChieu { get; set; } = 0;
        public decimal? TyLeTTHopDong { get; set; } = 0;
        public decimal? TyLeTTVAT { get; set; } = 0;
        public bool IsCongNoHD { get; set; } = false;
        public bool IsDotThamChieuInvalid { get; set; } = true;
    }

    public class ChinhSachThanhToanHopDongChiTietCanTruDatCoc
    {
        public string MaCSTT { get; set; } = string.Empty;
        public int DotTTHD { get; set; } = 0;
        public string NoiDungTTHD { get; set; } = string.Empty;
        public decimal? TyLeTTHD { get; set; } = 0;
        public int DotTTDC { get; set; } = 0;
        public string NoiDungTTDC { get; set; } = string.Empty;
        public decimal? TyLeTTDC { get; set; } = 0;
    }
}

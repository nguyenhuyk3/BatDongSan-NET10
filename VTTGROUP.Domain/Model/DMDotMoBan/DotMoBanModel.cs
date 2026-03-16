using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.DMDotMoBan
{
    public class DotMoBanModel
    {
        public string? MaDotMoBan { get; set; } = string.Empty;
        public string? TenDotMoBan { get; set; } = string.Empty;
        public string? MaDuAn { get; set; } = string.Empty;
        public string? TenDuAn { get; set; } = string.Empty;
        public int ThuTuHienThi { get; set; }
        public string? MaMau { get; set; } = string.Empty;
        public string NgayBatDau { get; set; } = string.Empty;
        public string NgayKetThuc { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }
    }
}

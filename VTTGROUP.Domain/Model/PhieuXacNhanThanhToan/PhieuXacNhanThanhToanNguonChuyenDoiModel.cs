using System.Security.Cryptography;

namespace VTTGROUP.Domain.Model.PhieuXacNhanThanhToan
{
    public class PhieuXacNhanThanhToanNguonChuyenDoiModel
    {
        public int Stt { get; set; } = 0!;
        public string MaPhieu { get; set; } = null!;
        public DateTime? Ngay { get; set; }  // để tính/format ngoài DB
        public string NoiDung { get; set; } = null!;
        public decimal SoTien { get; set; } = 0;
        public decimal SoTienDaCanTru { get; set; } = 0;
        public decimal SoTienConLai { get; set; } = 0;
        public int TotalCount { get; set; } = 0!;
        public bool IsSelected { get; set; } = false;
    }
    public class PhieuXacNhanThanhToanPhieuCongNoModel
    {
        public int Stt { get; set; } = 0!;
        public int TotalCount { get; set; } = 0!;
        public string MaDuAn { get; set; } = null!;
        public string TenDuAn { get; set; } = null!;
        public string MaPhieuCongNo { get; set; } = null!;
        public string IdChungTu { get; set; } = null!;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string NoiDung { get; set; } = null!;
        public decimal SoTien { get; set; } = 0;
        public decimal SoTienGoc { get; set; } = 0;
        public bool IsSelected { get; set; } = false;
    }
}

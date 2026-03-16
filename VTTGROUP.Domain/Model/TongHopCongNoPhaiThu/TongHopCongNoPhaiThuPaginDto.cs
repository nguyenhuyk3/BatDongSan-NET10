using VTTGROUP.Domain.Model.PhieuXacNhanThanhToan;

namespace VTTGROUP.Domain.Model.TongHopCongNoPhaiThu
{
    public class TongHopCongNoPhaiThuPaginDto
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string DuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaChungTu { get; set; } = string.Empty;
        public string IdChungTu { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public DateTime HanThanhToan { get; set; }
        public decimal SoTien { get; set; } = 0;
        public string MaCongViec { get; set; } = string.Empty;
        public string MaDoiTuong { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
        public decimal SoTienDaThu { get; set; } = 0;
        public bool IsDaThuDu { get; set; } = false;
        public string DanhSachPhieuXNTT { get; set; } = string.Empty;
        public bool IsDaChiDu { get; set; } = false;
        public decimal SoTienDaChi { get; set; } = 0;
    }

    public class XNTTDraftDto
    {
        public string MaDuAn { get; set; } = "";
        public string TenDuAn { get; set; } = "";
        public string LoaiPhieu { get; set; } = "";
        public string MaKhachHang { get; set; } = "";
        public string TenKhachHang { get; set; } = "";
        public List<PhieuXacNhanThanhToanPhieuCongNoModel> ListCN { get; set; } = new();
    }
}

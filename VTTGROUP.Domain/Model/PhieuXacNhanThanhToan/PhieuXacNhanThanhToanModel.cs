using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.PhieuXacNhanThanhToan
{
    public class PhieuXacNhanThanhToanModel
    {
        public string MaPhieu { get; set; } = null!;
        public string MaKhachHang { get; set; } = null!;
        public string TenKhachHang { get; set; } = null!;
        public string MaDuAn { get; set; } = null!;
        public string TenDuAn { get; set; } = null!;
        public DateTime? NgayHachToanDt { get; set; }  // để tính/format ngoài DB
        public string NgayHachToan { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public NguoiLapModel? NguoiLap { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public string SoChungTu { get; set; } = string.Empty;
        public bool IsXacNhan { get; set; } = false;
        public string LoaiPhieu { get; set; } = string.Empty;
        public string MaHinhThucTT { get; set; } = string.Empty;
        public string TenHinhThucTT { get; set; } = string.Empty;
        public bool IsCanTruNguonKhac { get; set; } = false;
        public bool FlagTong { get; set; } = false;
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
    }
}

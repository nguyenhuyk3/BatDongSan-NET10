using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.DMLoaiThietKe
{
    public class LoaiThietKeModel
    {
        public string? MaLoaiThietKe { get; set; }
        public string? TenLoaiThietKe { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? MoTa { get; set; }
        public DateTime? NgayLap { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }
        public string? MaNhanVien { get; set; }
        public string? HinhAnhUrl { get; set; }
        public string? FullDomain { get; set; }
        public bool IsSelected { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public int SoLuongCanHo { get; set; }
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
    }
}

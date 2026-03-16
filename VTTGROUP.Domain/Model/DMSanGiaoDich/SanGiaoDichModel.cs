using VTTGROUP.Domain.Model.DuAn;
using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.DMSanGiaoDich
{
    public class SanGiaoDichModel
    {
        public string MaSanGiaoDich { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string DienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TrangThai { get; set; } =0;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public List<DuAnModel>? ListDuAn { get; set; }
    }
}

namespace VTTGROUP.Domain.Model
{
    public class CongViecModel
    {
        public string? MaCongViec { get; set; }
        public string? TenCongViec { get; set; }
        public string? TenController { get; set; }
        public string? TenAction { get; set; }
        public string? TienTo { get; set; }
        public int? DoUuTien { get; set; }
        public int? TrangThaiHienThi { get; set; }
        public string? GhiChu { get; set; }
        public string? MaCha { get; set; }
        public int? LevelCay { get; set; }
        public string? OrderPath { get; set; }
        public string? TreeRoot { get; set; }
        public int? DoUuTienRoot { get; set; }
        public int? IsCapCon { get; set; }
        public bool? HienThiTrenMenu { get; set; }
        public string? MaVuViec { get; set; }
        public string? TenQuyen { get; set; }
        public List<VuViecOfCongViecModel> vuViecs { get; set; }
        public List<NhomUserOfCongViecModel> nhomUsers { get; set; }

    }
    public class CongViecDuyetModel
    {
        public string? MaCongViec { get; set; }
        public string? TenCongViec { get; set; }
        public string? TenController { get; set; }
        public string? TenAction { get; set; }
        public string? MaPhieu { get; set; }
        public string? NguoiDuyet { get; set; }
        public DateTime? NgayDuyet { get; set; }
        public string? GhiChu { get; set; }
        public bool? IsPhieuBiTraLai { get; set; }
        public bool? IsGap { get; set; }
        public string? TenTrangThai { get; set; }
    }
}

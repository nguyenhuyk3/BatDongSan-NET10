namespace VTTGROUP.Domain.Model.NhanVien
{
    public class NhanVienPagingDto
    {
        public int STT { get; set; }
        public int? ID { get; set; } = null!;
        public string MaNhanVien { get; set; } = null!;
        public string HoVaTen { get; set; } = null!;
        public string? MaPhongBan { get; set; }
        public string? TenPhongBan { get; set; }
        public string? MaChucVu { get; set; }
        public string? TenChucVu { get; set; }
        public string? UrlDaiDien { get; set; }
        public string Email { get; set; } = null!;
        public string SoDienThoai { get; set; } = null!;
        public string LoaiUser { get; set; } = null!;
        public int? UserId { get; set; } = null!;
        public bool? DaTaoUser { get; set; } = null!;
        public int TotalCount { get; set; }
    }
}

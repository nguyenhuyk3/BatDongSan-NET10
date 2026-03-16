namespace VTTGROUP.Domain.Model.DMSanGiaoDich
{
    public class SanGiaoDichPagingDto
    {
        public int STT { get; set; }
        public string MaSanGiaoDich { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string DiaChi { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string HoVaTen { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public string DienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public bool IsSelected { get; set; }
    }
}

namespace VTTGROUP.Domain.Model.ChinhSachThanhToan
{
    public class ChinhSachThanhToanPagingDto
    {
        public int STT { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public string MaCSTT { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public DateTime? NgayLap { get; set; } = null;
        public string HoVaTen { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string TenCSTT { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public decimal TyLeChietKhau { get; set; }
        public bool IsXacNhan { get; set; } = false;
    }
}

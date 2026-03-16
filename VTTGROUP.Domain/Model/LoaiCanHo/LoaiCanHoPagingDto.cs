namespace VTTGROUP.Domain.Model.LoaiCanHo
{
    public class LoaiCanHoPagingDto
    {
        public int STT { get; set; }
        public string MaLoaiCanHo { get; set; } = string.Empty;
        public string TenLoaiCanHo { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public decimal DienTich { get; set; } = 0;
        public decimal DienTichLotLong { get; set; } = 0;
        public decimal DienTichSanVuon { get; set; } = 0;
        public decimal HeSoDienTich { get; set; } = 0;
        public int SoPhongNgu { get; set; } = 0;
        public int SoLuongCanHo { get; set; } = 0;
        public string TenLoaiThietKe { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public bool IsSelected { get; set; }
    }
}

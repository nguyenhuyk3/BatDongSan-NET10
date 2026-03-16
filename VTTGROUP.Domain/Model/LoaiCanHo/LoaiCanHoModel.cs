namespace VTTGROUP.Domain.Model.LoaiCanHo
{
    public class LoaiCanHoModel
    {
        public string? MaLoaiCanHo { get; set; }
        public string? TenLoaiCanHo { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? MaLoaiThietKe { get; set; }
        public string? TenLoaiThietKe { get; set; }
        public decimal DienTich { get; set; }
        public decimal DienTichLotLong { get; set; }
        public decimal DienTichSanVuon { get; set; }
        public int SoPhongNgu { get; set; }
        public decimal HeSoDienTich { get; set; }
        public string? MoTa { get; set; }     
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public int SoLuongCanHo { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
    }

    public class LoaiCanHoImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaThietKe { get; set; }
        public string? MaLoaiCanHo { get; set; }
        public string? TenLoaiCanHo { get; set; }
        public decimal DienTich { get; set; }
        public decimal DienTichLotLong { get; set; }
        public decimal DienTichSanVuon { get; set; }
        public int SoPhongNgu { get; set; }
        public decimal HeSoDienTich { get; set; }
        public string? MoTa { get; set; }
    }
}

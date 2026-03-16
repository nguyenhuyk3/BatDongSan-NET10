namespace VTTGROUP.Domain.Model.ViewMatKhoi
{
    public class ViewMatKhoiModel
    {
        public string? MaMatKhoi { get; set; }
        public string? TenMatKhoi { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public string? MaBlock { get; set; }
        public string? TenBlock { get; set; }
        public decimal HeSoMatKhoi { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public bool IsSelected { get; set; }
        public int SoLuongCanHo { get; set; }
    }

    public class ViewMatKhoiImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaMatKhoi { get; set; }
        public string? TenMatKhoi { get; set; }     
        public decimal? HeSoMatKhoi { get; set; }
        public int RowIndex { get; set; } // <- dòng trong Excel (tính cả header)
    }
}

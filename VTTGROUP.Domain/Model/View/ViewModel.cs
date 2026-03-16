namespace VTTGROUP.Domain.Model.View
{
    public class ViewModel
    {
        public string? MaView { get; set; }
        public string? TenView { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public decimal? HeSoView { get; set; }
        public bool IsSelected { get; set; }
        public int SoLuongVMK { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public int? ThuTuHienThi { get; set; }
    }

    public class ViewImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaView { get; set; }
        public string? TenView { get; set; }
        public decimal? HeSoView { get; set; }
        public int RowIndex { get; set; } // <- dòng trong Excel (tính cả header)
    }
}

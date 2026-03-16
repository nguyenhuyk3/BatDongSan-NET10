namespace VTTGROUP.Domain.Model.Huong
{
    public class HuongModel
    {
        public string? MaHuong { get; set; }
        public string? TenHuong { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public decimal HeSoHuong { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public bool IsSelected { get; set; }
        public int SoLuongVMK { get; set; }
    }

    public class HuongImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaHuong { get; set; }
        public string? TenHuong { get; set; }
        public decimal? HeSoHuong { get; set; }
        public int RowIndex { get; set; } // <- dòng trong Excel (tính cả header)
    }
}

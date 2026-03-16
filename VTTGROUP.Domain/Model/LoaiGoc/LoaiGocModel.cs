namespace VTTGROUP.Domain.Model.LoaiGoc
{
    public class LoaiGocModel
    {
        public string? MaLoaiGoc { get; set; }
        public string? TenLoaiGoc { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public decimal HeSoGoc { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public bool IsSelected { get; set; }
        public int SoLuongVMK { get; set; }
    }

    public class LoaiGocImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaLoaiGoc { get; set; }
        public string? TenLoaiGoc { get; set; }
        public decimal? HeSoGoc { get; set; }
        public int RowIndex { get; set; } // <- dòng trong Excel (tính cả header)
    }
}

namespace VTTGROUP.Domain.Model.LoaiDienTich
{
    public class LoaiDienTichModel
    {
        public string? MaLoai { get; set; }
        public string? TenLoai { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public decimal HeSoLoaiDT { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public bool IsSelected { get; set; }
        public int SoLuongSP { get; set; }
    }

    public class LoaiDienTichImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaLoai { get; set; }
        public string? TenLoai { get; set; }
        public decimal? HeSoLoaiDT { get; set; }
        public int RowIndex { get; set; } // <- dòng trong Excel (tính cả header)
    }
}

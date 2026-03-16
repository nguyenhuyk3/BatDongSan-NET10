namespace VTTGROUP.Domain.Model.ViTri
{
    public class ViTriModel
    {
        public string? MaViTri { get; set; }
        public string? TenViTri { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public decimal HeSoViTri { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public bool IsSelected { get; set; }
        public int SoLuongVMK { get; set; }
    }

    public class ViTriImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaViTri { get; set; }
        public string? TenViTri { get; set; }
        public decimal? HeSoViTri { get; set; }
        public int RowIndex { get; set; } // <- dòng trong Excel (tính cả header)
    }
}

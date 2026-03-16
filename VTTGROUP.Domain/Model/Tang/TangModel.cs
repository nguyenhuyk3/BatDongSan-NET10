namespace VTTGROUP.Domain.Model.Tang
{
    public class TangModel
    {
        public string? MaTang { get; set; }
        public string? TenTang { get; set; }
        public string? MaBlock { get; set; }
        public string? TenBlock { get; set; }
        public decimal HeSo { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public int STTTang { get; set; }
        public bool IsSelected { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public List<UploadedFileModel> HinhAnh { get; set; } = new List<UploadedFileModel>();
        public string HinhAnhUrl { get; set; } = string.Empty;
    }

    public class TangImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaBlock { get; set; }
        public string? TenBlock { get; set; }
        public string? MaTang { get; set; }
        public string? TenTang { get; set; }
        public decimal? HeSoTang { get; set; }
        public int? STTTang { get; set; }
    }
}

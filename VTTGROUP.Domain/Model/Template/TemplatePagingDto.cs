using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.Template
{
    public class TemplatePagingDto
    {
        public int STT { get; set; }
        public string MaTemplate { get; set; } = string.Empty;
        public string SoPhieu { get; set; } = string.Empty;
        public string TenTemplate { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public bool IsSelected { get; set; }
    }

    public class TemplateModel
    {
        public string MaTemplate { get; set; } = string.Empty;
        public string SoPhieu { get; set; } = string.Empty;
        public string TenTemplate { get; set; } = string.Empty;
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();

    }
}

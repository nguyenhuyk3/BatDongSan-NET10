using System.ComponentModel.DataAnnotations;

namespace VTTGROUP.Domain.Model.Block
{
    public class BlockModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã block")]
        public string? MaBlock { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập tên block")]
        public string? TenBlock { get; set; }
        public string? MaDuAn { get; set; }
        public string? TenDuAn { get; set; }
        public bool IsSelected { get; set; }
        public int SoLuongTang { get; set; }
        public int SoLuongTruc { get; set; }
        // ✅ Dòng mới thêm trên UI
        public bool IsNew { get; set; }
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
    }

    public class BlockImportModel
    {
        public string? MaDuAn { get; set; }
        public string? MaBlock { get; set; }
        public string? TenBlock { get; set; }
    }
}

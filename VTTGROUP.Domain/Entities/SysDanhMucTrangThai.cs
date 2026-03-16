namespace VTTGROUP.Domain.Entities
{
    public class SysDanhMucTrangThai
    {
        public int? StatusCode { get; set; }//Mã trạng thái
        public string? StatusNameVi { get; set; }//Tên trạng thái VN
        public string? StatusNameEn { get; set; }//Tên trạng thái EN
        public string? StatusColor { get; set; }// Mã màu trạng thái
        public string? StatusBgColor { get; set; }//Mã màu nền
        public string? StatusTextColor { get; set; }//Mã màu chữ trạng thái
    }
}

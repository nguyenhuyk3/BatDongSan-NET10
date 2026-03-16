using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.MauIn
{
    public class MauInPagingDto
    {
        public int STT { get; set; }
        public string MaMauIn { get; set; } = string.Empty;
        public string TenMauIn { get; set; } = string.Empty;
        public string LoaiMauIn { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string TenLoaiMauIn { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
    }

    public class MauInModel
    {
        public string MaMauIn { get; set; } = string.Empty;
        public string TenMauIn { get; set; } = string.Empty;
        public string LoaiMauIn { get; set; } = string.Empty;
        public NguoiLapModel? NguoiLap { get; set; }
        public string MaNhanVien { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string TenLoaiMauIn { get; set; } = string.Empty;
        public string GhiChu { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
    }
}

using VTTGROUP.Domain.Model.DuAn;
using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.QuyTrinhDuyet
{
    public class QuyTrinhDuyetModel
    {
        public int Id { get; set; }
        public string? TenQuyTrinh { get; set; }
        public string? MaCongViec { get; set; }
        public string? TenCongViec { get; set; }
        public int? TrangThai { get; set; }
        public DateTime NgayLap { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }
        public string? MaNhanVien { get; set; }
        public string? GhiChu { get; set; }
        public List<DuAnModel>? ListDuAn { get; set; }
        public List<QuyTrinhDuyetBuocDuyetModel>? ListBuocDuyet { get; set; }
    }
    public class QuyTrinhDuyetBuocDuyetModel
    {
        public BuocDuyetModel? BuocDuyet { get; set; }
        public NguoiLapModel? NguoiDuyet { get; set; }
        public int? ThuTu { get; set; }
    }

    public class QuyTrinhDuyetPagingDto
    {
        public int STT { get; set; }
        public int Id { get; set; }
        public string MaCongViec { get; set; } = string.Empty;
        public string TenQuyTrinh { get; set; } = string.Empty;
        public string TrangThai { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string HoVaTen { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
    }

}

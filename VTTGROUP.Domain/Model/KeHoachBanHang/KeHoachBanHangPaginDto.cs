namespace VTTGROUP.Domain.Model.KeHoachBanHang
{
    public class KeHoachBanHangPaginDto
    {
        public int STT { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public string MaPhieuKH { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string HoVaTen { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public decimal DonGiaTB { get; set; }
        public decimal DoanhThuDuKien { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }

    public class QuiTrinhDuyetDto
    {
        public int IdQuyTrinh { get; set; }
        public string TenQuyTrinh { get; set; }
        public string QuyTrinhDuyetText { get; set; }
    }

    public class ThongTinPhieuDuyetModel
    {
        public int ID { get; set; }
        public string MaCongViec { get; set; }
        public string MaPhieu { get; set; }
        public string MaNhanVien { get; set; }
        public int TrangThai { get; set; }
        public DateTime NgayDuyet { get; set; }
        public int TrangThaiTraLai { get; set; }
        public string NoiDung { get; set; }
    }

    public class Duyet
    {
        public string MaNhanVien { get; set; }
        public string MaBuocDuyet { get; set; }
        public string MaCapDuyetCuoi { get; set; }
    }

    public class LichSuDuyetPhieuDto
    {
        public string TenTrangThai { get; set; }
        public string TenNhanVien { get; set; }
        public DateTime NgayDuyet { get; set; }
        public string NoiDung { get; set; }
        public int? TrangThaiTraLai { get; set; }
    }
}

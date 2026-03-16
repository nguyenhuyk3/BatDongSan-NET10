namespace VTTGROUP.Domain.Model.GioHang
{
    public class GioHangPagingDto
    {
        public int STT { get; set; }
        public int TotalCount { get; set; }
        public int RowNum { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; } = DateTime.Now;
        public string HoVaTen { get; set; } = string.Empty;
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaSanGiaoDich { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public decimal GiaBan { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public string MaPhieuKH { get; set; } = string.Empty;
        public string MaPhieuDuyetGia { get; set; } = string.Empty;
        public bool? LoaiGioHang { get; set; }
        public bool IsDong { get; set; } = false;
        public DateTime? NgayDong { get; set; } = DateTime.Now;
        public string NguoiDong { get; set; } = string.Empty;
        public string MaSoGioHang { get; set; } = string.Empty;
        public int TongSoCanTrongGio { get; set; } = 0;
        public bool IsSelected { get; set; }
    }

    public class LichSuGioHangDTO
    {
        public string? MaPhieu { get; set; }
        public bool? LoaiGioHangCu { get; set; }
        public bool? LoaiGioHangMoi { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public string? NguoiCapNhat { get; set; }
        public string? MaSanGiaoDichCu { get; set; }
        public string? TenSanGiaoDichCu { get; set; }
        public string? MaSanGiaoDichMoi { get; set; }
        public string? TenSanGiaoDichMoi { get; set; }
    }
}

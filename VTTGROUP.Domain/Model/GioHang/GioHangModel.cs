using VTTGROUP.Domain.Model.NhanVien;
using VTTGROUP.Domain.Model.PhieuDuyetGia;

namespace VTTGROUP.Domain.Model.GioHang
{
    public class GioHangModel
    {
        public string MaPhieu { get; set; } = string.Empty;
        public string MaSoGioHang { get; set; } = string.Empty;
        public string MaNhanVien { get; set; } = null!;
        public NguoiLapModel? NguoiLap { get; set; }
        public DateTime? NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaSanGiaoDich { get; set; } = string.Empty;
        public string TenSanGiaoDich { get; set; } = string.Empty;
        public bool? LoaiGioHang { get; set; }
        public string MaDotMoBan { get; set; } = string.Empty;
        public string TenDotMoBan { get; set; } = string.Empty;
        public decimal? GiaBan { get; set; } = 0;
        public string? NoiDung { get; set; }
        public string? MaPhieuDuyetGia { get; set; }
        public string? MaPhieuKH { get; set; }
        public List<GioHangCanHoModel> ListCanHo { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public bool IsDong { get; set; } = false;
        public DateTime? NgayDong { get; set; } = DateTime.Now;
        public string NguoiDong { get; set; } = string.Empty;
    }
    public class GioHangItem
    {
        public string? MaGioHang { get; set; }
        public string? MaSanPham { get; set; }
        public string? TenSanPham { get; set; }
    }
    public class GioHangCanHoModel
    {
        public int? Id { get; set; }
        public string? MaCanHo { get; set; }
        public string? TenCanHo { get; set; }
        public decimal? HeSoCanHo { get; set; }
        public decimal? DienTichCanHo { get; set; }
        public decimal? DienTichPhanBo { get; set; }
        public decimal? GiaBan { get; set; }
        public decimal? GiaBanSauPhanBo { get; set; }
        public string? MaBlock { get; set; }
        public string? MaTang { get; set; }
        public int TotalCount { get; set; }
        public int STT { get; set; }
        public bool IsSelected { get; set; }
    }
}

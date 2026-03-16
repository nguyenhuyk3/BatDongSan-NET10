using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.ThanhLyHopDong
{
    public class ThanhLyHopDongModel
    {
        public string MaPhieu { get; set; } = null!;
        public string SoPhieuThanhLy { get; set; } = string.Empty;
        public string MaHopDong { get; set; } = null!;
        public string MaKhachHang { get; set; } = null!;
        public string TenKhachHang { get; set; } = null!;
        public string MaCanHo { get; set; } = null!;
        public string TenCanHo { get; set; } = null!;
        public string MaDuAn { get; set; } = null!;
        public string TenDuAn { get; set; } = null!;
        public string? NgayThanhLy { get; set; }
        public DateTime NgayLap { get; set; }
        public NguoiLapModel? NguoiLap { get; set; }
        public string NguoiThanhLy { get; set; } = null!;
        public string? LyDoThanhLy { get; set; }
        public string HinhThucThanhLy { get; set; } = null!;
        public string TenHinhThucThanhLy { get; set; } = null!;
        public string TrangThai { get; set; } = null!;
        public string? GhiChu { get; set; }
        public decimal? GiaBan { get; set; } = null!;
        public decimal SoTienDaThu { get; set; }
        public decimal SoTienPhiBaoTriDaThu { get; set; }
        public decimal TyLeViPham { get; set; }
        public decimal SoTienViPhamHopDong { get; set; }
        public decimal SoTienHoanTra { get; set; }
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;

    }

    public class ThanhLyHopDongPagingDto
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string SoPhieuThanhLy { get; set; } = string.Empty;
        public string SoHopDong { get; set; } = string.Empty;
        public string NguoiThanhLy { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public decimal GiaTriHopDong { get; set; } = 0;
        public decimal SoTienDaThu { get; set; } = 0;
        public decimal SoTienPhiBaoTriDaThu { get; set; } = 0;
        public decimal TyLeViPham { get; set; } = 0;
        public decimal SoTienViPhamHopDong { get; set; } = 0;
        public decimal SoTienHoanTra { get; set; } = 0;
        public DateTime NgayThanhLy { get; set; }
        public string GhiChu { get; set; } = string.Empty;
        public string TenHTTL { get; set; } = string.Empty;
        public string LyDoThanhLy { get; set; } = string.Empty;
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaBuocDuyet { get; set; } = string.Empty;
        public string NguoiDuyet { get; set; } = string.Empty;
        public int ThuTu { get; set; } = 0;
        public string MaTrangThai { get; set; } = string.Empty;
        public string TenTrangThai { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Mau { get; set; } = string.Empty;
        public int TotalCount { get; set; }
    }

    public class HopDongChuaLenThanhLyModel
    {
        public string MaHopDong { get; set; }
        public string SoHopDong { get; set; }
        public string MaCanHo { get; set; }
        public string TenSanPham { get; set; }
        public string MaKhachHang { get; set; }
        public string TenKhachHang { get; set; }
        public decimal GiaBanTruocThue { get; set; }
        public decimal GiaBanTienThue { get; set; }
        public decimal GiaBanSauThue { get; set; }
        public decimal SoTienDaThu { get; set; }
    }
}

using VTTGROUP.Domain.Model.NhanVien;

namespace VTTGROUP.Domain.Model.ThanhLyDatCoc
{
    public class ThanhLyDatCocModel
    {
        public string MaPhieu { get; set; } = null!;
        public string SoPhieuThanhLy { get; set; } = string.Empty;
        public string MaPhieuDatCoc { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = null!;
        public string TenCanHo { get; set; } = null!;
        public decimal? GiaBan { get; set; } = null!;
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
        public decimal PhiPhat { get; set; }
        public decimal SoTienHoanTra { get; set; }
        public decimal? SoTienDaThanhToan { get; set; }
        public int MaQuiTrinhDuyet { get; set; } = 0;
        public int TrangThaiDuyet { get; set; } = 0;
        public string MaNhanVienDP { get; set; } = string.Empty;
        public int TrangThaiDuyetCuoi { get; set; } = 0;
        public bool FlagTong { get; set; } = false;
        public List<UploadedFileModel> Files { get; set; } = new List<UploadedFileModel>();
        public string FlagDK { get; set; } = string.Empty;
    }

    public class ThanhLyDatCocPagingDto
    {
        public int STT { get; set; }
        public string MaPhieu { get; set; } = string.Empty;
        public string SoPhieuThanhLy { get; set; } = string.Empty;
        public string MaPhieuDC { get; set; } = string.Empty;
        public string MaPhieuDangKy { get; set; } = string.Empty;
        public string MaPhieuDatCocKyLai { get; set; } = string.Empty;
        public string SoPhieuDC { get; set; } = string.Empty;
        public string NguoiLap { get; set; } = string.Empty;
        public string HoVaTen { get; set; } = string.Empty;
        public DateTime NgayLap { get; set; }
        public string MaDuAn { get; set; } = string.Empty;
        public string TenDuAn { get; set; } = string.Empty;
        public string MaKhachHang { get; set; } = string.Empty;
        public string TenKhachHang { get; set; } = string.Empty;
        public string MaCanHo { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public decimal SoTienHoanTra { get; set; } = 0;
        public decimal PhiPhat { get; set; } = 0;
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
        public bool IsSelected { get; set; }
    }

    public class PhieuTLDCTienDoThanhToanModel
    {
        public string MaPhieuDC { get; set; } = string.Empty;
        public string MaCSTT { get; set; } = string.Empty;
        public int DotTT { get; set; } = 0;
        public string NoiDungTT { get; set; } = string.Empty;
        public string MaKyTT { get; set; } = string.Empty;
        public string TenKyTT { get; set; } = string.Empty;
        public decimal SoTienThanhToan { get; set; } = 0;
        public decimal SoTienDaTT { get; set; } = 0;
        public decimal SoTienConLai { get; set; } = 0;
    }
}
